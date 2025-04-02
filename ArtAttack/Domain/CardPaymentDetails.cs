namespace ArtAttack.Domain
{
    public class CardPaymentDetails
    {
        required public string ID { get; set; }
        required public string CardholderName { get; set; }
        required public string CardNumber { get; set; }
        required public string Month { get; set; }
        required public string Year { get; set; }
        required public string Cvc { get; set; }
        required public string Country { get; set; }
        required public float Balance { get; set; }
    }
}
