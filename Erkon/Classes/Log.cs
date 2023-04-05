using Erkon.Models;
using MySqlConnector;

namespace Erkon.Classes
{
	public class Log
	{
		private readonly MySqlConnection _mySqlConnection;

		public Log(MySqlConnection mySqlConnection)
		{
			_mySqlConnection = mySqlConnection;
		}

		public void AddLog(LogModel log)
		{
			var sql = "INSERT logs (unitcode, temperature, humidity) " +
				"SELECT @unitcode, @temperature, @humidity " +
				"WHERE EXISTS (SELECT 1 FROM units WHERE code = @unitcode AND status = 1);";
			using var command = new MySqlCommand(sql, _mySqlConnection);
			command.Parameters.Add("@unitcode", MySqlDbType.VarChar, 36).Value = log.UnitCode;
			command.Parameters.Add("@temperature", MySqlDbType.VarChar, 50).Value = log.Temperature;
			command.Parameters.Add("@humidity", MySqlDbType.VarChar, 50).Value = log.Humidity;
			_mySqlConnection.Open();
			command.ExecuteNonQuery();
			_mySqlConnection.Close();
		}
	}
}
