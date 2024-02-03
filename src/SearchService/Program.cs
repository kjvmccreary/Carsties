

using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(
    builder.Configuration.GetConnectionString("MongoDbConnection")));

await DB.Index<Item>()
    .Key(c => c.Make, KeyType.Text)
    .Key(c => c.Model, KeyType.Text)
    .Key(c => c.Color, KeyType.Text)
    .CreateAsync();

app.Run();