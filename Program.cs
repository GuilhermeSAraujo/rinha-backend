using Newtonsoft.Json.Serialization;
using Npgsql;
using RinhaDeBackend.Data;
using RinhaDeBackend.Models;
using RinhaDeBackend.Services;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

//Enable CORS
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IPessoaService, PessoaService>();

builder.Services.AddDbContext<DataContext>();

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
