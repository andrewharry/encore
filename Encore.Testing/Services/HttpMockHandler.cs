using System.Net;

namespace Encore.Testing.Services
{
    public class HttpMockHandler : HttpMessageHandler
    {
        public HttpResponseMessage? Response { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Response ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
