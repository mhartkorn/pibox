using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using PiBox.Hosting.Abstractions.Middlewares.Models;

namespace PiBox.Hosting.Abstractions
{
    /// <summary>
    /// Abstract skeleton for asp net core middlewares
    /// No testing needed.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ApiMiddleware(RequestDelegate next)
    {
        protected RequestDelegate Next { get; } = next;

        public abstract Task Invoke(HttpContext context);

        protected static Task WriteResponse<T>(HttpContext context, int statusCode, T result)
        {
            if (context.Response.HasStarted) return Task.CompletedTask;
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(result);
        }

        protected Task WriteDefaultResponse(HttpContext context, int statusCode, DateTime dateTime, string message = null) =>
            WriteResponse(context, statusCode, new ErrorResponse(dateTime,
                message ?? ReasonPhrases.GetReasonPhrase(statusCode), context.TraceIdentifier));
    }
}
