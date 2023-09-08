using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using RinhaDeBackend;
using RinhaDeBackend.Services;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddNpgsqlDataSource(
    "Host=db;Username=admin;Password=123;Database=rinha",
    dataSourceBuilderAction: a => { a.UseLoggerFactory(NullLoggerFactory.Instance); });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});

builder.Services.AddSingleton(_ => new ConcurrentDictionary<string, byte>());
builder.Services.AddSingleton(_ => new ConcurrentDictionary<Guid, Pessoa>());
builder.Services.AddSingleton(_ => new ConcurrentQueue<Pessoa>());

builder.Services.AddScoped<IPessoaService, PessoaService>();

builder.Services.AddOutputCache();

var app = builder.Build();
app.UseOutputCache();

var UnprocessableEntity = Results.Text(ResponseCriacao.DuplicatedResultString, contentType: "application/json; charset=utf-8", statusCode: 422);
var BadRequestEntity = Results.Text("Bad Request", contentType: "application/json; charset=utf-8", statusCode: 400);
var ResponseAfeStringResponse = Results.Text(ResponseCriacao.ResponseAfeString, contentType: "application/json; charset=utf-8", statusCode: 422);
var optionsPersonCache = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

app.MapPost("/pessoas",
async (
    HttpContext http,
    IDistributedCache distributedCache,
    ConcurrentDictionary<string, byte> peopleByApelidoLocalCache,
    ConcurrentDictionary<Guid, Pessoa> peopleByIdLocalCache,
    ConcurrentQueue<Pessoa> waitingForCreation,
    IPessoaService pessoaService,
    Pessoa pessoa) =>
{
    if (!Pessoa.HasInvalidBody(pessoa))
    {
        return Results.UnprocessableEntity();
    }

    if (peopleByApelidoLocalCache.TryGetValue(pessoa.Apelido, out _))
    {
        return Results.UnprocessableEntity();
    }

    var personOnRedis = await distributedCache.GetAsync(pessoa.Apelido);
    if (personOnRedis is not null)
    {
        return UnprocessableEntity;
    }

    pessoa.Id = Guid.NewGuid();

    waitingForCreation.Enqueue(pessoa);
    if (waitingForCreation.Count > 100)
    {
        await pessoaService.CriarPessoa(waitingForCreation);
    }

    peopleByApelidoLocalCache.TryAdd(pessoa.Apelido, default);
    peopleByIdLocalCache.TryAdd((Guid)pessoa.Id, pessoa);

    var tasks = new[]
    {
        distributedCache.SetAsync(pessoa.Id.ToString(), JsonSerializer.SerializeToUtf8Bytes(pessoa), optionsPersonCache),
        distributedCache.SetAsync(pessoa.Apelido, JsonSerializer.SerializeToUtf8Bytes(pessoa), optionsPersonCache)
    };
    await Task.WhenAll(tasks);

    http.Response.Headers.Location = $"/pessoas/{pessoa.Id}";
    http.Response.StatusCode = 201;

    return Results.Json(new ResponseCriacao { Pessoa = pessoa }, ResponseCriacaoContext.Default.ResponseCriacao);
});

app.MapGet("/pessoas/{id}",
async (
    HttpContext http,
    IDistributedCache distributedCache,
    ConcurrentDictionary<Guid, Pessoa> peopleByIdLocalCache,
    IPessoaService pessoaService,
    Guid id) =>
{
    peopleByIdLocalCache.TryGetValue(id, out Pessoa? personLocalCache);
    if (personLocalCache is not null)
    {
        http.Response.StatusCode = 200;
        return Results.Json(personLocalCache);
    }

    var personOnRedis = await distributedCache.GetAsync(id.ToString());
    if (personOnRedis is not null)
    {
        var personAsString = Encoding.UTF8.GetString(personOnRedis);
        http.Response.StatusCode = 200;
        return Results.Json(JsonSerializer.Deserialize<Pessoa>(personAsString));
    }


    return Results.NotFound();
}).CacheOutput(x => x.VaryByValue(varyBy: httpContext => new KeyValuePair<string, string>("id", httpContext.Request.RouteValues["id"].ToString())));

app.MapGet("/pessoas",
async (
    HttpContext http,
    IPessoaService pessoaService,
    string t) =>
{

    if (string.IsNullOrEmpty(t))
    {
        http.Response.StatusCode = 400;
        return Results.Json(t);
    }

    var pessoas = await pessoaService.BuscarTermo(t);

    http.Response.StatusCode = 200;
    return Results.Json(pessoas);
});

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