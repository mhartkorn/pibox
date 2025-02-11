using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PiBox.Hosting.Abstractions.Attributes;
using PiBox.Hosting.Abstractions.Exceptions;
using PiBox.Hosting.Abstractions.Middlewares.Models;

namespace PiBox.Hosting.Abstractions.Middlewares
{
    [Middleware(int.MinValue)]
    public sealed class ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        GlobalStatusCodeOptions globalStatusCodeOptions,
        TimeProvider timeProvider)
        : ApiMiddleware(next)
    {
        private readonly ILogger _logger = logger;

        public override async Task Invoke(HttpContext context)
        {
            try
            {
                await Next.Invoke(context).ConfigureAwait(false);
                if (globalStatusCodeOptions.DefaultStatusCodes.Contains(context.Response.StatusCode))
                    await WriteDefaultResponse(context, context.Response.StatusCode, timeProvider.GetUtcNow().DateTime);
            }
            catch (ValidationPiBoxException validationPiBoxException)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, validationPiBoxException.Message);
                _logger.LogInformation(validationPiBoxException, "Validation PiBox Exception occured");
                var validationErrorResponse = new ValidationErrorResponse(timeProvider.GetUtcNow().DateTime,
                    validationPiBoxException.Message, context.TraceIdentifier,
                    validationPiBoxException.ValidationErrors);
                await WriteResponse(context, validationPiBoxException.HttpStatus, validationErrorResponse);
            }
            catch (PiBoxException piBoxException)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, piBoxException.Message);
                _logger.LogInformation(piBoxException, "PiBox Exception occured");
                await WriteDefaultResponse(context, piBoxException.HttpStatus, timeProvider.GetUtcNow().DateTime,
                    piBoxException.Message);
            }
            catch (Exception e)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, e.Message);
                _logger.LogError(e, "Unhandled Exception occured");
                await WriteDefaultResponse(context, StatusCodes.Status500InternalServerError,
                    timeProvider.GetUtcNow().DateTime);
            }
        }
    }
}
