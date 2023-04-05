using Dapper;
using Erkon.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Erkon.Classes
{
	public class Unit
	{
		private readonly MySqlConnection _mySqlConnection;

		public Unit(MySqlConnection mySqlConnection)
		{
			_mySqlConnection = mySqlConnection;
		}
				
		public List<UnitModel> GetUnits()
		{
			var sql = "SELECT code, status, roomnumber, roomlocation, " +
				"temperature, temperatureassigned, humidity, state " +
				"FROM units " +
				"WHERE status = 1 " +
				"ORDER BY roomnumber;";
			return _mySqlConnection.Query<UnitModel>(sql).ToList();
		}

		public bool IsAdmin(string userName)
		{
			var parameters = new DynamicParameters();
			parameters.Add("username", userName, System.Data.DbType.String);

			var sqlAccess = "SELECT 1 FROM access where userid = @username AND unitcode = '*' LIMIT 1;";

			List<UnitModel> haveAllAccessResult;
			haveAllAccessResult = _mySqlConnection.Query<UnitModel>(sqlAccess, parameters).ToList();

			return haveAllAccessResult.Count > 0;
		}

		public async Task<bool> IsAdminAsync(string userName)
		{
			var parameters = new DynamicParameters();
			parameters.Add("username", userName, System.Data.DbType.String);

			var sqlAccess = "SELECT 1 FROM access where userid = @username AND unitcode = '*' LIMIT 1;";

			IEnumerable<UnitModel> haveAllAccessResult;
			haveAllAccessResult = await _mySqlConnection.QueryAsync<UnitModel>(sqlAccess, parameters);

			return haveAllAccessResult.Any();

		}

		public List<UnitModel> GetUnitByAccess(string roomNumber, string userName)
		{
			var haveAllAccess = IsAdmin(userName);

			var haveAllAccessSql = "INNER JOIN access b ON a.code = b.unitcode AND b.userid = @username ";
			var roomnumberSql = " AND a.roomnumber = @roomnumber ";
			if (string.IsNullOrEmpty(roomNumber))
			{
				roomnumberSql = "";
			}
			if (haveAllAccess)
			{
				haveAllAccessSql = "";
			}
			var sql = "SELECT a.code, a.status, a.roomnumber, a.roomlocation, " +
				"a.temperature, a.temperatureassigned, a.humidity, a.state " +
				"FROM units a " +
				haveAllAccessSql +
				"WHERE a.status = 1 " + roomnumberSql +
				"ORDER BY a.roomnumber;";

			var parameters = new DynamicParameters();
			parameters.Add("username", userName, System.Data.DbType.String);
			if (roomnumberSql != "")
				parameters.Add("roomnumber", roomNumber, System.Data.DbType.String);

			return _mySqlConnection.Query<UnitModel>(sql, parameters).ToList();
		}

		public UnitModel GetUnitById(Guid id)
		{
			var sql = "SELECT code, status, roomnumber, roomlocation, " +
				"temperature, temperatureassigned, humidity, state " +
				"FROM units " +
				"WHERE code = @code";
			var parameters = new DynamicParameters();
			parameters.Add("code", id, System.Data.DbType.String);
			return _mySqlConnection.QuerySingle<UnitModel>(sql, parameters);
		}

		public string AddUnit(UnitModel units)
		{
			var newCode = Guid.NewGuid().ToString();
			var sql = "INSERT units (code, roomnumber, roomlocation) " +
				"SELECT @code, @roomnumber, @roomlocation " +
				"WHERE NOT EXISTS (SELECT 1 FROM units WHERE code = @code)";

			var con = _mySqlConnection;
			using var command = new MySqlCommand(sql, con);
			command.Parameters.Add("@code", MySqlDbType.VarChar, 36).Value = newCode;
			command.Parameters.Add("@roomnumber", MySqlDbType.VarChar, 50).Value = units.RoomNumber;
			command.Parameters.Add("@roomlocation", MySqlDbType.VarChar, 50).Value = units.RoomLocation;
			con.Open();
			command.ExecuteNonQuery();
			con.Close();

			return newCode;
		}

		public void InactivateUnit(Guid id)
		{
			var sql = "UPDATE units SET status = 0 WHERE code = @code";

			var con = _mySqlConnection;
			using var command = new MySqlCommand(sql, con);
			command.Parameters.Add("@code", MySqlDbType.VarChar, 36).Value = id;
			con.Open();
			command.ExecuteNonQuery();
			con.Close();
		}

		public string EditUnit(UnitModel units)
		{
			var newCode = Guid.NewGuid().ToString();
			var sql = "UPDATE units SET " +
				"roomnumber = @roomnumber," +
				"roomlocation = @roomlocation," +
				"status = 1 " +
				"WHERE code = @code";

			var con = _mySqlConnection;
			using var command = new MySqlCommand(sql, con);
			command.Parameters.Add("@code", MySqlDbType.VarChar, 36).Value = units.Code;
			command.Parameters.Add("@roomnumber", MySqlDbType.VarChar, 50).Value = units.RoomNumber;
			command.Parameters.Add("@roomlocation", MySqlDbType.VarChar, 50).Value = units.RoomLocation;
			con.Open();
			command.ExecuteNonQuery();
			con.Close();

			return newCode;
		}

		public async Task<bool> ChangeState(UnitModel unit, string userName)
		{
			var haveAllAccess = await IsAdminAsync(userName);

			string sql;

			if (haveAllAccess)
			{
				sql = "UPDATE units SET state = @state WHERE code = @code and state != @state AND status = 1;";
			}
			else
			{
				sql = "UPDATE units a " +
					"INNER JOIN access b ON a.code = b.unitcode " +
					"SET state = @state " +
					"WHERE code = @code AND b.userid = @username AND a.state != @state AND a.status = 1;";
			}

			var con = _mySqlConnection;
			using var command = new MySqlCommand(sql, con);
			command.Parameters.Add("@code", MySqlDbType.VarChar, 36).Value = unit.Code;
			command.Parameters.Add("@state", MySqlDbType.Int16).Value = unit.State;
			command.Parameters.Add("@username", MySqlDbType.VarChar, 50).Value = userName;

			await con.OpenAsync();
			await command.ExecuteNonQueryAsync();
			await con.CloseAsync();

			return true;
		}

		public async Task<bool> ChangeTemperature(UnitModel unit, string userName)
		{
			var haveAllAccess = await IsAdminAsync(userName);

			string sql;

			if (haveAllAccess)
			{
				sql = "UPDATE units SET temperatureassigned = @temperature WHERE code = @code and IFNULL(temperatureassigned, '') != @temperature AND status = 1;";
			}
			else
			{
				sql = "UPDATE units a " +
					"INNER JOIN access b ON a.code = b.unitcode " +
					"SET temperatureassigned = @temperature " +
					"WHERE code = @code AND b.userid = @username AND IFNULL(a.temperatureassigned, '') != @temperature AND a.status = 1;";
			}

			var con = _mySqlConnection;
			using var command = new MySqlCommand(sql, con);
			command.Parameters.Add("@code", MySqlDbType.VarChar, 36).Value = unit.Code;
			command.Parameters.Add("@temperature", MySqlDbType.Int16).Value = unit.Temperature;
			command.Parameters.Add("@username", MySqlDbType.VarChar, 50).Value = userName;
			await con.OpenAsync();
			await command.ExecuteNonQueryAsync();
			await con.CloseAsync();

			return true;
		}
		
	}
}
