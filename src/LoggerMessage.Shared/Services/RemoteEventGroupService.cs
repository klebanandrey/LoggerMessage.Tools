using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EventGroups.Contract;
using LoggerMessageExtension.EventGroupsClientService;

namespace LoggerMessage.Shared.Services
{
    public class RemoteEventGroupService : IEventGroupService
    {
        private EventGroupsClient _client;
        private Guid _userId;
        private bool _connected;

        public RemoteEventGroupService(Dictionary<string, object> configuration)
        {
            try
            {
                var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration[Constants.ApiKey].ToString());

                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(configuration[Constants.ApiKey].ToString());
                _userId = Guid.Parse(jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
                _client = new EventGroupsClient(configuration[Constants.ServiceUrl].ToString(), httpClient);
                _connected = true;
            }
            catch (Exception)
            {
                _connected = false;
            }            
        }

        //public Solution Solution {get; set;}

        public bool Connected => _connected;

        public async Task<IEnumerable<IEventGroup>> GetEventGroupsAsync()
        {
            if (!Connected)
                return null;

            //TODO : var groups = await _client.ApiEventgroupsSolutiongroupsAsync(Solution.Id.Id).ConfigureAwait(false);
            var groups = await _client.ApiEventgroupsSolutiongroupsAsync(Guid.Empty).ConfigureAwait(false);

            return await Task.FromResult(groups.Select(g => new EventGroupViewObject()
            {
                Abbreviation = g.EventGroupAbbr,
                Description = g.Description
            }));
        }

        public async Task<bool> IsAbbrExistAsync(string abbr)
        {
            if (!Connected)
                return false;

            //TODO :var result = await _client.ApiEventgroupsFindAsync(Solution.Id.Id, abbr);
            var result = await _client.ApiEventgroupsFindAsync(Guid.Empty, abbr);

            if (result == null)
                return false;
            return true;            
        }

        public async Task<bool> TryAddEventGroupAsync(EventGroupViewObject newEventGroupViewObject)
        {
            if (!Connected)
                return false;

            if (await _client.ApiEventgroupsPostAsync(new EventGroupDTO()
            {
                Description = newEventGroupViewObject.Description,
                EventGroupAbbr = newEventGroupViewObject.Abbreviation,
                //TODO : SolutionId = Solution.Id.Id,
                //TODO : SolutionName = Solution.GetSolutionName(),
                SolutionId = Guid.Empty,
                SolutionName = "",

                Owner = _userId
            }) != null)            
                return true;

            return false;
        }

        public async Task<IEventGroup> GetEventGroupAsync(string abbr)
        {
            if (!Connected)
                return null;

            //TODO :var result = await _client.ApiEventgroupsFindAsync(Solution.Id.Id, abbr);
            return await _client.ApiEventgroupsFindAsync(Guid.Empty, abbr) as IEventGroup;
        }
    }
}
