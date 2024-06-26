using ConfigurationSettings;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Options;
using PowerConsumptionOptimizer;
using PowerProduction;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using TeslaAPI;
using TeslaControl;


//TODO: Consider if vehicle charge level is below override level but there is no solar production (app would be paused)
//TODO: Use different cancellation token for program exit?
//TODO: Update & Add tests for new PCO methods
//TODO: Update & Add tests for new Helper methods
//TODO: Add comments to public and internal methods
//TODO: Consider logging the watt buffer setting along with the net power production output
//TODO: Enable start/stop from command line
//TODO: Respect vehicle location with regard to settings

[assembly: InternalsVisibleTo("PowerConsumptionOptimizer.Tests")]

using IHost host = CreateHostBuilder().Build();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose)
    .WriteTo.File(new JsonFormatter(), "./logs/log_.json",
        restrictedToMinimumLevel: LogEventLevel.Warning,
        retainedFileTimeLimit: TimeSpan.FromDays(7),
        rollingInterval: RollingInterval.Day, shared: true)
    .WriteTo.File("./logs/all_.logs",
        retainedFileTimeLimit: TimeSpan.FromDays(1),
        rollingInterval: RollingInterval.Day, shared: true)
    .WriteTo.File("./logs/important_.logs",
        restrictedToMinimumLevel: LogEventLevel.Warning,
        retainedFileTimeLimit: TimeSpan.FromDays(31),
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddRequestTimeouts(options => {
    options.DefaultPolicy =
        new RequestTimeoutPolicy { Timeout = TimeSpan.FromMilliseconds(1500) };
    options.AddPolicy("MyPolicy", TimeSpan.FromSeconds(2));
});

static IHostBuilder CreateHostBuilder() =>
    Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true);
            config.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            config.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
            IConfiguration configuration = hostContext.Configuration;

            services.Configure<TeslaSettings>(configuration.GetSection(nameof(TeslaSettings)));
            services.Configure<HelperSettings>(configuration.GetSection(nameof(HelperSettings)));
            services.Configure<VehicleSettings>(configuration.GetSection(nameof(VehicleSettings)));
            services.Configure<PowerProductionSettings>(configuration.GetSection(nameof(PowerProductionSettings)));
            services.Configure<ConsumptionOptimizerSettings>(configuration.GetSection(nameof(ConsumptionOptimizerSettings)));

            services.AddTeslaApi();

            // Register TeslaControl using IOptionsSnapshot
            services.AddSingleton<ITeslaControl, TeslaControl.TeslaControl>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<TeslaControl.TeslaControl>>();
                var teslaApi = serviceProvider.GetService<ITeslaAPI>();
                var teslaSettings = serviceProvider.GetRequiredService<IOptionsSnapshot<TeslaSettings>>();
                return new TeslaControl.TeslaControl(logger, teslaApi, teslaSettings.Value.TeslaRefreshToken);
            });

            services.AddSingleton<IPowerProduction, EnphaseLocal>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<EnphaseLocal>>();
                var powerProductionSettings = serviceProvider.GetRequiredService<IOptionsSnapshot<PowerProductionSettings>>();
                return new EnphaseLocal(logger, powerProductionSettings.Value.EnvoyToken);
            });

            // Register ConsumptionOptimizer using IOptionsSnapshot
            services.AddTransient<IConsumptionOptimizer, ConsumptionOptimizer>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ConsumptionOptimizer>>();
                var teslaControl = serviceProvider.GetService<ITeslaControl>();
                var powerProduction = serviceProvider.GetService<IPowerProduction>();
                var consumptionOptimizerSettings = serviceProvider.GetRequiredService<IOptionsMonitor<ConsumptionOptimizerSettings>>();
                var helperSettings = serviceProvider.GetRequiredService<IOptionsMonitor<HelperSettings>>();
                var vehicleSettings = serviceProvider.GetRequiredService<IOptionsMonitor<VehicleSettings>>();

                return new ConsumptionOptimizer(logger, helperSettings, vehicleSettings, consumptionOptimizerSettings, powerProduction, teslaControl);
            });

        })
        .UseSerilog();

Log.Information($"Power Consumption Optimizer - Starting v0.06");
var instance = ActivatorUtilities.CreateInstance<ConsumptionOptimizer>(host.Services);
await instance.Optimize();

Log.Information($"Power Consumption Optimizer - Exiting");
Log.CloseAndFlush();