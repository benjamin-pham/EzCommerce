using System.Reflection;
using MyProject.API.Endpoints;
using MyProject.API.Extensions;
using MyProject.Application;
using MyProject.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.AddSerilogLogging();

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddOpenApiServices();
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthenticationSchemes(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

// ── Pipeline ─────────────────────────────────────────────────────────────────
var app = builder.Build();
app.MapOpenApiEndpoints();
app.UseExceptionHandler(o => { });
app.UseCorrelationId();
app.UseSerilogLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();
app.MapHealthChecks("/health");

app.MapFallback(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Resource Not Found");
});
app.Run();

// Expose Program for integration tests
public partial class Program { }
