namespace ArtAttack.Domain
{
    public class PredefinedContract : IPredefinedContract
    {
        public int ContractID { get; set; }
        public required string ContractContent { get; set; }
        public string Content { get; set; }
        public int ID { get; set; }
    }

    /// <summary>
    /// Enum for the predefined contract types.
    /// </summary>
    public enum PredefinedContractType
    {
        BuyingContract = 1,
        SellingContract = 2,
        BorrowContract = 3
    }
}
