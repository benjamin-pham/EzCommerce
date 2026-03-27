using System.Text.Json;
using MyProject.API.Extensions;
using MyProject.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyProject.API.IntegrationTests.Infrastructure;

public sealed class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler =
        new(NullLogger<GlobalExceptionHandler>.Instance);

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithErrors()
    {
        var context = CreateHttpContext();
        var errors = new[]
        {
            new ValidationError("FirstName", "'First Name' must not be empty."),
            new ValidationError("Password", "'Password' is not in the correct format.")
        };
        var exception = new ValidationException(errors);

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        var body = await ReadResponseBodyAsync(context);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        body.RootElement.GetProperty("title").GetString().Should().Be("Validation Error");
        body.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_ResponseBodyContainsFieldErrors()
    {
        var context = CreateHttpContext();
        var errors = new[] { new ValidationError("Username", "Username is required.") };
        var exception = new ValidationException(errors);

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        var body = await ReadResponseBodyAsync(context);

        var errorsArray = body.RootElement.GetProperty("errors").EnumerateArray().ToList();
        errorsArray.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_Returns500()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong internally.");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        var body = await ReadResponseBodyAsync(context);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.RootElement.GetProperty("title").GetString().Should().Be("Server error");
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_DoesNotExposeStackTrace()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong internally.");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        var body = await ReadResponseBodyAsync(context);

        var responseJson = body.RootElement.GetRawText();
        responseJson.Should().NotContain("StackTrace");
        responseJson.Should().NotContain("at MyProject");
        responseJson.Should().NotContain("Something went wrong internally.");
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();

        var result1 = await _handler.TryHandleAsync(context1, new ValidationException([]), CancellationToken.None);
        var result2 = await _handler.TryHandleAsync(context2, new Exception("unhandled"), CancellationToken.None);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }
}
