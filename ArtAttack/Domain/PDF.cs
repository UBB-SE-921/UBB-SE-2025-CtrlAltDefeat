using System.Diagnostics.CodeAnalysis;

namespace ArtAttack.Domain
{
    [ExcludeFromCodeCoverage]
    public class PDF
    {
        public int ContractID { get; set; }
        public int PdfID { get; set; }
        public byte[] File { get; set; }
    }
}
