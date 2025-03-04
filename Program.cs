using KanbanWebApi;
using KanbanWebApi.Repository;
using KanbanWebApi.Service;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Ū���s�u�r��
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ���U IDbConnection�A�����C���ШD�ɳ��|���ͷs���s�u
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

builder.Services.AddScoped<BoardService>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));

// ���U Mapper
builder.Services.AddAutoMapper(typeof(Program));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var corsPolicyName = "MyCorsPolicy";
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy(corsPolicyName, policy =>
        {
            policy.AllowAnyOrigin() // ���\���ӷ�
                  .AllowAnyHeader() // ���\�����Y
                  .AllowAnyMethod(); // ���\�� HTTP ��k (GET, POST, PUT, DELETE ��)
        });
    }
    else
    {
        var cors = builder.Configuration.GetSection("Cors").Get<string[]>();

        options.AddPolicy(corsPolicyName, policy =>
        {
            policy.WithOrigins(cors) // ���\���ӷ�
                  .AllowAnyHeader() // ���\�����Y
                  .AllowAnyMethod() // ���\�� HTTP ��k (GET, POST, PUT, DELETE ��)
                  .AllowCredentials(); // �p�G�ݭn��a Cookie �ξ���
        });
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthorization();

app.MapControllers();

app.Run();