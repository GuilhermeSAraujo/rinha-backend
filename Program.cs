using Microsoft.Extensions.Logging.Abstractions;
using RinhaDeBackend;
using RinhaDeBackend.Services;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.


builder.Services.AddNpgsqlDataSource(
    "Host=localhost;Username=postgres;Password=admin;Database=postgres",
    dataSourceBuilderAction: a => { a.UseLoggerFactory(NullLoggerFactory.Instance); });

builder.Services.AddSingleton(_ => new ConcurrentDictionary<string, Pessoa>());
builder.Services.AddScoped<IPessoaService, PessoaService>();

builder.Services.AddOutputCache();

var app = builder.Build();
app.UseOutputCache();

var UnprocessableEntity = Results.Text(ResponseCriacao.DuplicatedResultString, contentType: "application/json; charset=utf-8", statusCode: 422);
var BadRequestEntity = Results.Text("Bad Request", contentType: "application/json; charset=utf-8", statusCode: 400);
var ResponseAfeStringResponse = Results.Text(ResponseCriacao.ResponseAfeString, contentType: "application/json; charset=utf-8", statusCode: 422);

app.MapPost("/pessoas", async (HttpContext http, ConcurrentDictionary<string, Pessoa> pessoasAdicionadas, IPessoaService pessoaService, Pessoa pessoa) =>
{

    if (!Pessoa.BasicamenteValida(pessoa) || pessoasAdicionadas.TryGetValue(pessoa.Nome, out _))
        return UnprocessableEntity;
    

    if(Pessoa.PossuiValoresInvalidos(pessoa))
        return BadRequestEntity;

    pessoa.Id = Guid.NewGuid();
    var pessoaCriada = await pessoaService.CriarPessoa(pessoa);
    pessoasAdicionadas.TryAdd(pessoa.Nome, pessoaCriada);

    http.Response.Headers.Location = $"/pessoas/{pessoaCriada.Id}";
    http.Response.StatusCode = 201;

    return Results.Json(new ResponseCriacao { Pessoa = pessoaCriada }, ResponseCriacaoContext.Default.ResponseCriacao);
});

app.MapGet("/pessoas/{id}", async (HttpContext http, ConcurrentDictionary<string, Pessoa> pessoasAdicionadas, IPessoaService pessoaService, Guid id) =>
{
    var p = await pessoaService.BuscarPessoa(id);

    if(p is null)
    {
        http.Response.StatusCode = 404;
        return Results.Json(p);
    }

    return Results.Json(p);

});

app.MapGet("/pessoas", async (HttpContext http, ConcurrentDictionary<string, Pessoa> pessoasAdicionadas, IPessoaService pessoaService, string t) =>
{
    if (string.IsNullOrEmpty(t))
    {
        http.Response.StatusCode = 400;
        return Results.Json(t);
    }

    var pessoas = await pessoaService.BuscarTermo(t);

    return Results.Json(pessoas);
});

app.Run();
