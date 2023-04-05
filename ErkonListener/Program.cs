using ErkonListener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
{
	services.AddHostedService<WorkerService>();

	IConfiguration configuration = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", false)
		.Build();

	services.AddTransient(_ => new MySqlConnection(configuration["ConnectionStrings:Mysql"]));

}).Build().Run();



Console.WriteLine("Running");