namespace ArtAttack.Domain
{
    public enum PaymentStatus
    {
        SUCCESS = 0,
        FAILED_INSUFFICIENT_FUNDS = 1,
        FAILED_INVALID_CARD = 2
    }
}
