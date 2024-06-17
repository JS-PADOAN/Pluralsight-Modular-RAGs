using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication.Services
{
    public interface ILLMService
    {
        Task<string> GenerateSummarySentence(string userQuery, List<ResultSearch> results);
    }
}
