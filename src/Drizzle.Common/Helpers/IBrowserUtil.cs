using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Helpers
{
    public interface IBrowserUtil
    {
        public Task<bool> OpenBrowserAsync(Uri uri);
        public Task<bool> OpenBrowserAsync(string url);
    }
}
