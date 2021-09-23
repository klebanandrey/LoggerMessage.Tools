using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggerMessageExtension.Scopes;
using Microsoft.CodeAnalysis;

namespace LoggerMessageExtension
{
    public interface IEventGroupService
    {
        bool Connected { get;}

        Solution Solution { get; set; }

        Task<bool> IsAbbrExistAsync(string abbr);

        Task<IEnumerable<EventGroupViewObject>> GetEventGroupsAsync();

        Task<bool> TryAddEventGroupAsync(EventGroupViewObject newEventGroupViewObject);
    }
}
