using ArtAttack.Model;

namespace ArtAttack.Services
{
    public class PaymentService
    {
        private readonly PaymentDetailValidator validator;
        private readonly DummyCardModel dummyCardModel;

        public PaymentService(string connectionString)
        {
            validator = new PaymentDetailValidator();
            dummyCardModel = new DummyCardModel(connectionString);
        }
    }
}
