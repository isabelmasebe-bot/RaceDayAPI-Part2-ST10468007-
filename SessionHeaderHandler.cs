using System.Net.Http.Headers;

namespace RaceDayMVC.Services
{
    public class SessionHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null)
            {
                var userId = context.Session.GetInt32("UserId");
                var role = context.Session.GetString("Role");

                if (userId.HasValue)
                    request.Headers.Add("X-User-Id", userId.Value.ToString());

                if (!string.IsNullOrEmpty(role))
                    request.Headers.Add("X-User-Role", role);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}