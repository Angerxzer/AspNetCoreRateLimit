﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitMiddleware : RateLimitMiddleware<IpRateLimitProcessor>
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpRateLimitMiddleware> _logger;

        public IpRateLimitMiddleware(
            RequestDelegate next, 
            IOptions<IpRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IIpPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<IpRateLimitMiddleware> logger)
        : base(options?.Value, new IpRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)

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
            _logger?.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }
    }
}