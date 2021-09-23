using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggerMessageExtension.Extensions;
using LoggerMessageExtension.EventGroupsClientService;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using EventGroups.Contract;
using LoggerMessages.Common;

namespace LoggerMessageExtension.Services
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

        public Solution Solution {get; set;}

        public bool Connected => _connected;

        public async Task<IEnumerable<IEventGroup>> GetEventGroupsAsync()
        {
            if (!Connected)
                return null;

            var groups = await _client.ApiEventgroupsSolutiongroupsAsync(Solution.Id.Id).ConfigureAwait(false);

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

            var result = await _client.ApiEventgroupsFindAsync(Solution.Id.Id, abbr);

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
                SolutionId = Solution.Id.Id,
                SolutionName = Solution.GetSolutionName(),
                Owner = _userId
            }) != null)            
                return true;

            return false;
        }
    }
}
