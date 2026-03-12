using Infrastructure.Repositories;
using ManageAccount.Infrastructure.Context;
using ManageAccount.Infrastructure.Implementations;
using ManageAccount.Infrastructure.Repositories;
using ManageAccount.Services;
using ManageAccount.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ManageAccount
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                var configuration = BuildConfiguration();
                var loggingMode = ResolveLoggingMode(configuration);

                GlobalDiagnosticsContext.Set("loggingMode", loggingMode);

                LogManager.Setup().LoadConfigurationFromFile("nlog.config");
                LogManager.ReconfigExistingLoggers();

                var services = new ServiceCollection();
                services.AddSingleton(configuration);
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    logging.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true,
                        IncludeScopes = true
                    });
                });

                services.AddDbContext<ApplicationDbContext>();

                services.AddScoped<IAccountRepository, AccountRepository>();
                services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
                services.AddScoped<IInterestTypeRepository, InterestTypeRepository>();
                services.AddScoped<AccountService>();
                services.AddScoped<AccountFunctionsUI>();
                services.AddScoped<ConsoleUI>();

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();

                var scopedProvider = scope.ServiceProvider;
                var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                DatabaseSeeder.Seed(dbContext);

                var appLogger = serviceProvider.GetRequiredService<ILogger<Program>>();
                appLogger.LogInformation("ManageAccount is starting with logging mode {LoggingMode}.", loggingMode);
                appLogger.LogInformation("Database initialized and seed routine completed.");

                var consoleUI = scopedProvider.GetRequiredService<ConsoleUI>();
                consoleUI.Run();

                appLogger.LogInformation("ManageAccount stopped normally.");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Application terminated unexpectedly.");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        private static string ResolveLoggingMode(IConfiguration configuration)
        {
            var mode = configuration["LoggingMode:ModeFlag"]?.Trim().ToLowerInvariant();

            return mode switch
            {
                "console" => "console",
                "file" => "file",
                "database" => "database",
                _ => "file"
            };
        }
    }
}
