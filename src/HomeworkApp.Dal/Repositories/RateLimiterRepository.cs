using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories;

public class RateLimiterRepository : RedisRepository, IRateLimiterRepository
{
    protected override string KeyPrefix => "rate-limit";
    protected override TimeSpan KeyTtl => TimeSpan.FromMinutes(1);

    private const int requestsPerMinute = 100;

    public RateLimiterRepository(IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task ThrowIfTooManyRequests(string clientName)
    {
        var key = GetKey(clientName);

        var db = await GetConnection();

        if (!db.KeyExists(key))
        {
            db.StringSet(key,
                requestsPerMinute,
                KeyTtl);
        }

        var actualRequests = db.StringDecrement(key);

        if (actualRequests > 0)
        {
            return;
        }

        throw new InvalidOperationException("Too many requests");
    }
}
