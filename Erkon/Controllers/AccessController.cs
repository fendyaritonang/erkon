using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Erkon.Classes;
using Erkon.Models;
using System;

namespace Erkon.Controllers
{
    [Authorize]
    public class AccessController : Controller
	{
        private readonly MySqlConnection _mySqlConnection;

        public AccessController(MySqlConnection mySqlConnection)
		{
			_mySqlConnection = mySqlConnection;
		}

		public IActionResult Index()
		{
			var access = new Access(_mySqlConnection);
			return View(access.GetAccessList());
		}

        [HttpGet]
        public IActionResult Add()
        {
            var unit = new Unit(_mySqlConnection);
            var accessMaintenance = new AccessMaintenanceModel();
            accessMaintenance.Access = new AccessModel();
            accessMaintenance.Units = unit.GetUnits(); ;
            return View(accessMaintenance);
        }

        [HttpPost]
        public IActionResult Add(AccessModel access)
        {
            var a = new Access(_mySqlConnection);
            a.AddAccess(access);
            return Redirect($"/Home/Successful?identifier=0&messagetype=addaccess");
        }

        [HttpGet]
        public IActionResult Edit(string unitcode, string userid)
        {
            var unit = new Unit(_mySqlConnection);
            var access = new Access(_mySqlConnection);
            var accessMaintenance = new AccessMaintenanceModel();
            accessMaintenance.Units = unit.GetUnits();
            accessMaintenance.Access = access.GetAccessById(unitcode, userid);
            accessMaintenance.Access.UserIdOriginal = accessMaintenance.Access.UserId;
            accessMaintenance.Access.UnitCodeOriginal = accessMaintenance.Access.UnitCode;
            return View(accessMaintenance);
        }

        [HttpPost]
        public IActionResult Edit(AccessModel access, string submit)
        {
            var a = new Access(_mySqlConnection);
            switch (submit)
            {
                case "submit":
                    a.EditAccess(access);
                    return Redirect($"/Home/Successful?identifier=&messagetype=editaccess");
                case "inactivate":
                    a.InactivateAccess(access.UnitCodeOriginal, access.UserIdOriginal);
                    return Redirect($"/Home/Successful?identifier=&messagetype=inactivateaccess");
            }
            return Redirect($"/Access");
        }
    }
}
