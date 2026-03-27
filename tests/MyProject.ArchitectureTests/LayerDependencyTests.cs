namespace MyProject.ArchitectureTests;

public class LayerDependencyTests
{
    private const string DomainNamespace         = "MyProject.Domain";
    private const string ApplicationNamespace    = "MyProject.Application";
    private const string InfrastructureNamespace = "MyProject.Infrastructure";
    private const string ApiNamespace            = "MyProject.API";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Application()
    {
        Types.InAssembly(typeof(MyProject.Domain.Abstractions.IRepository<>).Assembly)
             .Should().NotHaveDependencyOn(ApplicationNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Infrastructure()
    {
        Types.InAssembly(typeof(MyProject.Domain.Abstractions.IRepository<>).Assembly)
             .Should().NotHaveDependencyOn(InfrastructureNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Infrastructure()
    {
        Types.InAssembly(typeof(MyProject.Application.DependencyInjection).Assembly)
             .Should().NotHaveDependencyOn(InfrastructureNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Api()
    {
        Types.InAssembly(typeof(MyProject.Application.DependencyInjection).Assembly)
             .Should().NotHaveDependencyOn(ApiNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }
}
