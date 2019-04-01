using Microsoft.AspNetCore.Http;

namespace PolloPollo.Web.Helpers
{
    public class HttpContextHelper
    {
        public static string GetBaseUrl(string scheme, HostString host)
        {
            return $"{scheme}://{host}";
        }
    }
}
