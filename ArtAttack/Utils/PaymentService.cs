using ArtAttack.Model;

namespace ArtAttack.Services
{
    public class PaymentService
    {
        private readonly PaymentDetailValidator _validator;
        private readonly DummyCardModel _dummyCardModel;

        public PaymentService(string connectionString)
        {
            _validator = new PaymentDetailValidator();
            _dummyCardModel = new DummyCardModel(connectionString);
        }

        //public PaymentStatus ProcessCardPayment(string cardNumber, string cvc, string month, string year, float orderTotal)
        //{
        //    float cardBalance = _dummyCardModel.GetCardBalance(cardNumber);

        //    if (!_validator.ValidateCardDetails(cardNumber, cvc, month, year) || _dummyCardModel.GetCardBalance(cardNumber)==-1)
        //        return PaymentStatus.FAILED_INVALID_CARD;

        //    if(cardBalance < orderTotal)
        //    {
        //        return PaymentStatus.FAILED_INSUFFICIENT_FUNDS;
        //    }

        //    return PaymentStatus.SUCCESS;

        //}

        // This should either be refactored or cut out entirely
    }
}
