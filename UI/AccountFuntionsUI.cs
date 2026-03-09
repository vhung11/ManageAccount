using ManageAccount.Data.DTOs;
using ManageAccount.Helpers;
using ManageAccount.Services;

namespace ManageAccount.UI
{
    public class AccountFunctionsUI
    {
        #region Fields

        private readonly AccountService _accountService;

        #endregion

        #region Constructor

        public AccountFunctionsUI(AccountService accountService)
        {
            _accountService = accountService;
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

            int newId = _accountService.AddAccount(name, balance);
            if (newId <= 0)
            {
                Console.WriteLine("✗ Thêm tài khoản thất bại.");
                return;
            }

            Console.WriteLine("\n✓ Thêm tài khoản thành công!");
            Console.WriteLine($"  - ID: {newId}");
            Console.WriteLine($"  - Tài khoản tiết kiệm: {balance / 2:N0} VND");
            Console.WriteLine($"  - Tài khoản thanh toán: {balance / 2:N0} VND");
        }

        public void DeleteAccount()
        {
            Console.WriteLine("=== XÓA TÀI KHOẢN ===");

            int id = InputHelper.ReadInt("Nhập ID tài khoản cần xóa: ");

            if (_accountService.DeleteAccount(id))
                Console.WriteLine("✓ Xóa tài khoản thành công!");
            else
                Console.WriteLine("✗ Không tìm thấy tài khoản với ID này.");
        }

        #endregion

        #region Transaction Methods

        public void Deposit()
        {
            Console.WriteLine("=== NỘP TIỀN ===");

            int id = InputHelper.ReadInt("Nhập ID: ");

            var account = _accountService.GetAllAccounts().FirstOrDefault(a => a.Id == id);
            if (account == null)
            {
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
                Console.WriteLine("✗ Giao dịch thất bại.");
                return;
            }
            
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

            var account = _accountService.GetAllAccounts().FirstOrDefault(a => a.Id == id);
            if (account == null)
            {
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
                // Lấy dữ liệu tài khoản đã cập nhật sau giao dịch
                var updatedAccount = _accountService.GetAccountById(id);
                if (updatedAccount != null)
                {
                    ShowAccountDetails(updatedAccount);
                }
            }
        }

        public void ApplyInterest()
        {
            Console.WriteLine("=== TÍNH LÃI SUẤT ===");
            Console.WriteLine("Áp dụng lãi suất cho tất cả tài khoản...");

            _accountService.ApplyInterestToAllAccounts();

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
            Console.WriteLine($"Tổng số dư tài khoản đầu tư (tiết kiệm): {totalInvestmentBalance:N0} VND");
        }

        #endregion
    }
}
