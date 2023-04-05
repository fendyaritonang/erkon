using Dapper;
using Erkon.Models;
using MySqlConnector;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Erkon.Classes
{
	public class Access
	{
		private readonly MySqlConnection _mySqlConnection;

		public Access(MySqlConnection mySqlConnection)
		{
			_mySqlConnection = mySqlConnection;
		}
				
		public List<AccessModel> GetAccessList()
		{
			var sql = "SELECT a.userid, a.unitcode, " +
				"CASE WHEN a.unitcode = '*' THEN 'All Rooms' ELSE b.roomnumber END AS unitroomnumber, " +
				"CASE WHEN a.unitcode = '*' THEN 'All Locations' ELSE b.roomlocation END AS unitroomlocation " +
				"FROM access a " +
				"LEFT JOIN units b ON a.unitcode = b.code AND b.status = 1 " +
				"WHERE a.unitcode = '*' OR b.code IS NOT NULL " +
				"ORDER BY a.userid;";
			return _mySqlConnection.Query<AccessModel>(sql).ToList();
		}

		public AccessModel GetAccessById(string unitcode, string userid)
		{
			var sql = "SELECT a.userid, a.unitcode, " +
				"CASE WHEN a.unitcode = '*' THEN 'All Rooms' ELSE b.roomnumber END AS unitroomnumber, " +
				"CASE WHEN a.unitcode = '*' THEN 'All Locations' ELSE b.roomlocation END AS unitroomlocation " +
				"FROM access a " +
				"LEFT JOIN units b ON a.unitcode = b.code AND b.status = 1 " +
				"WHERE a.unitcode = @unitcode and a.userid = @userid AND (a.unitcode = '*' OR b.code IS NOT NULL);";
			var parameters = new DynamicParameters();
			parameters.Add("unitcode", unitcode, System.Data.DbType.String);
			parameters.Add("userid", userid, System.Data.DbType.String);
			return _mySqlConnection.QuerySingle<AccessModel>(sql, parameters);
		}

		public void AddAccess(AccessModel access)
		{
			var sql = "INSERT access (userid, unitcode) " +
				"SELECT @userid, @unitcode " +
				"WHERE NOT EXISTS (SELECT 1 FROM access WHERE userid = @userid and (unitcode = @unitcode or unitcode = '*'))";

			if (access.UnitCode == "*")
			{
				sql = "DELETE FROM access WHERE userid = @userid; " + sql;
			}

			using var command = new MySqlCommand(sql, _mySqlConnection);
			command.Parameters.Add("@unitcode", MySqlDbType.VarChar, 36).Value = access.UnitCode;
			command.Parameters.Add("@userid", MySqlDbType.VarChar, 50).Value = access.UserId;
			_mySqlConnection.Open();
			command.ExecuteNonQuery();
			_mySqlConnection.Close();
		}
		public void InactivateAccess(string unitcode, string userid)
		{
			var sql = "DELETE FROM access WHERE userid = @useridOriginal and unitcode = @unitcodeOriginal";
			using var command = new MySqlCommand(sql, _mySqlConnection);
			command.Parameters.Add("@unitcodeOriginal", MySqlDbType.VarChar, 36).Value = unitcode;
			command.Parameters.Add("@useridOriginal", MySqlDbType.VarChar, 50).Value = userid;
			_mySqlConnection.Open();
			command.ExecuteNonQuery();
			_mySqlConnection.Close();
		}

		public string EditAccess(AccessModel access)
		{
			var newCode = Guid.NewGuid().ToString();
			var sql = "UPDATE access SET " +
				"userid = @userid," +
				"unitcode = @unitcode " +
				"WHERE userid = @useridOriginal AND unitcode = @unitcodeOriginal " +
				"AND NOT EXISTS (SELECT 1 FROM access WHERE userid = @userid and unitcode = @unitcode)";

			if (access.UnitCode == "*")
			{
				sql = "DELETE FROM access WHERE userid = @userid; " +
					"INSERT access (userid, unitcode) " +
					"SELECT @userid, @unitcode " +
					"WHERE NOT EXISTS (SELECT 1 FROM access WHERE userid = @userid and unitcode = @unitcode)";
			}
			using var command = new MySqlCommand(sql, _mySqlConnection);
			command.Parameters.Add("@unitcode", MySqlDbType.VarChar, 36).Value = access.UnitCode;
			command.Parameters.Add("@unitcodeOriginal", MySqlDbType.VarChar, 36).Value = access.UnitCodeOriginal;
			command.Parameters.Add("@userid", MySqlDbType.VarChar, 50).Value = access.UserId;
			command.Parameters.Add("@useridOriginal", MySqlDbType.VarChar, 50).Value = access.UserIdOriginal;
			_mySqlConnection.Open();
			command.ExecuteNonQuery();
			_mySqlConnection.Close();

			return newCode;
		}
	}
}
