using System.Threading.Tasks;

namespace HomeworkApp.Bll.Services.Interfaces;

public interface IRateLimiterService
{
    public Task ThrowIfTooManyRequests(string clientName);
}
