using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenVpnAuthUser.Models;
using OpenVpnAuthUser.Services;
using Serilog;
using System;
using System.Collections.Generic;
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
            int result = 1;

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
                
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();


                // call user validation
                string authService = serviceProvider.GetService<IOptions<SettingsModel>>().Value.AuthService;
                IAuthUserService authUserService = serviceProvider.GetService<Func<string, IAuthUserService>>()(authService);
                await authUserService.Validate(args);

                // OK
                result = 0;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Fatal(ex, "Unauthorized");
                // NOK
                result = 1;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "App terminated unexpectedly");
                // NOK
                result = 1;
            }
            finally
            {
                Log.Information("App finished");
                Log.CloseAndFlush();
            }

            
            Environment.Exit(result);
        }
    


        static private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());

            services.Configure<SettingsModel>(Configuration.GetSection("Settings"));

            // add auth services
            services.AddScoped<AuthUserPasswordEnvironmentService>();
            services.AddScoped<AuthUserPasswordFileService>();

            // Strategies factory for IAuthUserService
            services.AddScoped<Func<string, IAuthUserService>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case nameof(AuthUserPasswordEnvironmentService):
                        return serviceProvider.GetService<AuthUserPasswordEnvironmentService>();
                    case nameof(AuthUserPasswordFileService):
                        return serviceProvider.GetService<AuthUserPasswordFileService>();
                    default:
                        throw new KeyNotFoundException($"Assertion, register type {key} not exists in IOC");
                }
            });
        }


    }
}
