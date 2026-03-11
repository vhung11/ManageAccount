# ManageAccount - Console app quản lý tài khoản ngân hàng
Được xây dựng bằng .NET 10.0,app cho phép quản lý các tài khoản ngân hàng với các chức năng như:
- Tạo, xóa tài khoản
- Nộp, rút tiền từ tài khoản (tiết kiệm/thanh toán)
- Tính lãi suất
- Truy vấn dữ liệu

Sử dụng **Entity Framework Core** với **Oracle Database** để lưu trữ và quản lý dữ liệu.

## 🎯 Áp Dụng Các Tính Chất OOP

### 1. **Đóng gói**
## Tính đóng gói trong `AccountService`
- Các repository được khai báo **`private`** không cho các lớp bên ngoài truy cập trực tiếp.
```csharp
  private readonly IAccountRepository _accountRepo;
```
- Chỉ thao tác dữ liệu thông qua các **public method**
```csharp
public AccountDTO? GetAccountById(int accountId)
```
- Các xử lý nội bộ được đặt trong **private method**
```csharp
private InterestType EnsureInterestType(decimal rate)
```
→ Nhờ đó, logic và truy cập dữ liệu được **ẩn bên trong service**.

### 2. **Abstraction (Trừu tượng hóa)**
- **Interfaces**: Định nghĩa contracts cho repositories
  - `IAccountRepository`
  - `IAccountBalanceRepository`
  - `IInterestTypeRepository`
- Ẩn đi chi tiết implementation, chỉ expose các phương thức cần thiết

```csharp
public interface IAccountRepository
{
    Account? GetById(int id);
    IEnumerable<Account> GetAll();
    Account Add(Account account);
    void Update(Account account);
    void Delete(Account account);
    // ... more methods
}
```

### 3. **Inheritance (Kế thừa)**
- `ApplicationDbContext` kế thừa từ `DbContext` của Entity Framework
- Tái sử dụng và mở rộng chức năng của lớp cha

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    // ... override methods
}
```

### 4. **Polymorphism (Đa hình)**
- Trong dự án, đa hình được thể hiện qua **method overriding** khi kế thừa `DbContext`.
- `ApplicationDbContext` ghi đè các phương thức `OnConfiguring` và `OnModelCreating` của lớp cha để tùy biến hành vi theo nhu cầu của ứng dụng.
- Khi EF Core gọi các phương thức này ở runtime, phiên bản đã override trong `ApplicationDbContext` sẽ được thực thi.

```csharp
public class ApplicationDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // custom Oracle configuration
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // custom entity mapping and precision rules
    }
}
```

## 🔍 Sử Dụng LINQ

### 1. **Query Data với LINQ to Entities**
```csharp
// Trong AccountRepository.cs
public Account? GetById(int id)
{
    return _context.Accounts.FirstOrDefault(a => a.Id == id);
}

public bool Exists(int id)
{
    return _context.Accounts.Any(a => a.Id == id);
}
```

### 2. **Filtering và Ordering**
```csharp
// Trong AccountService.cs
public List<AccountDTO> GetAccountsBelowBalance(decimal threshold)
{
    return GetAllAccounts()
        .Where(account => account.TotalBalance < threshold)
        .ToList();
}

public List<AccountDTO> GetAccountsRankedByBalance()
{
    return GetAllAccounts()
        .OrderByDescending(account => account.TotalBalance)
        .ToList();
}
```

### 3. **Aggregation Operations**
```csharp
// Tính tổng số dư tiết kiệm
public decimal GetTotalInvestmentBalance()
{
    return GetAllAccounts().Sum(account => account.SavingsBalance);
}
```

### 4. **Limiting Results**
```csharp
// Lấy top N tài khoản
public List<AccountDTO> GetTopCheckingAccounts(int topCount)
{
    return GetAllAccounts()
        .OrderByDescending(account => account.CheckingBalance)
        .Take(topCount)
        .ToList();
}
```

## 🗄️ Kết Nối Với Oracle Database

### 1. **Cấu Hình Connection String**
File `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=account_app;Password=Hung2003;Data Source=localhost:1521/FREEPDB1"
  }
}
```

### 2. **DbContext Configuration**
Trong `ApplicationDbContext.cs`:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("OracleConnection");
        optionsBuilder.UseOracle(connectionString);
    }
}
```

### 3. **Entity Relationships**
```csharp
// One-to-Many: Account -> AccountBalances
modelBuilder.Entity<Account>()
    .HasMany(a => a.AccountBalances)
    .WithOne(ab => ab.Account)
    .HasForeignKey(ab => ab.AccountId)
    .OnDelete(DeleteBehavior.Cascade);

// Many-to-One: AccountBalance -> InterestType
modelBuilder.Entity<AccountBalance>()
    .HasOne(ab => ab.InterestType)
    .WithMany(it => it.AccountBalances)
    .HasForeignKey(ab => ab.InterestTypeId)
    .OnDelete(DeleteBehavior.Restrict);
```

### 5. **Database Migrations**
Sử dụng EF Core Migrations để quản lý schema:
```bash
# Tạo migration
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update
```

## 📝 Áp dụng Logging

Ứng dụng sử dụng **NLog** và tích hợp qua **Microsoft.Extensions.Logging** để vừa log ra console, vừa lưu file log theo ngày.

### 1. **Khởi tạo logging trong Program.cs**
- Nạp cấu hình từ file `nlog.config`.
- Xóa các logging provider mặc định và chỉ dùng NLog.
- Cấu hình mức log tối thiểu ở tầng `Microsoft.Extensions.Logging` là `Debug`.
- Bật các option:
    - `CaptureMessageTemplates = true`
    - `CaptureMessageProperties = true`
    - `IncludeScopes = true`

### 2. **Targets và định dạng log trong nlog.config**
- **Console target** (`ColoredConsole`): hiển thị log trực tiếp khi chạy app.
- **File target** (`File`): ghi log theo ngày vào thư mục `logs/`.
    - Tên file: `logs/manageaccount-${shortdate}.log`
    - Archive theo ngày tại `logs/archives/`
    - Giữ tối đa `14` file archive
- Layout log đang dùng:
    - `${longdate}|${level:uppercase=true}|${logger}|${message}`
    - Nếu có exception, tự động nối thêm thông tin exception.

### 3. **Quy tắc lọc log (rules)**
- `Microsoft.EntityFrameworkCore.*`: chặn log nhiễu mức `Info` trở xuống (`final=true`) để giảm noise từ EF Core.
- `Microsoft.*`: chỉ ghi từ mức `Warn` trở lên.
- `*` (toàn bộ logger ứng dụng): ghi từ mức `Info` trở lên ra cả console và file.

### 4. **Vòng đời và xử lý lỗi logging**
- App ghi các mốc quan trọng như:
    - Bắt đầu ứng dụng
    - Khởi tạo database và seed dữ liệu
    - Dừng ứng dụng bình thường
    - Các hoạt động, sự kiện do người dùng tạo ra
- Cuối chương trình gọi `LogManager.Shutdown()` để flush và đóng tài nguyên logging an toàn.

### 5. **Internal log của NLog**
- NLog internal diagnostics được ghi tại: `logs/internal-nlog.txt`.

## 🚀 Cách Chạy Ứng Dụng

### Yêu Cầu
1. Cài đặt Oracle Database (hoặc Oracle Database Free)
2. .NET 10.0 SDK
3. Tạo user và database theo connection string

### Các Bước
1. Clone repository
2. Cập nhật connection string trong `appsettings.json`
3. Chạy migrations (nếu cần):
   ```bash
   dotnet ef database update
   ```
4. Build và chạy project:
   ```bash
   dotnet build
   dotnet run
   ```