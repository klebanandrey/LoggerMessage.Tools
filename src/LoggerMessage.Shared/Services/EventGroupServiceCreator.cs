namespace LoggerMessage.Shared.Services
{
    public static class EventGroupServiceCreator
    {
        public static IEventGroupService Create(string solutionPath)
        {
            var helper = new ConfigurationHelper(solutionPath);
            var settings = helper.GetConfiguration();

            if (!bool.Parse(settings[Constants.IsShared].ToString()))
                return new LocalEventGroupService(helper.GetConfigFolder());

            return new RemoteEventGroupService(settings);
        }
    }
}
