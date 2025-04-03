namespace ArtAttack.Domain
{
    public class PredefinedContract
    {
        public int ContractID { get; set; }
        public required string ContractContent { get; set; }
    }

    public enum PredefinedContractType
    {
        BuyingContract = 1,
        SellingContract = 2,
        BorrowingContract = 3
    }
}
