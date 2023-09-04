using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using NATS.Client.Hosting;
using Npgsql;
using RinhaDeBackend;
using RinhaDeBackend.Services;
using System.Collections.Concurrent;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNats(1, configureOptions: options => NatsOptions.Default with { Url = "nats:4222" });

builder.Services.AddNpgsqlDataSource(
    "Host=db;Username=admin;Password=123;Database=rinha;Connection Pruning Interval=1;Connection Idle Lifetime=2;Enlist=false;No Reset On Close=true",
    dataSourceBuilderAction: a => { a.UseLoggerFactory(NullLoggerFactory.Instance); });

builder.Services.AddSingleton(_ => new ConcurrentDictionary<string, byte>()); // apelido created cache
builder.Services.AddSingleton(_ => new ConcurrentDictionary<Guid, Pessoa>()); // id cache route
builder.Services.AddSingleton(_ => new ConcurrentDictionary<string, Pessoa>()); // termo cache

builder.Services.AddSingleton(_ => Channel.CreateUnbounded<Pessoa>(new UnboundedChannelOptions { SingleReader = true })); // channel to create users "queue"

var natsDestination = "Creation";

builder.Services.AddHostedService<PersonInsertService>();
builder.Services.AddSingleton<IHostedService, PeopleSyncService>();

builder.Services.AddOutputCache();

var app = builder.Build();
app.UseOutputCache();

var UnprocessableEntity = Results.Text(ResponseCriacao.DuplicatedResultString, contentType: "application/json; charset=utf-8", statusCode: 422);
var BadRequestEntity = Results.Text("Bad Request", contentType: "application/json; charset=utf-8", statusCode: 400);
var ResponseAfeStringResponse = Results.Text(ResponseCriacao.ResponseAfeString, contentType: "application/json; charset=utf-8", statusCode: 422);

app.MapPost("/pessoas",
async (
    HttpContext http,
    ConcurrentDictionary<string, byte> peopleByApelidoCache,
    ConcurrentDictionary<Guid, Pessoa> peopleByIdCache,
    Channel<Pessoa> createPersonChannel,
    INatsConnection natsConnection,
    Pessoa pessoa) =>
{
    if (Pessoa.HasInvalidBody(pessoa) || !peopleByApelidoCache.TryAdd(pessoa.Apelido, default))
    {
        return UnprocessableEntity;
    }

    if (Pessoa.IsBadRequest(pessoa))
    {
        return BadRequestEntity;
    }

    pessoa.Id = Guid.NewGuid();

    var tasks = new Task[]
    {
        natsConnection.PublishAsync(natsDestination, pessoa).AsTask(), // public for sync images
        createPersonChannel.Writer.WriteAsync(pessoa).AsTask() // write for saving on db
    };
    await Task.WhenAll(tasks);

    peopleByApelidoCache.TryAdd(pessoa.Apelido, default);
    peopleByIdCache.TryAdd((Guid)pessoa.Id, pessoa);

    http.Response.Headers.Location = $"/pessoas/{pessoa.Id}";
    http.Response.StatusCode = 201;

    return Results.Json(new ResponseCriacao { Pessoa = pessoa }, ResponseCriacaoContext.Default.ResponseCriacao);
});

app.MapGet("/pessoas/{id}",
async (
    HttpContext http,
    ConcurrentDictionary<Guid, Pessoa> peopleByIdLocalCache,
    Guid id) =>
{
    peopleByIdLocalCache.TryGetValue(id, out var personLocalCache);
    if (personLocalCache is not null)
    {
        http.Response.StatusCode = 200;
        return Results.Json(personLocalCache);
    }
    await Task.Delay(10);

    peopleByIdLocalCache.TryGetValue(id, out personLocalCache);
    if (personLocalCache is not null)
    {
        http.Response.StatusCode = 200;
        return Results.Json(personLocalCache);
    }
    await Task.Delay(10);

    peopleByIdLocalCache.TryGetValue(id, out personLocalCache);
    if (personLocalCache is not null)
    {
        http.Response.StatusCode = 200;
        return Results.Json(personLocalCache);
    }


    return Results.NotFound();
}).CacheOutput(x => x.VaryByValue(varyBy: httpContext => new KeyValuePair<string, string>("id", httpContext.Request.RouteValues["id"].ToString())));

app.MapGet("/pessoas",
async (
    HttpContext http,
    ConcurrentDictionary<string, Pessoa> termoPessoaCache,
    string t) =>
{

    if (string.IsNullOrEmpty(t))
    {
        http.Response.StatusCode = 400;
        return Results.Json(t);
    }

    var people = termoPessoaCache.Where(p => p.Key.Contains(t))
                                 .Take(50)
                                 .Select(p => p.Value);

    http.Response.StatusCode = 200;
    return Results.Json(people);
}).CacheOutput(c => c.SetVaryByQuery("t").Expire(TimeSpan.FromMinutes(1)));

app.MapGet("/contagem-pessoas", async (NpgsqlConnection conn) =>
{
    await using (conn)
    {
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "select count(*) from pessoas";
        return await cmd.ExecuteScalarAsync();
    }
});

app.Run();