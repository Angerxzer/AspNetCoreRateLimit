﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitMiddleware : RateLimitMiddleware<ClientRateLimitProcessor>
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClientRateLimitMiddleware> _logger;

        public ClientRateLimitMiddleware(RequestDelegate next,
            IOptions<ClientRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IClientPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<ClientRateLimitMiddleware> logger)
        : base(options?.Value, new ClientRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)
        {
            _next = next;
            _logger = logger;
        }

        public virtual Task Invoke(HttpContext context)
        {
            return base.ThrottleAsync(context, () => _next.Invoke(context));
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger?.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }
    }
}