using System.Threading.Tasks;

namespace ArtAttack.Repository
{
    public interface IPDFRepository
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}