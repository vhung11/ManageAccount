using ManageAccount.Data.DTOs;
using ManageAccount.Data.Entities;
using ManageAccount.Infrastructure.Repositories;
using ManageAccount.Mappers;
using Microsoft.Extensions.Logging;
using Infrastructure.Repositories;

namespace ManageAccount.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IAccountBalanceRepository _accountBalanceRepo;
        private readonly IInterestTypeRepository _interestTypeRepo;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepo,
            IAccountBalanceRepository accountBalanceRepo,
            IInterestTypeRepository interestTypeRepo,
            ILogger<AccountService> logger)
        {
            _accountRepo = accountRepo;
            _accountBalanceRepo = accountBalanceRepo;
            _interestTypeRepo = interestTypeRepo;
            _logger = logger;
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
                _logger.LogWarning("Account {AccountId} was not found.", accountId);
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

            var accountDtos = AccountMapper.ToDTOList(accounts, allBalances);
            _logger.LogDebug("Loaded {AccountCount} accounts from storage.", accountDtos.Count);

            return accountDtos;
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
            var rankedAccounts = GetAllAccounts()
                .OrderByDescending(account => account.TotalBalance)
                .ToList();

            _logger.LogDebug("Calculated balance ranking for {AccountCount} accounts.", rankedAccounts.Count);
            return rankedAccounts;
        }

        public List<AccountDTO> GetAccountsBelowBalance(decimal threshold)
        {
            var accounts = GetAllAccounts()
                .Where(account => account.TotalBalance < threshold)
                .ToList();

            _logger.LogDebug("Found {AccountCount} accounts below threshold {Threshold}.", accounts.Count, threshold);
            return accounts;
        }

        public List<AccountDTO> GetTopCheckingAccounts(int topCount)
        {
            if (topCount <= 0)
            {
                _logger.LogWarning("Top checking accounts requested with invalid topCount {TopCount}.", topCount);
                return new List<AccountDTO>();
            }

            var accounts = GetAllAccounts()
                .OrderByDescending(account => account.CheckingBalance)
                .Take(topCount)
                .ToList();

            _logger.LogDebug("Computed top {TopCount} checking accounts. Actual count {AccountCount}.", topCount, accounts.Count);
            return accounts;
        }

        public decimal GetTotalInvestmentBalance()
        {
            var totalBalance = GetAllAccounts().Sum(account => account.SavingsBalance);
            _logger.LogDebug("Calculated total investment balance: {TotalInvestmentBalance}.", totalBalance);
            return totalBalance;
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
                _logger.LogInformation("Creating account for {AccountName} with initial balance {InitialBalance}.", name, balance);

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

                _logger.LogInformation(
                    "Created account {AccountId} for {AccountName}. Savings balance {SavingsBalance}, checking balance {CheckingBalance}.",
                    addedAccount.Id,
                    name,
                    savingsBalance.Balance,
                    checkingBalance.Balance);

                return addedAccount.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create account for {AccountName}.", name);
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
                _logger.LogWarning("Update-account-name rejected for account {AccountId} because the new name is empty.", accountId);
                return false;
            }

            var account = _accountRepo.GetById(accountId);
            if (account == null)
            {
                _logger.LogWarning("Update-account-name failed because account {AccountId} was not found.", accountId);
                return false;
            }

            try
            {
                account.Name = newName;
                _accountRepo.Update(account);
                _accountRepo.SaveChanges();

                _logger.LogInformation("Updated account {AccountId} name to {AccountName}.", accountId, newName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update account {AccountId} name.", accountId);
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
                _logger.LogWarning("Delete-account failed because account {AccountId} was not found.", accountId);
                return false;
            }

            try
            {
                _accountRepo.Delete(account);
                _accountRepo.SaveChanges();

                _logger.LogInformation("Deleted account {AccountId}.", accountId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete account {AccountId}.", accountId);
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
            if (amount <= 0)
            {
                _logger.LogWarning("Savings deposit rejected for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var savingsBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản tiết kiệm");
            if (savingsBalance == null)
            {
                _logger.LogWarning("Savings deposit failed because savings balance for account {AccountId} was not found.", accountId);
                return false;
            }

            try
            {
                savingsBalance.Balance += amount;
                _accountBalanceRepo.Update(savingsBalance);
                _accountBalanceRepo.SaveChanges();

                _logger.LogInformation("Deposited {Amount} into savings account {AccountId}. New balance {NewBalance}.", amount, accountId, savingsBalance.Balance);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deposit into savings account {AccountId}.", accountId);
                return false;
            }
        }

        /// <summary>
        /// Nạp tiền vào tài khoản thanh toán
        /// </summary>
        public bool DepositToChecking(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Checking deposit rejected for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var checkingBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản thanh toán");
            if (checkingBalance == null)
            {
                _logger.LogWarning("Checking deposit failed because checking balance for account {AccountId} was not found.", accountId);
                return false;
            }

            try
            {
                checkingBalance.Balance += amount;
                _accountBalanceRepo.Update(checkingBalance);
                _accountBalanceRepo.SaveChanges();

                _logger.LogInformation("Deposited {Amount} into checking account {AccountId}. New balance {NewBalance}.", amount, accountId, checkingBalance.Balance);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deposit into checking account {AccountId}.", accountId);
                return false;
            }
        }

        /// <summary>
        /// Rút tiền từ tài khoản tiết kiệm
        /// </summary>
        public bool WithdrawFromSavings(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Savings withdrawal rejected for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var savingsBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản tiết kiệm");
            if (savingsBalance == null)
            {
                _logger.LogWarning("Savings withdrawal failed because savings balance for account {AccountId} was not found.", accountId);
                return false;
            }

            if (savingsBalance.Balance < amount)
            {
                _logger.LogWarning(
                    "Savings withdrawal rejected for account {AccountId}. Requested {Amount}, available {AvailableBalance}.",
                    accountId,
                    amount,
                    savingsBalance.Balance);
                return false;
            }

            try
            {
                savingsBalance.Balance -= amount;
                _accountBalanceRepo.Update(savingsBalance);
                _accountBalanceRepo.SaveChanges();

                _logger.LogInformation("Withdrew {Amount} from savings account {AccountId}. New balance {NewBalance}.", amount, accountId, savingsBalance.Balance);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to withdraw from savings account {AccountId}.", accountId);
                return false;
            }
        }

        /// <summary>
        /// Rút tiền từ tài khoản thanh toán
        /// </summary>
        public bool WithdrawFromChecking(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Checking withdrawal rejected for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var checkingBalance = _accountBalanceRepo.GetByAccountIdAndType(accountId, "Tài khoản thanh toán");
            if (checkingBalance == null)
            {
                _logger.LogWarning("Checking withdrawal failed because checking balance for account {AccountId} was not found.", accountId);
                return false;
            }

            if (checkingBalance.Balance < amount)
            {
                _logger.LogWarning(
                    "Checking withdrawal rejected for account {AccountId}. Requested {Amount}, available {AvailableBalance}.",
                    accountId,
                    amount,
                    checkingBalance.Balance);
                return false;
            }

            try
            {
                checkingBalance.Balance -= amount;
                _accountBalanceRepo.Update(checkingBalance);
                _accountBalanceRepo.SaveChanges();

                _logger.LogInformation("Withdrew {Amount} from checking account {AccountId}. New balance {NewBalance}.", amount, accountId, checkingBalance.Balance);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to withdraw from checking account {AccountId}.", accountId);
                return false;
            }
        }

        #endregion

        public void ApplyInterestToAllAccounts()
        {
            _logger.LogInformation("Applying interest to all account balances.");

            var allBalances = _accountBalanceRepo.GetAll().ToList();
            int updatedBalanceCount = 0;
            int skippedBalanceCount = 0;

            foreach (var balance in allBalances)
            {
                var interestType = _interestTypeRepo.GetById(balance.InterestTypeId);
                if (interestType == null)
                {
                    skippedBalanceCount++;
                    _logger.LogWarning("Skipped interest application because InterestType {InterestTypeId} was not found for balance {AccountBalanceId}.", balance.InterestTypeId, balance.Id);
                    continue;
                }

                var rate = interestType.Rate > 1 ? interestType.Rate / 100m : interestType.Rate;
                var previousBalance = balance.Balance;
                balance.Balance += balance.Balance * rate;
                _accountBalanceRepo.Update(balance);
                updatedBalanceCount++;

                _logger.LogDebug(
                    "Applied interest to balance {AccountBalanceId}. Previous {PreviousBalance}, new {NewBalance}, rate {Rate}.",
                    balance.Id,
                    previousBalance,
                    balance.Balance,
                    rate);
            }

            _accountBalanceRepo.SaveChanges();

            _logger.LogInformation(
                "Interest application completed. Updated {UpdatedBalanceCount} balances and skipped {SkippedBalanceCount}.",
                updatedBalanceCount,
                skippedBalanceCount);
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
            _logger.LogInformation("Created missing interest type with rate {InterestRate}.", rate);
            return interestType;
        }
    }
}