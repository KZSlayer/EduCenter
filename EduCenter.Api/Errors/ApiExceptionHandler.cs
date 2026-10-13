using System.Diagnostics;
using EduCenter.Business.Pricing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EduCenter.Api.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request {RequestMethod} {RequestPath} was canceled. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
            return true;
        }

        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "An exception occurred after the response started for {RequestMethod} {RequestPath}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
            return false;
        }

        var (problemDetails, isUnexpected) = CreateProblemDetails(httpContext, exception);
        problemDetails.Extensions["traceId"] = traceId;

        if (isUnexpected)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {RequestMethod} {RequestPath}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static (ProblemDetails ProblemDetails, bool IsUnexpected) CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        var instance = httpContext.Request.Path.ToString();

        return exception switch
        {
            InvalidCartPriceCalculateRequestException invalidRequest =>
                (new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [invalidRequest.Field] = [invalidRequest.Message]
                })
                {
                    Type = "about:blank",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = instance
                }, false),
            PricingUserNotFoundException =>
                (new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "The authenticated user has no pricing profile.",
                    Instance = instance
                }, false),
            UnknownCourseIdsException unknownCourseIds =>
                (CreateUnknownCoursesProblemDetails(instance, unknownCourseIds), false),
            _ =>
                (new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "The server encountered an unexpected error. Use the traceId to contact support.",
                    Instance = instance
                }, true)
        };
    }

    private static ProblemDetails CreateUnknownCoursesProblemDetails(
        string instance,
        UnknownCourseIdsException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "about:blank",
            Title = "Unprocessable Content",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "The cart contains course IDs that do not exist. No price was calculated.",
            Instance = instance
        };
        problemDetails.Extensions["unknownCourseIds"] = exception.CourseIds;
        return problemDetails;
    }
}
