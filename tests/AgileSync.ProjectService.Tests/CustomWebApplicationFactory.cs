using AgileSync.ProjectService.Models;
using AgileSync.Shared.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace AgileSync.ProjectService.Tests;

/// <summary>
/// Custom web application factory for ProjectService integration tests.
/// Replaces real MongoDB repositories with NSubstitute mocks.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IRepository<Project> ProjectRepository { get; } = Substitute.For<IRepository<Project>>();
    public IRepository<Board> BoardRepository { get; } = Substitute.For<IRepository<Board>>();
    public IRepository<WorkItem> WorkItemRepository { get; } = Substitute.For<IRepository<WorkItem>>();
    public IRepository<Sprint> SprintRepository { get; } = Substitute.For<IRepository<Sprint>>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("MongoDb:ConnectionString", "mongodb://localhost:27017");
        builder.UseSetting("MongoDb:DatabaseName", "agilesync-test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRepository<Project>>();
            services.RemoveAll<IRepository<Board>>();
            services.RemoveAll<IRepository<WorkItem>>();
            services.RemoveAll<IRepository<Sprint>>();

            services.AddSingleton(ProjectRepository);
            services.AddSingleton(BoardRepository);
            services.AddSingleton(WorkItemRepository);
            services.AddSingleton(SprintRepository);
        });
    }
}
