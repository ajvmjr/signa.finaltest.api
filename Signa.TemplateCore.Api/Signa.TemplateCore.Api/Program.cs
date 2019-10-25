using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Signa.TemplateCore.Api
{
    public class Program
    {
        private const string LOG_PATH = "logs/logs.txt";

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Error()
                            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .WriteTo.File(LOG_PATH)
                            .CreateLogger();

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Aplicação finalizada por erro. Verificar log no arquivo {LOG_PATH}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
            ;
    }
}
