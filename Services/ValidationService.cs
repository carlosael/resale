using System.Text.RegularExpressions;

namespace ResaleApi.Services
{
    public class ValidationService
    {
        public static bool IsValidCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            // Remove non-numeric characters
            cnpj = Regex.Replace(cnpj, @"[^\d]", "");

            // Check if CNPJ has 14 digits
            if (cnpj.Length != 14)
                return false;

            // Check if all digits are the same
            if (cnpj.All(c => c == cnpj[0]))
                return false;

            // Calculate first check digit
            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += int.Parse(cnpj[i].ToString()) * multiplier1[i];
            }
            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (int.Parse(cnpj[12].ToString()) != digit1)
                return false;

            // Calculate second check digit
            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            sum = 0;
            for (int i = 0; i < 13; i++)
            {
                sum += int.Parse(cnpj[i].ToString()) * multiplier2[i];
            }
            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            return int.Parse(cnpj[13].ToString()) == digit2;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove non-numeric characters
            string cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");

            // Brazilian phone patterns: 
            // Mobile: 11 digits (with country code 55) or 9 digits (without country code)
            // Landline: 10 digits (with country code 55) or 8 digits (without country code)
            return cleanPhone.Length >= 8 && cleanPhone.Length <= 13;
        }

        public static string CleanCnpj(string cnpj)
        {
            return Regex.Replace(cnpj ?? "", @"[^\d]", "");
        }

        public static string CleanPhoneNumber(string phoneNumber)
        {
            return Regex.Replace(phoneNumber ?? "", @"[^\d]", "");
        }

        public static string FormatCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return "";

            cnpj = CleanCnpj(cnpj);
            if (cnpj.Length != 14)
                return cnpj;

            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "";

            phoneNumber = CleanPhoneNumber(phoneNumber);
            
            // Format Brazilian phone number
            if (phoneNumber.Length == 11) // Mobile with area code
                return $"({phoneNumber.Substring(0, 2)}) {phoneNumber.Substring(2, 5)}-{phoneNumber.Substring(7, 4)}";
            else if (phoneNumber.Length == 10) // Landline with area code
                return $"({phoneNumber.Substring(0, 2)}) {phoneNumber.Substring(2, 4)}-{phoneNumber.Substring(6, 4)}";
            
            return phoneNumber;
        }
    }
} 