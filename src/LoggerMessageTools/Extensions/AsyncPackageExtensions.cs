using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerMessageTools.Extensions
{
    internal static class AsyncPackageExtensions
    {
        internal static async Task<TService> GetPackageServiceAsync<TService>(this AsyncPackage package)
        {
            return (TService)await package.GetServiceAsync(typeof(TService));
        }
    }
}
