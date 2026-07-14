using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class CachingBehaviorTests
{
    private readonly HybridCache _cache = Substitute.For<HybridCache>();
    private readonly ILogger<CachingBehavior<CachedQuery, Result<string>>> _logger = Substitute.For<
        ILogger<CachingBehavior<CachedQuery, Result<string>>>
    >();

    private readonly CachingBehavior<CachedQuery, Result<string>> _sut;

    public CachingBehaviorTests()
    {
        _sut = new CachingBehavior<CachedQuery, Result<string>>(_cache, _logger);
    }

    [Fact]
    public async Task Handle_WhenNotCachedQuery_ShouldSkipCacheAndReturnResult()
    {
        // Arrange
        var uncachedRequest = new NonCachedQuery();
        var behavior = new CachingBehavior<NonCachedQuery, string>(
            _cache,
            Substitute.For<ILogger<CachingBehavior<NonCachedQuery, string>>>());

        // Act
        var result = await behavior.Handle(
            uncachedRequest,
            _ => Task.FromResult("OK"),
            CancellationToken.None);

        // Assert

        Assert.Equal("OK", result);
        await _cache
            .DidNotReceive()
            .SetAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<string[]>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsSuccess_ShouldCachedResult()
    {
        // Arrange
        var cachedRequest = new CachedQuery();
        var response = (Result<string>)"test-result";

        string? actualCacheKey = null;
        object? actualValue = null;
        HybridCacheEntryOptions? actualOptions = null;
        string[]? actualTags = null;
        CancellationToken actualCancellationToken = default;

        _cache
            .SetAsync(
                Arg.Do<string>(k => actualCacheKey = k),
                Arg.Do<object>(v => actualValue = v),
                Arg.Do<HybridCacheEntryOptions>(o => actualOptions = o),
                Arg.Do<string[]>(t => actualTags = t),
                Arg.Do<CancellationToken>(ct => actualCancellationToken = ct))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _sut.Handle(
            cachedRequest,
            _ => Task.FromResult(response),
            CancellationToken.None);

        // Assert
        Assert.Equal(response, result);
        Assert.Equal(cachedRequest.CacheKey, actualCacheKey);

        var typed = Assert.IsType<Result<string>>(actualValue);
        Assert.True(typed.IsSuccess);
        Assert.Equal("test-result", typed.Value);

        Assert.Equal(cachedRequest.Expiration, actualOptions?.Expiration);
        Assert.Equal(cachedRequest.Tags, actualTags);
    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsError_ShouldNotCacheResult()
    {
        // Arrange
        var request = new CachedQuery();
        var errorResult = (Result<string>)Error.Validation("code", "message");

        // Act

        var result = await _sut.Handle(
            request,
            _ => Task.FromResult(errorResult),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        var calls = _cache.ReceivedCalls();
        var setCalls = calls.Where(call =>
            call.GetMethodInfo().Name == nameof(HybridCache.SetAsync)
            && call.GetMethodInfo().IsGenericMethod
            && call.GetMethodInfo().GetGenericArguments().FirstOrDefault() == typeof(Result<string>));

        Assert.Empty(setCalls);
    }

    public class NonCachedQuery;

    public class CachedQuery : ICachedQuery
    {
        public string CacheKey => "test-key";
        public TimeSpan Expiration => TimeSpan.FromMinutes(5);
        public string[] Tags => ["unit-test"];
    }
}
