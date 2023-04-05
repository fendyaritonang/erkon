using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Erkon.Classes;
using Erkon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using MQTTnet;
using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Erkon.Controllers
{
    [Authorize]
    public class UnitsController : Controller
    {
        private readonly MySqlConnection _mySqlConnection;
		private readonly IConfiguration _configuration;

		public UnitsController(MySqlConnection mySqlConnection, IConfiguration configuration)
        {
            _mySqlConnection = mySqlConnection;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var unit = new Unit(_mySqlConnection);
            var units = unit.GetUnits();
            return View(units);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(UnitModel units)
        {
            var unit = new Unit(_mySqlConnection);
            var newCode = unit.AddUnit(units);

            return Redirect($"/Home/Successful?identifier={newCode}&messagetype=addunit");
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var unit = new Unit(_mySqlConnection);
            return View(unit.GetUnitById(id));
        }

        [HttpPost]
        public IActionResult Edit(UnitModel unit, string submit)
        {
            var u = new Unit(_mySqlConnection);
            switch (submit)
            {
                case "submit":
                    u.EditUnit(unit);
                    return Redirect($"/Home/Successful?identifier={unit.Code}&messagetype=editunit");
                case "inactivate":
                    u.InactivateUnit(Guid.Parse(unit.Code));
                    return Redirect($"/Home/Successful?identifier={unit.Code}&messagetype=inactivateunit");
            }
            return Redirect($"/Units");
        }

		[HttpGet]
		public List<UnitModel> GetAllUnitInfo(string? roomnumber)
		{
			var login = Login.GetUserInfo(User);

			var unit = new Unit(_mySqlConnection);
			var units = unit.GetUnitByAccess(roomnumber, login.Username);

            return units;
		}

		[HttpGet]
        public async Task<IActionResult> ChangeState(string code, short state)
        {
            var mqtt = new MqttHelper(_configuration);
            await mqtt.SendPayload(code, (state == 0 ? "00" : "01"));

            var user = Login.GetUserInfo(User);
            var u = new Unit(_mySqlConnection);
            var unit = new UnitModel
            {
                Code = code,
                State = state,
            };
            await u.ChangeState(unit, user.Username);

			return Json(new { status = "success" });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeTemperature(string code, short temperature)
        {
			var mqtt = new MqttHelper(_configuration);
			await mqtt.SendPayload(code, temperature.ToString());

			var user = Login.GetUserInfo(User);
			var u = new Unit(_mySqlConnection);
            var unit = new UnitModel
            {
                Code = code,
                Temperature = temperature,
            };
            await u.ChangeTemperature(unit, user.Username);

            return Json(new { status = "success" });
        }
    }
}
