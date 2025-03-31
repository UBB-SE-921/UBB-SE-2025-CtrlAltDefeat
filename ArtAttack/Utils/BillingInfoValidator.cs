using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArtAttack.Services
{
    class BillingInfoValidator
    {
        public bool ValidateFullName(string fullName)
        {
            return !fullName.IsNullOrEmpty();
        }

        public bool ValidateEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        public bool ValidatePhoneNumber(string phoneNumber)
        {
            return (phoneNumber.All(char.IsAsciiDigit)
                && phoneNumber.Length == 10
                && phoneNumber[0] == '0' && phoneNumber[1] == '7'
                );
        }

        public bool ValidateAddress(string address)
        {
            return !address.IsNullOrEmpty();
        }


        public bool ValidateBillingInfo(string fullName, string email, string phoneNumber, string address, string postalCode)
        {
            return ValidateFullName(fullName) &&
                   ValidateEmail(email) &&
                   ValidatePhoneNumber(phoneNumber) &&
                   ValidateAddress(address) &&
                   ValidatePostalCode(postalCode);
        }

        private bool ValidatePostalCode(string postalCode)
        {
            return (postalCode.All(char.IsAsciiDigit)
                && postalCode.Length == 6);
        }
    }
}
