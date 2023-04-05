using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Erkon.Classes;
using Erkon.Models;

namespace Erkon.Controllers
{
    public class LogsController : Controller
    {
        private readonly MySqlConnection _mySqlConnection;
        private readonly IConfiguration _configuration;
        private readonly string _hs;

        public LogsController(MySqlConnection mySqlConnection, IConfiguration configuration)
        {
            _mySqlConnection = mySqlConnection;
            _configuration = configuration;
            _hs = _configuration["Hsk"].ToString();
        }

        public IActionResult Index(string unitcode, float temperature, float humidity, string hsk)
        {
            var log = new LogModel();
            log.UnitCode = unitcode;
            log.Temperature = temperature;
            log.Humidity = humidity;
            if (hsk == _hs)
            {
                var l = new Log(_mySqlConnection);
                l.AddLog(log);
                return Json(new { status = "success" });
            }

            return Json(new { status = "failed" });
        }
    }
}
