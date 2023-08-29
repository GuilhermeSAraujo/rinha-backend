using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Data;
using RinhaDeBackend.Models;
using RinhaDeBackend.Services;
using System.Collections.Concurrent;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.


builder.Services.AddNpgsqlDataSource(
    "Host=localhost;Username=postgres;Password=admin;Database=postgres" ??
        "ERRO de connection string!!!", dataSourceBuilderAction: a => { a.UseLoggerFactory(NullLoggerFactory.Instance); });

builder.Services.AddSingleton(_ => new ConcurrentDictionary<string, Pessoa>());
builder.Services.AddScoped<IPessoaService, PessoaService>();

builder.Services.AddOutputCache();

var app = builder.Build();
app.UseOutputCache();

var UnprocessableEntity = Results.Text(ResponseCriacao.DuplicatedResultString, contentType: "application/json; charset=utf-8", statusCode: 422);
var ResponseAfeStringResponse = Results.Text(ResponseCriacao.ResponseAfeString, contentType: "application/json; charset=utf-8", statusCode: 422);

app.Map("/pessoas", async (HttpContext http, ConcurrentDictionary<string, Pessoa> pessoasAdicionadas, IPessoaService pessoaService, CriarPessoaRequest pessoa) =>
{
    var pessoaValida = pessoa.ValidarRequest();

    if (pessoaValida && !pessoasAdicionadas.TryAdd(pessoa.Nome, CriarPessoaRequest.ParsePessoa(pessoa)))
    {
        return UnprocessableEntity;
    }

    if (!pessoaValida)
    {
        return UnprocessableEntity;
    }

    var pessoaCriada = await pessoaService.CriarPessoa(pessoa);
    pessoasAdicionadas.TryAdd(pessoa.Nome, pessoaCriada);

    http.Response.Headers.Location = $"/pessoas/{pessoaCriada.Id}";
    http.Response.StatusCode = 201;

    return Results.Json(new ResponseCriacao { Pessoa = pessoaCriada }, ResponseCriacaoContext.Default.ResponseCriacao);
});

app.Run();
