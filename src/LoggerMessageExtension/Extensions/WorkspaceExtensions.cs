using System;
using LoggerMessageExtension.Scopes;
using EventGroups.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace LoggerMessageExtension.Extensions
{
    public static class WorkspaceExtensions
    {
        //public static IServiceProvider CreateServiceProvider(this Workspace workspace)
        //{
        //    var sc = new ServiceCollection();
        //    var conf = workspace.GetConfiguration();

        //    if (!Boolean.Parse(conf[LoggerMessages.Common.Constants.IsShared].ToString()))
        //    {
        //        sc.AddSingleton<IEventGroupService, LocalEventGroupService>();
        //        sc.AddDbContext<EventGroupDbContext>(builder => builder.UseSqlite($"Data Source={workspace.GetConfigFolder()}\\EventGroups.db"));
        //    }

        //    var sp = sc.BuildServiceProvider();

        //    //using (var scope = sp.CreateScope())
        //    //{
        //    //    var db = scope.ServiceProvider.GetRequiredService<EventGroupDbContext>();
        //    //    db.Database.Migrate();
        //    //}

        //    return sp;
        //}

    }
}
