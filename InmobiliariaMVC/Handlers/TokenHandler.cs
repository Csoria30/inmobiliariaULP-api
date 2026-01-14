using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace InmobiliariaMVC.Handlers
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
