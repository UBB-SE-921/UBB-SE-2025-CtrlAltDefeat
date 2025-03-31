using System;
using System.Linq;

namespace ArtAttack.Services
{
    public class PaymentDetailValidator
    {
        public bool ValidateCardNumber(string cardNumber)
        {
            // luhn algorithm implementation for card number validation
            int numberOfDigits = cardNumber.Length;

            if (!cardNumber.All(char.IsAsciiDigit) || numberOfDigits != 16)
                return false;

            int nSum = 0;
            bool isSecond = false;
            for (int i = numberOfDigits - 1; i >= 0; i--)
            {
                int d = cardNumber[i] - '0';

                if (isSecond == true)
                    d = d * 2;

                // adding in case the doubling results in a 2 digit number
                nSum += d / 10;
                nSum += d % 10;

                isSecond = !isSecond;
            }
            return (nSum % 10 == 0);
        }

        public bool ValidateCVC(string cvc)
        {
            return (cvc.All(char.IsAsciiDigit) && cvc.Length == 3);
        }

        public bool ValidateMonth(string month)
        {
            return (month.All(char.IsAsciiDigit) && month.Length == 2);
        }

        public bool ValidateYear(string year)
        {
            return (year.All(char.IsAsciiDigit) && year.Length == 2);
        }

        public bool ValidateExpiryDate(string month, string year)
        {
            if (!ValidateMonth(month) || !ValidateYear(year))
                return false;

            int currentYear = DateTime.Now.Year % 100;
            int currentMonth = DateTime.Now.Month;

            int cardYear = int.Parse(year);
            int cardMonth = int.Parse(month);

            return (cardYear > currentYear) || (cardYear == currentYear && cardMonth >= currentMonth);
        }

        public bool ValidateCardDetails(string cardNumber, string cvc, string month, string year)
        {
            return ValidateCardNumber(cardNumber) &&
                ValidateCVC(cvc) &&
                ValidateExpiryDate(month, year);
        }
    }
}
