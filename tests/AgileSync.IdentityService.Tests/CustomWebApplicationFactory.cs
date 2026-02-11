using AgileSync.IdentityService.Models;
using AgileSync.Shared.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace AgileSync.IdentityService.Tests;

/// <summary>
/// Custom web application factory for IdentityService integration tests.
/// Replaces real MongoDB repositories with NSubstitute mocks and provides
/// a test SuperAdmin configuration.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IRepository<User> UserRepository { get; } = Substitute.For<IRepository<User>>();
    public IRepository<Organization> OrgRepository { get; } = Substitute.For<IRepository<Organization>>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real repository registrations
            services.RemoveAll<IRepository<User>>();
            services.RemoveAll<IRepository<Organization>>();

            // Inject mocks
            services.AddSingleton(UserRepository);
            services.AddSingleton(OrgRepository);
        });

        builder.UseSetting("MongoDb:ConnectionString", "mongodb://localhost:27017");
        builder.UseSetting("MongoDb:DatabaseName", "agilesync-test");
        builder.UseSetting("SuperAdmin:Username", "testadmin");
        builder.UseSetting("SuperAdmin:Password", "testpass");
    }
}
