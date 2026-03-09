using System.Text;

namespace ManageAccount.UI
{
    public class ConsoleUI
    {
        private readonly AccountFunctionsUI _functionsUI;

        public ConsoleUI(AccountFunctionsUI functionsUI)
        {
            _functionsUI = functionsUI;
        }

        public void Run()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            bool isRunning = true;

            while (isRunning)
            {
                ShowMenu();
                string choice = Console.ReadLine() ?? "";

                Console.Clear();

                switch (choice)
                {
                    case "1":
                        _functionsUI.ShowAllAccounts();
                        break;

                    case "2":
                        _functionsUI.AddAccount();
                        break;

                    case "3":
                        _functionsUI.DeleteAccount();
                        break;

                    case "4":
                        _functionsUI.Deposit();
                        break;

                    case "5":
                        _functionsUI.Withdraw();
                        break;

                    case "6":
                        _functionsUI.ApplyInterest();
                        break;

                    case "7":
                        _functionsUI.ShowRankedAccountsByBalance();
                        break;

                    case "8":
                        _functionsUI.ShowAccountsBelowOneMillion();
                        break;

                    case "9":
                        _functionsUI.ShowTop10CheckingAccounts();
                        break;

                    case "10":
                        _functionsUI.ShowTotalInvestmentBalance();
                        break;

                    case "0":
                        isRunning = false;
                        Console.WriteLine("Thoát chương trình...");
                        continue;

                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }

                Pause();
            }
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("=== QUẢN LÝ TÀI KHOẢN ===");
            Console.WriteLine("1. Xem danh sách tài khoản");
            Console.WriteLine("2. Thêm tài khoản mới");
            Console.WriteLine("3. Xóa tài khoản");
            Console.WriteLine("4. Nộp tiền");
            Console.WriteLine("5. Rút tiền");
            Console.WriteLine("6. Tính lãi suất");
            Console.WriteLine("7. Xếp hạng account theo số dư");
            Console.WriteLine("8. Hiển thị account dưới 1 triệu");
            Console.WriteLine("9. Top 10 account có số dư thanh toán lớn nhất");
            Console.WriteLine("10. Tổng số dư tài khoản đầu tư");
            Console.WriteLine("0. Thoát");
            Console.Write("Chọn chức năng (0-10): ");
        }

        private void Pause()
        {
            Console.WriteLine("\nNhấn phím bất kỳ để quay lại menu...");
            Console.ReadKey();
        }
    }
}
