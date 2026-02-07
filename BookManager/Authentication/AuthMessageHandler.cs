using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace BookManager.Authentication
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly ITokenStore _tokenStore;

        public AuthMessageHandler(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenStore.GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
