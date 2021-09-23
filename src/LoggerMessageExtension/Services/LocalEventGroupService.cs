using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoggerMessageExtension.Services;
using Newtonsoft.Json;
using Constants = EventGroups.Common.Constants;

namespace LoggerMessageExtension.Scopes
{
    public class LocalEventGroupService : IEventGroupService
    {
        public bool Connected => true;
        public Microsoft.CodeAnalysis.Solution Solution { get; set; }

        private List<EventGroupLocal> _groups;
        private readonly string filePath;
        private JsonSerializerSettings _settings = new JsonSerializerSettings() {Formatting = Formatting.Indented};

        public LocalEventGroupService(string configFolder)
        {
            filePath = Path.Combine(configFolder, Constants.GroupsFile);
            if (File.Exists(filePath))
                _groups = JsonConvert.DeserializeObject<List<EventGroupLocal>>(File.ReadAllText(filePath), _settings);
            else
                _groups = new List<EventGroupLocal>();
        }

        public Task<bool> IsAbbrExistAsync(string abbr)
        {
            return Task.FromResult(_groups.Any(e => e.EventGroupAbbr == abbr));
        }

        public Task<IEnumerable<EventGroupViewObject>> GetEventGroupsAsync()
        {
            return Task.FromResult(_groups
                    .OrderBy(e => e.EventGroupAbbr)
                    .Select(e => new EventGroupViewObject()
                    {
                        Abbreviation = e.EventGroupAbbr,
                        Description = e.Description
                    }).AsEnumerable());
        }

        public async Task<bool> TryAddEventGroupAsync(EventGroupViewObject newEventGroupViewObject)
        {
            if (await IsAbbrExistAsync(newEventGroupViewObject.Abbreviation))
                return true;

            _groups.Add(new EventGroupLocal()
            {
                EventGroupAbbr = newEventGroupViewObject.Abbreviation,
                Description = newEventGroupViewObject.Description
            });

            File.WriteAllText(filePath, JsonConvert.SerializeObject(_groups, _settings));

            return true;
        }
    }
}
