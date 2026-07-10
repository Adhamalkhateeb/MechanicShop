using MechanicShop.Application.Common.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class UnhandledExceptionBehavior
{
    private readonly ILogger<TestRequest> _logger = Substitute.For<ILogger<TestRequest>>();

    private readonly UnhandledExceptionBehavior<TestRequest, string> _sut;

    public UnhandledExceptionBehavior()
    {
        _sut = new UnhandledExceptionBehavior<TestRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_LogsErrorAndRethrows()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new InvalidOperationException("Test Exception");
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke(TestContext.Current.CancellationToken)
            .Returns<Task<string>>(_ => throw exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(request, next, TestContext.Current.CancellationToken)
        );

        Assert.Equal(exception, ex);
        _logger
            .Received(1)
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Unhandled Exception")),
                exception,
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task Handle_WhenNoException_InvokesNextAndReturnsResult()
    {
        // Arrange
        var request = new TestRequest();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke(TestContext.Current.CancellationToken).Returns("OK");

        // Act
        var result = await _sut.Handle(request, next, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("OK", result);
    }

    public class TestRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
