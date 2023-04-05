using Microsoft.Extensions.Hosting;
using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace ErkonListener
{
	internal class WorkerService : BackgroundService
	{
		private readonly IConfiguration _configuration;
		private readonly MySqlConnection _mySqlConnection;

		public WorkerService(IConfiguration configuration, MySqlConnection mySqlConnection)
		{
			_configuration = configuration;	
			_mySqlConnection = mySqlConnection;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var mqttfactory = new MqttFactory();
			using var mqttClient = mqttfactory.CreateMqttClient();
			var mqttClientOptions = new MqttClientOptionsBuilder()
				.WithClientId(Guid.NewGuid().ToString())
				.WithTcpServer(_configuration["Mqtt:Server"], Convert.ToInt16(_configuration["Mqtt:Port"]))
				.WithCleanSession()
				.WithCredentials(_configuration["Mqtt:Username"], _configuration["Mqtt:Password"])
				.Build();

			mqttClient.ApplicationMessageReceivedAsync += e =>
			{				
				// message pattern: {deviceId}/{infotype}/{infovalue}
				// infotype -> t: temperature, h: humidity
				var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
				Console.Write(message + "...");

				var messageData = message.Split("/");

				if (messageData.Length != 3)
				{
					Console.WriteLine("No data..");
					return Task.CompletedTask;
				}

				var deviceId = messageData[0];
				var infoType = messageData[1];
				var infoValue = Convert.ToDouble(messageData[2]);

				var sql = "UPDATE `units` set " + (infoType == "t" ? "`temperature`" : "`humidity`") + " = @infoValue WHERE `code` = @deviceId";
				using var command = new MySqlCommand(sql, _mySqlConnection);
				command.Parameters.Add("@infoValue", MySqlDbType.Double).Value = infoValue;
				command.Parameters.Add("@deviceId", MySqlDbType.VarChar, 100).Value = deviceId;
				_mySqlConnection.Open();
				command.ExecuteNonQuery();
				_mySqlConnection.Close();

				Console.WriteLine("Stored");

				return Task.CompletedTask;
			};

			await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

			var mqttSubscribeOptions = mqttfactory.CreateSubscribeOptionsBuilder()
				.WithTopicFilter(
					f =>
					{
						f.WithTopic("DeviceInfo");
					})
				.Build();

			await mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

			Console.WriteLine("MQTT client subscribed to topic.");

			Console.WriteLine("Press enter to exit.");
			Console.ReadLine();
		}
	}
}
