using System;
using System.Linq;

namespace ArtAttack.Services
{
    public class PaymentDetailValidator
    {
        /// <summary>
        /// Validates the card number using the Luhn algorithm
        /// </summary>
        /// <param name="cardNumber">The card number of the card to be validated</param>
        /// <returns></returns>
        public bool ValidateCardNumber(string cardNumber)
        {
            // luhn algorithm implementation for card number validation
            int numberOfDigits = cardNumber.Length;

            if (!cardNumber.All(char.IsAsciiDigit) || numberOfDigits != 16)
            {
                return false;
            }

            int nSum = 0;
            bool isSecond = false;
            for (int i = numberOfDigits - 1; i >= 0; i--)
            {
                int d = cardNumber[i] - '0';

                if (isSecond == true)
                {
                    d = d * 2;
                }

                // adding in case the doubling results in a 2 digit number
                nSum += d / 10;
                nSum += d % 10;

                isSecond = !isSecond;
            }
            return nSum % 10 == 0;
        }

        /// <summary>
        /// Validates the CVC code
        /// </summary>
        /// <param name="cvc">The cvc to be validated</param>
        /// <returns></returns>
        public bool ValidateCVC(string cvc)
        {
            return cvc.All(char.IsAsciiDigit) && cvc.Length == 3;
        }

        /// <summary>
        /// Validates the month
        /// </summary>
        /// <param name="month">The month to be validated</param>
        /// <returns></returns>
        public bool ValidateMonth(string month)
        {
            return month.All(char.IsAsciiDigit) && month.Length == 2;
        }

        /// <summary>
        /// Validates the year
        /// </summary>
        /// <param name="year">The year of the card to be validated</param>
        /// <returns></returns>
        public bool ValidateYear(string year)
        {
            return year.All(char.IsAsciiDigit) && year.Length == 2;
        }

        /// <summary>
        /// Validates the expiry date
        /// </summary>
        /// <param name="month">Month to be validated</param>
        /// <param name="year">Year to be validated</param>
        /// <returns></returns>
        public bool ValidateExpiryDate(string month, string year)
        {
            if (!ValidateMonth(month) || !ValidateYear(year))
            {
                return false;
            }

            int currentYear = DateTime.Now.Year % 100;
            int currentMonth = DateTime.Now.Month;

            int cardYear = int.Parse(year);
            int cardMonth = int.Parse(month);

            return (cardYear > currentYear) || (cardYear == currentYear && cardMonth >= currentMonth);
        }

        /// <summary>
        /// Validates the card details
        /// </summary>
        /// <param name="cardNumber">The number of the card to be validated</param>
        /// <param name="cvc">The cvc of the card to be validated</param>
        /// <param name="month">The month of the card to be validated</param>
        /// <param name="year">The year of the card to be validated</param>
        /// <returns></returns>
        public bool ValidateCardDetails(string cardNumber, string cvc, string month, string year)
        {
            return ValidateCardNumber(cardNumber) &&
                ValidateCVC(cvc) &&
                ValidateExpiryDate(month, year);
        }
    }
}
