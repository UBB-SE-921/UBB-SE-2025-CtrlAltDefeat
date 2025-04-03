namespace ArtAttack.Domain
{
    public class PredefinedContract : IPredefinedContract
    {
        public int ID { get; set; }
        public required string Content { get; set; }
    }

    /// <summary>
    /// Enum for the predefined contract types.
    /// </summary>
    public enum PredefinedContractType
    {
        Buying = 1,
        Selling = 2,
        Borrowing = 3
    }
}
