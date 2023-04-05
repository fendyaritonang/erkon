using Microsoft.Extensions.Configuration;
using MQTTnet.Client;
using MQTTnet;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace Erkon.Classes
{
	public class MqttHelper
	{
		private readonly IConfiguration _configuration;

		public MqttHelper(IConfiguration configuration)
		{
			_configuration = configuration;	
		}

		/*
		00: Turn off,
		01: Turn on,
		16-26: Change temperature to assigned degree
		 */
		private static readonly string[] payloadList = { "00", "01", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26" };

		private bool validatePayload(string payload)
		{
			if (Array.FindIndex(payloadList, x => x == payload) == -1)
			{
				return false;
			}

			return true;
		}

		public async Task SendPayload(string topic, string payload)
		{
			if (!validatePayload(payload))
				return;

			var mqttfactory = new MqttFactory();
			using var mqttClient = mqttfactory.CreateMqttClient();
			var mqttClientOptions = new MqttClientOptionsBuilder()
				.WithClientId(Guid.NewGuid().ToString())
				.WithTcpServer(_configuration["Mqtt:Server"], Convert.ToInt16(_configuration["Mqtt:Port"]))
				.WithCleanSession()
				.WithCredentials(_configuration["Mqtt:Username"], _configuration["Mqtt:Password"])
				.Build();

			try
			{
				using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
				{
					await mqttClient.ConnectAsync(mqttClientOptions, timeoutToken.Token);
				}
			}
			catch (OperationCanceledException)
			{
				//lblSendOnPayload.Text = "Timeout while connecting.";
			}
			catch (Exception ex)
			{
				//lblSendOnPayload.Text = "Authentication failed";
			}

			var message = new MqttApplicationMessageBuilder()
								.WithTopic(topic)
								.WithPayload(payload)
								.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
								.Build();

			if (mqttClient.IsConnected)
			{
				try
				{
					await mqttClient.PublishAsync(message);
				}
				catch (Exception ex)
				{
					//lblSendOnPayload.Text = ex.ToString();
				}
				await mqttClient.DisconnectAsync();
			}
		}
	}
}
