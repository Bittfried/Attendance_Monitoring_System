namespace Attendance_Monitoring_System
{
    internal static class ScanCodeValidator
    {
        private const int RequiredLength = 10;

        public static bool IsValid(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode) || rawCode.Length != RequiredLength)
            {
                return false;
            }

            foreach (char character in rawCode)
            {
                bool isAsciiDigit = character >= '0' && character <= '9';
                bool isAsciiUppercase = character >= 'A' && character <= 'Z';
                bool isAsciiLowercase = character >= 'a' && character <= 'z';

                if (!isAsciiDigit && !isAsciiUppercase && !isAsciiLowercase)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
