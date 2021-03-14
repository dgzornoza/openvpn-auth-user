using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenVpnAuthUser.Models;
using OpenVpnAuthUser.Services;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OpenVpnAuthUser
{
    class Program
    {
        /// <summary>
        /// Application configuration
        /// </summary>
        static public IConfiguration Configuration { get; private set; }


        static async Task Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {                
                Log.Information("App started");



                LogUserName();



                ServiceCollection serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

                IAuthUserService authUserService = serviceProvider.GetService<IAuthUserService>();
                await authUserService.Validate();

                // OK
                Environment.Exit(0);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Fatal(ex, "Unauthorized");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Log.Fatal(ex, "App terminated unexpectedly");                
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Log.Information("App finished");
                Log.CloseAndFlush();
            }

            // NO OK
            Environment.Exit(1);
        }


        private static void LogUserName()
        {
            string userName = Environment.UserName;


            Log.Information($"UserName: {userName}");

            Log.Information($"Launched from {Environment.CurrentDirectory}");
            Log.Information($"Physical location {AppDomain.CurrentDomain.BaseDirectory}");
            Log.Information($"AppContext.BaseDir {AppContext.BaseDirectory}");
            Log.Information($"Runtime Call {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}");

        }
    


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());

            services.Configure<SettingsModel>(Configuration.GetSection("Settings"));

            services.AddScoped<IAuthUserService, AuthUserPasswordService>();
        }


    }
}
