using KanbanWebApi;
using KanbanWebApi.Repository;
using KanbanWebApi.Service;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// 讀取連線字串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 註冊 IDbConnection，讓它每次請求時都會產生新的連線
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

builder.Services.AddScoped<BoardService>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));

// 註冊 Mapper
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
            policy.AllowAnyOrigin() // 允許的來源
                  .AllowAnyHeader() // 允許的標頭
                  .AllowAnyMethod(); // 允許的 HTTP 方法 (GET, POST, PUT, DELETE 等)
        });
    }
    else
    {
        var cors = builder.Configuration.GetSection("Cors").Get<string[]>();

        options.AddPolicy(corsPolicyName, policy =>
        {
            policy.WithOrigins(cors) // 允許的來源
                  .AllowAnyHeader() // 允許的標頭
                  .AllowAnyMethod() // 允許的 HTTP 方法 (GET, POST, PUT, DELETE 等)
                  .AllowCredentials(); // 如果需要攜帶 Cookie 或憑證
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