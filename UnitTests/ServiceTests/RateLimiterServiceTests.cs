using FluentAssertions;
using HomeworkApp.Bll.Exceptions;
using HomeworkApp.Bll.Services;
using HomeworkApp.Dal.Repositories.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;

namespace UnitTests.ServiceTests;

public class RateLimiterServiceTests
{
    private Mock<IRateLimiterRepository> _rateLimiterRepositoryFake = new(MockBehavior.Strict);
    private RateLimiterService _rateLimiterService;

    public RateLimiterServiceTests()
    {
        _rateLimiterService = new RateLimiterService(_rateLimiterRepositoryFake.Object);
    }

    [Fact]
    public async Task ThrowIfTooManyRequests_UnderLimit_Success()
    {
        //Arrange
        _rateLimiterRepositoryFake
            .Setup(f => f.ThrowIfTooManyRequests(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var clientName = "client-ip";

        //Act
        await _rateLimiterService.ThrowIfTooManyRequests(clientName);

        //Assert
        _rateLimiterRepositoryFake.Verify(f => f.ThrowIfTooManyRequests(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ThrowIfTooManyRequests_AboveLimit_Success()
    {
        //Arrange
        _rateLimiterRepositoryFake
            .Setup(f => f.ThrowIfTooManyRequests(It.IsAny<string>()))
            .Throws(() => new InvalidOperationException());

        var clientName = "client-ip";

        //Act
        var act = async () => await _rateLimiterService.ThrowIfTooManyRequests(clientName);

        //Assert
        await act.Should().ThrowAsync<TooManyRequestsException>();
    }
}