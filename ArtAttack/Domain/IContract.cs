namespace ArtAttack.Domain
{
    public interface IContract
    {
        string AdditionalTerms { get; set; }
        string ContractContent { get; set; }
        long ContractID { get; set; }
        string ContractStatus { get; set; }
        int OrderID { get; set; }
        int PDFID { get; set; }
        int? PredefinedContractID { get; set; }
        int RenewalCount { get; set; }
        long? RenewedFromContractID { get; set; }
    }
}