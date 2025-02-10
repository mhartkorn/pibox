using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.DependencyInjection;
using PiBox.Hosting.Abstractions;
using PiBox.Hosting.Abstractions.Attributes;
using Unleash;
using TimeProvider = System.TimeProvider;

namespace PiBox.Plugins.Management.Unleash
{
    [Middleware(50)]
    public class UnleashMiddlware(
        RequestDelegate next,
        TimeProvider timeProvider,
        UnleashConfiguration unleashConfiguration,
        IServiceProvider serviceProvider)
        : ApiMiddleware(next)
    {
        private readonly bool _sessionFeatureEnabled = serviceProvider.GetService<ISessionStore>() != null;

        internal const string Unleashcontext = "unleashContext";

        public override async Task Invoke(HttpContext context)
        {
            var unleashContext = new UnleashContext
            {
                UserId = context.User?.Identity?.Name,
                AppName = unleashConfiguration.AppName,
                Environment = unleashConfiguration.Environment,
                CurrentTime = timeProvider.GetUtcNow(),
                RemoteAddress = context.Request?.Host.Host,
                Properties = new Dictionary<string, string>()
            };
            if (_sessionFeatureEnabled)
            {
                unleashContext.SessionId = context.Session?.Id;
            }

            context.Items[Unleashcontext] = unleashContext;
            await Next.Invoke(context);
        }
    }
}
