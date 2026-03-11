using ManageAccount.Data.DTOs;
using ManageAccount.Helpers;
using ManageAccount.Services;
using Microsoft.Extensions.Logging;

namespace ManageAccount.UI
{
    public class AccountFunctionsUI
    {
        #region Fields

        private readonly AccountService _accountService;
        private readonly ILogger<AccountFunctionsUI> _logger;

        #endregion

        #region Constructor

        public AccountFunctionsUI(AccountService accountService, ILogger<AccountFunctionsUI> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        #endregion

        #region Display Methods

        private void ShowAccountDetails(AccountDTO account)
        {
            Console.WriteLine("\nThông tin tài khoản hiện tại:");
            Console.WriteLine($"ID: {account.Id} | Chủ tài khoản: {account.Name}");
            Console.WriteLine($"  ├─ Tài khoản tiết kiệm: {account.SavingsBalance:N0} VND (4.7%)");
            Console.WriteLine($"  ├─ Tài khoản thanh toán: {account.CheckingBalance:N0} VND (5.1%)");
            Console.WriteLine($"  └─ Tổng số dư: {account.TotalBalance:N0} VND");
        }

        public void ShowAllAccounts()
        {
            var accounts = _accountService.GetAllAccounts();
            _logger.LogInformation("Showing all accounts. Count: {AccountCount}.", accounts.Count);

            Console.WriteLine("=== DANH SÁCH TÀI KHOẢN ===\n");

            if (accounts.Count() == 0)
            {
                Console.WriteLine("Chưa có tài khoản nào.");
                return;
            }

            foreach (var acc in accounts)
            {
                ShowAccountDetails(acc);
                Console.WriteLine(new string('-', 40));
            }
        }

        #endregion

        #region CRUD Operations

        public void AddAccount()
        {
            Console.WriteLine("=== THÊM TÀI KHOẢN ===");

            string name = InputHelper.ReadString("Nhập tên chủ tài khoản: ");
            decimal balance = InputHelper.ReadDecimal("Nhập số dư ban đầu: ");

            _logger.LogInformation("Add-account request received for {AccountName} with initial balance {InitialBalance}.", name, balance);

            int newId = _accountService.AddAccount(name, balance);
            if (newId <= 0)
            {
                _logger.LogWarning("Add-account request failed for {AccountName}.", name);
                Console.WriteLine("✗ Thêm tài khoản thất bại.");
                return;
            }

            _logger.LogInformation("Add-account request completed successfully. New account id: {AccountId}.", newId);

            Console.WriteLine("\n✓ Thêm tài khoản thành công!");
            Console.WriteLine($"  - ID: {newId}");
            Console.WriteLine($"  - Tài khoản tiết kiệm: {balance / 2:N0} VND");
            Console.WriteLine($"  - Tài khoản thanh toán: {balance / 2:N0} VND");
        }

        public void DeleteAccount()
        {
            Console.WriteLine("=== XÓA TÀI KHOẢN ===");

            int id = InputHelper.ReadInt("Nhập ID tài khoản cần xóa: ");
            _logger.LogInformation("Delete-account request received for account {AccountId}.", id);

            if (_accountService.DeleteAccount(id))
            {
                _logger.LogInformation("Delete-account request completed for account {AccountId}.", id);
                Console.WriteLine("✓ Xóa tài khoản thành công!");
            }
            else
            {
                _logger.LogWarning("Delete-account request could not find account {AccountId}.", id);
                Console.WriteLine("✗ Không tìm thấy tài khoản với ID này.");
            }
        }

        #endregion

        #region Transaction Methods

        public void Deposit()
        {
            Console.WriteLine("=== NỘP TIỀN ===");

            int id = InputHelper.ReadInt("Nhập ID: ");
            _logger.LogInformation("Deposit request started for account {AccountId}.", id);

            var account = _accountService.GetAccountById(id);
            if (account == null)
            {
                _logger.LogWarning("Deposit request rejected because account {AccountId} was not found.", id);
                Console.WriteLine("✗ Không tìm thấy tài khoản với ID này.");
                return;
            }

            string choice;
            while (true)
            {
                Console.WriteLine("\nChọn loại tài khoản:");
                Console.WriteLine("1. Tài khoản tiết kiệm");
                Console.WriteLine("2. Tài khoản thanh toán");
                Console.Write("Lựa chọn (1-2): ");

                choice = Console.ReadLine() ?? "";

                if (choice == "1" || choice == "2")
                {
                    break;
                }

                Console.WriteLine("✗ Lựa chọn không hợp lệ! Vui lòng chọn 1 hoặc 2.\n");
            }

            decimal amount = InputHelper.ReadDecimal("Nhập số tiền nộp: ");
            string accountType = choice == "1" ? "Savings" : "Checking";
            _logger.LogInformation("Submitting deposit of {Amount} to {AccountType} for account {AccountId}.", amount, accountType, id);

            bool result = false;

            switch (choice)
            {
                case "1":
                    result = _accountService.DepositToSavings(id, amount);
                    if (result)
                        Console.WriteLine($"✓ Nộp {amount:N0} VND vào tài khoản tiết kiệm thành công!");
                    break;

                case "2":
                    result = _accountService.DepositToChecking(id, amount);
                    if (result)
                        Console.WriteLine($"✓ Nộp {amount:N0} VND vào tài khoản thanh toán thành công!");
                    break;
            }

            if (!result)
            {
                _logger.LogWarning("Deposit request failed for account {AccountId} and account type {AccountType}.", id, accountType);
                Console.WriteLine("✗ Giao dịch thất bại.");
                return;
            }

            _logger.LogInformation("Deposit request completed for account {AccountId} and account type {AccountType}.", id, accountType);
            
            // Lấy dữ liệu tài khoản đã cập nhật sau giao dịch
            var updatedAccount = _accountService.GetAccountById(id);
            if (updatedAccount != null)
            {
                ShowAccountDetails(updatedAccount);
            }
        }

        public void Withdraw()
        {
            Console.WriteLine("=== RÚT TIỀN ===");

            int id = InputHelper.ReadInt("Nhập ID: ");
            _logger.LogInformation("Withdraw request started for account {AccountId}.", id);

            var account = _accountService.GetAccountById(id);
            if (account == null)
            {
                _logger.LogWarning("Withdraw request rejected because account {AccountId} was not found.", id);
                Console.WriteLine("✗ Không tìm thấy tài khoản với ID này.");
                return;
            }

            string choice;
            while (true)
            {
                Console.WriteLine("\nChọn loại tài khoản:");
                Console.WriteLine("1. Tài khoản tiết kiệm (4.7%)");
                Console.WriteLine("2. Tài khoản thanh toán (5.1%)");
                Console.Write("Lựa chọn (1-2): ");

                choice = Console.ReadLine() ?? "";

                if (choice == "1" || choice == "2")
                {
                    break;
                }

                Console.WriteLine("✗ Lựa chọn không hợp lệ! Vui lòng chọn 1 hoặc 2.\n");
            }

            decimal amount = InputHelper.ReadDecimal("Nhập số tiền rút: ");
            string accountType = choice == "1" ? "Savings" : "Checking";
            _logger.LogInformation("Submitting withdrawal of {Amount} from {AccountType} for account {AccountId}.", amount, accountType, id);

            bool result = false;

            switch (choice)
            {
                case "1":
                    result = _accountService.WithdrawFromSavings(id, amount);
                    if (result)
                        Console.WriteLine($"✓ Rút {amount:N0} VND từ tài khoản tiết kiệm thành công!");
                    else
                        Console.WriteLine("✗ Rút tiền thất bại: số dư không đủ.");
                    break;

                case "2":
                    result = _accountService.WithdrawFromChecking(id, amount);
                    if (result)
                        Console.WriteLine($"✓ Rút {amount:N0} VND từ tài khoản thanh toán thành công!");
                    else
                        Console.WriteLine("✗ Rút tiền thất bại: số dư không đủ.");
                    break;
            }

            if (result)
            {
                _logger.LogInformation("Withdraw request completed for account {AccountId} and account type {AccountType}.", id, accountType);
                // Lấy dữ liệu tài khoản đã cập nhật sau giao dịch
                var updatedAccount = _accountService.GetAccountById(id);
                if (updatedAccount != null)
                {
                    ShowAccountDetails(updatedAccount);
                }
            }
            else
            {
                _logger.LogWarning("Withdraw request failed for account {AccountId} and account type {AccountType}.", id, accountType);
            }
        }

        public void ApplyInterest()
        {
            Console.WriteLine("=== TÍNH LÃI SUẤT ===");
            Console.WriteLine("Áp dụng lãi suất cho tất cả tài khoản...");

            _logger.LogInformation("Apply-interest request started.");

            _accountService.ApplyInterestToAllAccounts();

            _logger.LogInformation("Apply-interest request completed.");

            Console.WriteLine("✓ Tính lãi suất thành công!");
            Console.WriteLine("\nDanh sách tài khoản sau khi cộng lãi:");
            ShowAllAccounts();
        }

        #endregion

        #region Query & Statistics Methods

        public void ShowRankedAccountsByBalance()
        {
            Console.WriteLine("=== XẾP HẠNG ACCOUNT THEO SỐ DƯ ===\n");

            var rankedAccounts = _accountService.GetAccountsRankedByBalance();
            _logger.LogInformation("Showing ranked accounts by balance. Count: {AccountCount}.", rankedAccounts.Count);
            if (rankedAccounts.Count == 0)
            {
                Console.WriteLine("Chưa có tài khoản nào.");
                return;
            }

            int rank = 1;
            foreach (var account in rankedAccounts)
            {
                Console.WriteLine($"#{rank} - {account.Name} (ID: {account.Id})");
                Console.WriteLine($"  Tổng số dư: {account.TotalBalance:N0} VND");
                Console.WriteLine($"  Tiết kiệm: {account.SavingsBalance:N0} VND | Thanh toán: {account.CheckingBalance:N0} VND");
                Console.WriteLine(new string('-', 40));
                rank++;
            }
        }

        public void ShowAccountsBelowOneMillion()
        {
            const decimal threshold = 1_000_000m;

            Console.WriteLine("=== ACCOUNT CÓ SỐ DƯ DƯỚI 1 TRIỆU ===\n");

            var lowBalanceAccounts = _accountService.GetAccountsBelowBalance(threshold);
            _logger.LogInformation("Showing accounts below threshold {Threshold}. Count: {AccountCount}.", threshold, lowBalanceAccounts.Count);
            if (lowBalanceAccounts.Count == 0)
            {
                Console.WriteLine("Không có account nào dưới 1,000,000 VND.");
                return;
            }

            foreach (var account in lowBalanceAccounts)
            {
                ShowAccountDetails(account);
                Console.WriteLine(new string('-', 40));
            }
        }

        public void ShowTop10CheckingAccounts()
        {
            Console.WriteLine("=== TOP 10 ACCOUNT CÓ SỐ DƯ THANH TOÁN LỚN NHẤT ===\n");

            var topCheckingAccounts = _accountService.GetTopCheckingAccounts(10);
            _logger.LogInformation("Showing top checking accounts. Count: {AccountCount}.", topCheckingAccounts.Count);
            if (topCheckingAccounts.Count == 0)
            {
                Console.WriteLine("Chưa có tài khoản nào.");
                return;
            }

            int rank = 1;
            foreach (var account in topCheckingAccounts)
            {
                Console.WriteLine($"#{rank} - {account.Name} (ID: {account.Id})");
                Console.WriteLine($"  Số dư thanh toán: {account.CheckingBalance:N0} VND");
                Console.WriteLine(new string('-', 40));
                rank++;
            }
        }

        public void ShowTotalInvestmentBalance()
        {
            Console.WriteLine("=== TỔNG SỐ DƯ TÀI KHOẢN ĐẦU TƯ ===\n");

            decimal totalInvestmentBalance = _accountService.GetTotalInvestmentBalance();
            _logger.LogInformation("Showing total investment balance: {TotalInvestmentBalance}.", totalInvestmentBalance);
            Console.WriteLine($"Tổng số dư tài khoản đầu tư (tiết kiệm): {totalInvestmentBalance:N0} VND");
        }

        #endregion
    }
}
