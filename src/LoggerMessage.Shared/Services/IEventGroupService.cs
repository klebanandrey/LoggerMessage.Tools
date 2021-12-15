using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoggerMessage.Shared.Services
{
    public interface IEventGroupService
    {
        bool Connected { get;}

        //Solution Solution { get; set; }

        Task<bool> IsAbbrExistAsync(string abbr);

        Task<IEnumerable<IEventGroup>> GetEventGroupsAsync();

        Task<bool> TryAddEventGroupAsync(EventGroupViewObject newEventGroupViewObject);
    }
}
