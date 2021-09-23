using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventGroups.App.Services
{
    public interface IApiKeyService
    {
        Task<string> GenerateApiKey(string username);
    }
}
