using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Infrastructure.Persistence.Interceptors;

namespace Todo_App.Infrastructure.Persistence.Factories;
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        
        var serviceProvider = new ServiceCollection()
            .Configure<OperationalStoreOptions>(options => { }) 
            .AddSingleton<IMediator>(sp => null!)
            .AddSingleton<AuditableEntitySaveChangesInterceptor>()
            .BuildServiceProvider();

        var operationalStoreOptions = serviceProvider.GetRequiredService<IOptions<OperationalStoreOptions>>();
        var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
        var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();

        return new ApplicationDbContext(optionsBuilder.Options, operationalStoreOptions, null!, interceptor);
    }
}
