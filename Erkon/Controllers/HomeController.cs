using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Erkon.Models;
using System.Security.Claims;
using System.Linq;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MySqlConnector;
using MySqlConnector.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Dapper;
using Erkon.Classes;
using Microsoft.Extensions.Configuration;

namespace Erkon.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MySqlConnection _mySqlConnection;
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration, MySqlConnection mySqlConnection)
        {
            _mySqlConnection = mySqlConnection;
            _configuration = configuration;
        }

        public IActionResult Index(string? roomnumber)
        {
            var login = Login.GetUserInfo(User);

            var unit = new Unit(_mySqlConnection);
            var units = unit.GetUnitByAccess(roomnumber, login.Username);

            ViewBag.TemperatureListing = new AirconTemperature(_configuration).Listing();
            ViewBag.RoomNumber = roomnumber;

            return View(units);
        }

        /*[AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/

        [HttpGet]
        public IActionResult Successful(string identifier, string messagetype)
        {
            var successful = new Successful(identifier, messagetype);
            return View(successful);
        }
    }
}