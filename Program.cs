using Infrastructure.Repositories;
using ManageAccount.Infrastructure.Context;
using ManageAccount.Infrastructure.Implementations;
using ManageAccount.Infrastructure.Repositories;
using ManageAccount.Services;
using ManageAccount.UI;
using Microsoft.Extensions.DependencyInjection;

namespace ManageAccount
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

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

            dbContext.Database.EnsureCreated();
            DatabaseSeeder.Seed(dbContext);

            var consoleUI = scopedProvider.GetRequiredService<ConsoleUI>();
            consoleUI.Run();
        }
    }
}
