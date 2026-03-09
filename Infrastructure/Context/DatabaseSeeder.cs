using ManageAccount.Data.Entities;

namespace ManageAccount.Infrastructure.Context
{
    public static class DatabaseSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            var savingsInterest = context.InterestTypes.FirstOrDefault(i => i.Rate == 4.7m);
            if (savingsInterest == null)
            {
                savingsInterest = new InterestType { Rate = 4.7m };
                context.InterestTypes.Add(savingsInterest);
            }

            var checkingInterest = context.InterestTypes.FirstOrDefault(i => i.Rate == 5.1m);
            if (checkingInterest == null)
            {
                checkingInterest = new InterestType { Rate = 5.1m };
                context.InterestTypes.Add(checkingInterest);
            }

            context.SaveChanges();

            if (context.Accounts.Any())
            {
                return;
            }

            AddSeedAccount(context, "Nguyen Van An", 18000000m, 6200000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Tran Thi Binh", 950000m, 350000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Le Minh Chau", 27500000m, 12500000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Pham Quoc Dat", 4200000m, 980000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Hoang Thu Giang", 13200000m, 24000000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Do Anh Khoa", 760000m, 540000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Bui Ngoc Lan", 64000000m, 31000000m, savingsInterest.Id, checkingInterest.Id);
            AddSeedAccount(context, "Dang Gia Minh", 5200000m, 1800000m, savingsInterest.Id, checkingInterest.Id);

            context.SaveChanges();
        }

        private static void AddSeedAccount(
            ApplicationDbContext context,
            string name,
            decimal savingsBalance,
            decimal checkingBalance,
            int savingsInterestTypeId,
            int checkingInterestTypeId)
        {
            var account = new Account { Name = name };
            context.Accounts.Add(account);
            context.SaveChanges();

            context.AccountBalances.Add(new AccountBalance
            {
                AccountId = account.Id,
                Type = "Tài khoản tiết kiệm",
                Balance = savingsBalance,
                InterestTypeId = savingsInterestTypeId
            });

            context.AccountBalances.Add(new AccountBalance
            {
                AccountId = account.Id,
                Type = "Tài khoản thanh toán",
                Balance = checkingBalance,
                InterestTypeId = checkingInterestTypeId
            });
        }
    }
}
