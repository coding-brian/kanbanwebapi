using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Ū���s�u�r��
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ���U IDbConnection�A�����C���ШD�ɳ��|���ͷs���s�u
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();