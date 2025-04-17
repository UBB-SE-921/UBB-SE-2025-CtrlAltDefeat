using System.Threading.Tasks;

namespace ArtAttack.Service
{
    public interface IPDFService
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}