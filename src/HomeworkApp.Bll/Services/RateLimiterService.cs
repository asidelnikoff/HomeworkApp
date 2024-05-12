using HomeworkApp.Bll.Exceptions;
using HomeworkApp.Bll.Services.Interfaces;
using HomeworkApp.Dal.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace HomeworkApp.Bll.Services;
public class RateLimiterService : IRateLimiterService
{
    private readonly IRateLimiterRepository _repository;

    public RateLimiterService(IRateLimiterRepository repository)
    {
        _repository = repository;
    }

    public async Task ThrowIfTooManyRequests(string clientName)
    {
        try
        {
            await _repository.ThrowIfTooManyRequests(clientName);
        }
        catch (InvalidOperationException ex)
        {
            throw new TooManyRequestsException("Too many requests", ex);
        }
    }
}