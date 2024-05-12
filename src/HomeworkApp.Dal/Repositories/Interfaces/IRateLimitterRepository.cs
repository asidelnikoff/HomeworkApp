using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories.Interfaces;

public interface IRateLimiterRepository
{
    public Task ThrowIfTooManyRequests(string clientName);
}
