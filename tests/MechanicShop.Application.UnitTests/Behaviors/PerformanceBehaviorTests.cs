using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class PerformanceBehaviorTests
{
    private readonly ILogger<TestRequest> _logger = Substitute.For<ILogger<TestRequest>>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private readonly PerformanceBehavior<TestRequest, TestResponse> _sut;

    public PerformanceBehaviorTests()
    {
        _sut = new PerformanceBehavior<TestRequest, TestResponse>(_logger, _user, _identityService);
    }

    [Fact]
    public async Task Handle_WhenRequestTakesLessThan500Ms_ShouldNotLogWarning()
    {
        // Arrange
        var request = new TestRequest { Name = "Test Request" };
        var expectedResponse = new TestResponse { Result = "Test Response" };

        // Act
        var response = await _sut.Handle(
            request,
            _ => Task.FromResult(expectedResponse),
            CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        _logger
            .DidNotReceive()
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenRequestTakesMoreThan500Ms_ShouldLogWarning()
    {
        // Arrange
        var request = new TestRequest { Name = "Test Request" };
        var expectedResponse = new TestResponse { Result = "Test Response" };
        _user.Id.Returns("user-id");
        _identityService.GetUserNameAsync("user-id").Returns("adham");

        // Act
        var response = await _sut.Handle(
            request,
            async _ =>
            {
                await Task.Delay(600, CancellationToken.None); // Simulate a long-running request
                return expectedResponse;
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        _logger
            .Received(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.Contains("Long Running Request")
                    && o.ToString()!.Contains("TestRequest")
                    && o.ToString()!.Contains("user-id")
                    && o.ToString()!.Contains("adham")),
                null,
                Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Handle_WhenUserIdIsNullOrEmpty_ShouldLogWarningWithEmptyUserName(
        string? userId)
    {
        // Arrange
        var request = new TestRequest { Name = "Test Request" };
        var expectedResponse = new TestResponse { Result = "Test Response" };
        _user.Id.Returns(userId);

        // Act
        var response = await _sut.Handle(
            request,
            async _ =>
            {
                await Task.Delay(600, CancellationToken.None); // Simulate a long-running request
                return expectedResponse;
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        _logger
            .Received(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.Contains("Long Running Request")
                    && o.ToString()!.Contains("TestRequest")),
                null,
                Arg.Any<Func<object, Exception?, string>>());

        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldAlwaysReturnResponseFromNext()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.Handle(
            request,
            (_) => Task.FromResult(expectedResponse),
            cancellationToken);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldNotCatchException()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(request, (_) => throw expectedException, cancellationToken));

        Assert.Equal(expectedException, exception);
    }

    public class TestRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}
