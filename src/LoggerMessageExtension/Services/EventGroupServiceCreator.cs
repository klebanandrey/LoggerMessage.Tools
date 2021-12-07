﻿using System;
using EventGroups.Roslyn;
using LoggerMessages.Roslyn;
using Microsoft.CodeAnalysis;
using Constants = LoggerMessages.Common.Constants;

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
