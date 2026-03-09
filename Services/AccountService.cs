using ManageAccount.Data.DTOs;
using ManageAccount.Data.Entities;
using ManageAccount.Mappers;
using ManageAccount.Infrastructure.Repositories;
using Infrastructure.Repositories;

namespace ManageAccount.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IAccountBalanceRepository _accountBalanceRepo;
        private readonly IInterestTypeRepository _interestTypeRepo;

        public AccountService(
            IAccountRepository accountRepo,
            IAccountBalanceRepository accountBalanceRepo,
            IInterestTypeRepository interestTypeRepo)
        {
            _accountRepo = accountRepo;
            _accountBalanceRepo = accountBalanceRepo;
            _interestTypeRepo = interestTypeRepo;
        }

        #region Query Operations

        /// <summary>
        /// Lấy thông tin chi tiết tài khoản theo ID
        /// </summary>
        /// <param name="accountId">ID của tài khoản</param>
        /// <returns>AccountDTO hoặc null nếu không tìm thấy</returns>
        public AccountDTO? GetAccountById(int accountId)
        {
            var account = _accountRepo.GetById(accountId);
            if (account == null)
            {
                return null;
            }

            var accountBalances = _accountBalanceRepo.GetByAccountId(accountId);
            return AccountMapper.ToDTO(account, accountBalances);
        }

        /// <summary>
        /// Lấy danh sách tất cả tài khoản (trả về DTO)
        /// </summary>
        /// <returns>Danh sách AccountDTO</returns>
        public List<AccountDTO> GetAllAccounts()
        {
            var accounts = _accountRepo.GetAll();
            var allBalances = _accountBalanceRepo.GetAll();
            
            return AccountMapper.ToDTOList(accounts, allBalances);
        }

        /// <summary>
        /// Kiểm tra tài khoản có tồn tại hay không
        /// </summary>
        public bool AccountExists(int accountId)
        {
            return _accountRepo.Exists(accountId);
        }

        public List<AccountDTO> GetAccountsRankedByBalance()
        {
            return GetAllAccounts()
                .OrderByDescending(account => account.TotalBalance)
                .ToList();
        }

        public List<AccountDTO> GetAccountsBelowBalance(decimal threshold)
        {
            return GetAllAccounts()
                .Where(account => account.TotalBalance < threshold)
                .ToList();
        }

        public List<AccountDTO> GetTopCheckingAccounts(int topCount)
        {
            if (topCount <= 0)
            {
                return new List<AccountDTO>();
            }

            return GetAllAccounts()
                .OrderByDescending(account => account.CheckingBalance)
                .Take(topCount)
                .ToList();
        }

        public decimal GetTotalInvestmentBalance()
        {
            return GetAllAccounts().Sum(account => account.SavingsBalance);
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Thêm tài khoản mới
        /// </summary>
        /// <param name="name">Tên chủ tài khoản</param>
        /// <param name="balance">Số dư ban đầu</param>
        /// <returns>ID của tài khoản mới tạo, hoặc 0 nếu thất bại</returns>
        public int AddAccount(string name, decimal balance)
        {
            try
            {
                var savingsInterest = EnsureInterestType(4.7m);
                var checkingInterest = EnsureInterestType(5.1m);

                // Thêm Account
                var addedAccount = _accountRepo.Add(new Account { Name = name });
                _accountRepo.SaveChanges();

                // Tạo 2 AccountBalance: Savings và Checking
                var savingsBalance = new AccountBalance
                {
                    AccountId = addedAccount.Id,
                    Type = "Tài khoản tiết kiệm",
                    Balance = balance / 2,
                    InterestTypeId = savingsInterest.Id
                };

                var checkingBalance = new AccountBalance
                {
                    AccountId = addedAccount.Id,
                    Type = "Tài khoản thanh toán",
                    Balance = balance / 2,
                    InterestTypeId = checkingInterest.Id
                };

                _accountBalanceRepo.Add(savingsBalance);
                _accountBalanceRepo.Add(checkingBalance);
                _accountBalanceRepo.SaveChanges();

                Console.WriteLine($"✓ Đã thêm tài khoản: {name} với số dư {balance:N0} VNĐ");
                return addedAccount.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi khi thêm tài khoản: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản
        /// </summary>
        public bool UpdateAccountName(int accountId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("✗ Tên mới không được để trống!");
                return false;
            }

            var account = _accountRepo.GetById(accountId);
            if (account == null)
            {
                Console.WriteLine($"✗ Không tìm thấy tài khoản ID: {accountId}");
                return false;
            }

            try
            {
                account.Name = newName;
                _accountRepo.Update(account);
                _accountRepo.SaveChanges();

                Console.WriteLine($"✓ Đã cập nhật tên tài khoản thành: {newName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi khi cập nhật: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        public bool DeleteAccount(int accountId)
        {
            var account = _accountRepo.GetById(accountId);
            if (account == null)
            {
                Console.WriteLine($"✗ Không tìm thấy tài khoản ID: {accountId}");
                return false;
            }

            try
            {
                _accountRepo.Delete(account);
                _accountRepo.SaveChanges();

                Console.WriteLine($"✓ Đã xóa tài khoản ID: {accountId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi khi xóa tài khoản: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Transaction Operations

        /// <summary>
        /// Nạp tiền vào tài khoản tiết kiệm
        /// </summary>
        public bool DepositToSavings(int accountId, decimal amount)
        {
            var savingsBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản tiết kiệm");
            if (savingsBalance == null)
            {
                Console.WriteLine("✗ Không tìm thấy tài khoản tiết kiệm.");
                return false;
            }

            try
            {
                savingsBalance.Balance += amount;
                _accountBalanceRepo.Update(savingsBalance);
                _accountBalanceRepo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Nạp tiền vào tài khoản thanh toán
        /// </summary>
        public bool DepositToChecking(int accountId, decimal amount)
        {
            var checkingBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản thanh toán");
            if (checkingBalance == null)
            {
                Console.WriteLine("✗ Không tìm thấy tài khoản thanh toán.");
                return false;
            }

            try
            {
                checkingBalance.Balance += amount;
                _accountBalanceRepo.Update(checkingBalance);
                _accountBalanceRepo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Rút tiền từ tài khoản tiết kiệm
        /// </summary>
        public bool WithdrawFromSavings(int accountId, decimal amount)
        {
            var savingsBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản tiết kiệm");
            if (savingsBalance == null)
            {
                Console.WriteLine("✗ Không tìm thấy tài khoản tiết kiệm.");
                return false;
            }

            if (savingsBalance.Balance < amount)
            {
                return false;
            }

            try
            {
                savingsBalance.Balance -= amount;
                _accountBalanceRepo.Update(savingsBalance);
                _accountBalanceRepo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Rút tiền từ tài khoản thanh toán
        /// </summary>
        public bool WithdrawFromChecking(int accountId, decimal amount)
        {
            var checkingBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản thanh toán");
            if (checkingBalance == null)
            {
                Console.WriteLine("✗ Không tìm thấy tài khoản thanh toán.");
                return false;
            }

            if (checkingBalance.Balance < amount)
            {
                return false;
            }

            try
            {
                checkingBalance.Balance -= amount;
                _accountBalanceRepo.Update(checkingBalance);
                _accountBalanceRepo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
                return false;
            }
        }

        #endregion

        public void ApplyInterestToAllAccounts()
        {
            var allBalances = _accountBalanceRepo.GetAll();

            foreach (var balance in allBalances)
            {
                var interestType = _interestTypeRepo.GetById(balance.InterestTypeId);
                if (interestType == null)
                {
                    Console.WriteLine($"⚠ Không tìm thấy InterestType cho AccountBalance ID: {balance.Id}");
                    continue;
                }

                var rate = interestType.Rate > 1 ? interestType.Rate / 100m : interestType.Rate;
                balance.Balance += balance.Balance * rate;
                _accountBalanceRepo.Update(balance);
            }

            _accountBalanceRepo.SaveChanges();
        }

        private InterestType EnsureInterestType(decimal rate)
        {
            var interestType = _interestTypeRepo.GetByRate(rate);
            if (interestType != null)
            {
                return interestType;
            }

            interestType = _interestTypeRepo.Add(new InterestType { Rate = rate });
            _interestTypeRepo.SaveChanges();
            return interestType;
        }
    }
}