namespace ArtAttack.Domain
{
    public class CardPaymentDetails
    {
        required public string ID { get; set; }
        required public string cardholderName { get; set; }
        required public string cardNumber { get; set; }
        required public string month { get; set; }
        required public string year { get; set; }
        required public string cvc { get; set; }
        required public string country { get; set; }
        required public float balance { get; set; }


    }
}
