namespace ArtAttack.Domain
{
    public interface IPredefinedContract
    {
        int ContractID { get; set; }
        string ContractContent { get; set; }
        string Content { get; set; }
        int ID { get; set; }
    }
}