using System.Text.RegularExpressions;

namespace Contact_Management_system
{
    public class Validations
    {
        public bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }
        public string ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            if (password.Length < 8)
                return "Password must be at least 8 characters long.";

            if (!Regex.IsMatch(password, "[A-Z]"))
                return "Password must contain at least one uppercase letter.";

            if (!Regex.IsMatch(password, "[a-z]"))
                return "Password must contain at least one lowercase letter.";

            if (!Regex.IsMatch(password, "[0-9]"))
                return "Password must contain at least one digit.";

            if (!Regex.IsMatch(password, "[^A-Za-z0-9]"))
                return "Password must contain at least one special character.";

            return "Valid";
        }
        public bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            if (!Regex.IsMatch(phoneNumber, @"^\d+$"))
                return false;

            return true;
        }
    }
}
