using System;
using EventGroups.Roslyn;
using LoggerMessageExtension.Scopes;
using LoggerMessageExtension.Services;
using Microsoft.CodeAnalysis;

namespace LoggerMessageExtension
{
    public static class EventGroupServiceCreator
    {
        public static IEventGroupService Create(Workspace workspace)
        {
            try
            {
                var configuration = workspace.GetConfiguration();

                if (!bool.Parse(configuration[EventGroups.Common.Constants.IsShared].ToString()))
                {
                    return new LocalEventGroupService(workspace.GetConfigFolder());
                }

                return new RemoteEventGroupService(configuration);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
