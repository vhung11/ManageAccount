using Infrastructure.Repositories;
using ManageAccount.Infrastructure.Context;
using ManageAccount.Infrastructure.Implementations;
using ManageAccount.Infrastructure.Repositories;
using ManageAccount.Services;
using ManageAccount.UI;
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
                LogManager.Setup().LoadConfigurationFromFile("nlog.config");

                var services = new ServiceCollection();

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
                var appLogger = serviceProvider.GetRequiredService<ILogger<Program>>();

                appLogger.LogInformation("ManageAccount is starting.");
                appLogger.LogInformation("Initializing database connection and seed data.");

                using var scope = serviceProvider.CreateScope();
                var scopedProvider = scope.ServiceProvider;
                var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.EnsureCreated();
                DatabaseSeeder.Seed(dbContext);

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
    }
}
