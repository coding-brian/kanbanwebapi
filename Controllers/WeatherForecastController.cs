using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace KanbanWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IDbConnection _connection;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDbConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        [HttpGet]
        public async Task<object> Get()
        {
            var sql = @"SELECT * FROM board;";

            return await _connection.QueryAsync(sql);
        }
    }
}