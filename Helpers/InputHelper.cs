namespace ManageAccount.Helpers
{
    public static class InputHelper
    {
        // Check số nguyên dương
        public static int ReadInt(string message)
        {
            int result;
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out result) && result > 0)
                    return result;
                Console.WriteLine("Vui lòng nhập số nguyên dương!");
            }
        }

        // Check số tiền không âm
        public static decimal ReadDecimal(string message)
        {
            decimal result;
            while (true)
            {
                Console.Write(message);
                if (decimal.TryParse(Console.ReadLine(), out result) && result >= 0)
                    return result;
                Console.WriteLine("Số tiền phải là số không âm!");
            }
        }

        // Check chuỗi không rỗng
        public static string ReadString(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(input))
                    return input;
                Console.WriteLine("Nội dung này không được để trống!");
            }
        }
    }
}