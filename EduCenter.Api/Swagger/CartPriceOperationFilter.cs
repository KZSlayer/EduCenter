using EduCenter.Api.DTOs;
using EduCenter.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EduCenter.Api.Swagger;

public sealed class CartPriceOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(CartPriceController) ||
            context.MethodInfo.Name != nameof(CartPriceController.Calculate))
        {
            return;
        }

        operation.Description = $"The request body is limited to {CartPriceRequestLimits.MaximumRequestBodySizeBytes} bytes. CourseIds must contain 1 to {CartPriceRequestLimits.MaximumCourseCount} unique, non-blank IDs, each no longer than {CartPriceRequestLimits.MaximumCourseIdLength} characters. Duplicate or invalid values are rejected with HTTP 400. Unknown course IDs are rejected with HTTP 422, and no partial price is returned.";

        var requestSchema = operation.RequestBody?.Content.Values.FirstOrDefault()?.Schema;
        if (requestSchema?.Reference?.Id is { } schemaId &&
            context.SchemaRepository.Schemas.TryGetValue(schemaId, out var referencedSchema))
        {
            requestSchema = referencedSchema;
        }

        if (requestSchema?.Properties.TryGetValue("courseIds", out var courseIdsSchema) == true)
        {
            courseIdsSchema.MinItems = 1;
            courseIdsSchema.MaxItems = CartPriceRequestLimits.MaximumCourseCount;
            courseIdsSchema.UniqueItems = true;
            courseIdsSchema.Description = "Unique, non-blank course IDs. Unknown IDs result in HTTP 422.";

            if (courseIdsSchema.Items is { } courseIdSchema)
            {
                courseIdSchema.MinLength = 1;
                courseIdSchema.MaxLength = CartPriceRequestLimits.MaximumCourseIdLength;
                courseIdSchema.Description = "A non-blank course ID.";
            }
        }

        if (operation.Responses.TryGetValue("400", out var badRequestResponse))
        {
            badRequestResponse.Description = "The request is invalid or contains duplicate course IDs.";
        }

        if (operation.Responses.TryGetValue("401", out var unauthorizedResponse))
        {
            unauthorizedResponse.Description = "The token is invalid or the authenticated user has no pricing profile. The response uses Problem Details.";
        }

        if (operation.Responses.TryGetValue("422", out var unprocessableResponse))
        {
            unprocessableResponse.Description = "One or more course IDs were not found. No cart price was calculated. The Problem Details response includes the unknownCourseIds extension.";
        }

        if (operation.Responses.TryGetValue("413", out var payloadTooLargeResponse))
        {
            payloadTooLargeResponse.Description = "The request body exceeds the configured size limit. The response uses Problem Details.";
        }

        if (operation.Responses.TryGetValue("500", out var internalServerErrorResponse))
        {
            internalServerErrorResponse.Description = "An unexpected server error occurred. The response includes a traceId for support.";
        }
    }
}
