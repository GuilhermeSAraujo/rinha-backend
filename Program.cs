using Npgsql;
using RinhaDeBackend.Models;
using RinhaDeBackend.UseCases.CriarPessoa;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var DB_CONNECTION_STRING = "Host=localhost;Username=postgres;Password=admin;Database=postgres";

builder.Services.AddNpgsqlDataSource(
        DB_CONNECTION_STRING ??
        "ERRO de connection string!!!");

var connection = new NpgsqlConnection(DB_CONNECTION_STRING);

builder.Services.AddSingleton(connection);
builder.Services.AddSingleton<ICriarPessoa, CriarPessoa>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
