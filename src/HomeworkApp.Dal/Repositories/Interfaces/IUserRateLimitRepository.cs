using System.Threading;
using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories.Interfaces;

public interface IUserRateLimitRepository
{
    Task<long> IncRequestPerMinute(long userId, CancellationToken token);
}