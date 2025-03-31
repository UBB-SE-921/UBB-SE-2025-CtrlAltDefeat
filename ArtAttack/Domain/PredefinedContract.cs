namespace ArtAttack.Domain
{
    public class PredefinedContract
    {
        public int ID { get; set; }
        public required string Content { get; set; }
    }

    public enum PredefinedContractType
    {
        Buying = 1,
        Selling = 2,
        Borrowing = 3
    }
}
