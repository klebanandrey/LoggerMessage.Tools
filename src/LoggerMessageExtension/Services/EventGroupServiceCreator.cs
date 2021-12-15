using System;
using LoggerMessages.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Constants = LoggerMessage.Shared.Constants;


namespace LoggerMessageExtension.Services
{
    public static class EventGroupServiceCreator
    {
        public static IEventGroupService Create(Workspace workspace)
        {
            try
            {
                var configuration = workspace.GetConfiguration();

                if (!bool.Parse(configuration[Constants.IsShared].ToString()))
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
