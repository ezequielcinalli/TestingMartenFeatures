using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace TestingMartenFeatures.Api.Tests;

public class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("postgres:12.16")
        .WithPortBinding(5432, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .WithEnvironment("POSTGRES_DB", ContainerDatabase)
        .WithEnvironment("POSTGRES_USER", ContainerUsername)
        .WithEnvironment("POSTGRES_PASSWORD", ContainerPassword)
        .Build();

    private IServiceScope? _scope;
    private static string ContainerDatabase => "TestingMartenFeatures";
    private static string ContainerUsername => "postgres";
    private static string ContainerPassword => "postgres";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder();
            connectionStringBuilder.Host = _container.Hostname;
            connectionStringBuilder.Port = _container.GetMappedPublicPort(5432);
            connectionStringBuilder.Database = ContainerDatabase;
            connectionStringBuilder.Username = ContainerUsername;
            connectionStringBuilder.Password = ContainerPassword;
            connectionStringBuilder.CommandTimeout = 3;

            services.RemoveAll(typeof(IConfigureOptions<MartenOptions>));
            services.PostConfigure<MartenOptions>(config =>
            {
                config.ConnectionString = connectionStringBuilder.ToString();
            });
        });
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, bool forceNewScope = false)
    {
        if (_scope is null || forceNewScope) _scope = Services.CreateScope();

        var mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(request);
    }
}