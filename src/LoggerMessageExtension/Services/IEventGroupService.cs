using System.Collections.Generic;
using System.Threading.Tasks;
using LoggerMessages.Common;
using Microsoft.CodeAnalysis;

namespace LoggerMessageExtension.Services
{
    public interface IEventGroupService
    {
        bool Connected { get;}

        Solution Solution { get; set; }

        Task<bool> IsAbbrExistAsync(string abbr);

        Task<IEnumerable<IEventGroup>> GetEventGroupsAsync();

        Task<bool> TryAddEventGroupAsync(EventGroupViewObject newEventGroupViewObject);
    }
}
