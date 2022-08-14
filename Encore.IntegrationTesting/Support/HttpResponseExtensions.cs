using System.Net.Http;
using System.Threading.Tasks;

namespace Encore.IntegrationTesting
{
    public static class HttpResponseExtensions
    {
        public static Task<string> GetBody(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync();
        }
    }
}
