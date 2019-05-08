using Microsoft.AspNetCore.Http;
using System.Net;

namespace PolloPollo.Web.Security
{
    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return (connection.LocalIpAddress.Equals(IPAddress.Parse("127.0.0.1")) && (connection.RemoteIpAddress.Equals(connection.LocalIpAddress)) 
                        || req.Host.Value.StartsWith("localhost:"))
                        && (connection.LocalPort == 4001 || connection.LocalPort == 4000);
                }
                else
                {
                    return IPAddress.IsLoopback(connection.RemoteIpAddress)
                        && (connection.LocalPort == 4001 || connection.LocalPort == 4000);
                }
            }
            // for in memory TestServer or when dealing with default connection info
            else if (connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
}
