using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MechanicShop.Api.OpenApi;

public class BearerSecurityOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metaData = context.Description.ActionDescriptor.EndpointMetadata;

        var hasAuthorizeAttribute = metaData.OfType<AuthorizeAttribute>().Any();
        var hasAllowAnonymousAttribute = metaData.OfType<AllowAnonymousAttribute>().Any();

        if (!hasAuthorizeAttribute || hasAllowAnonymousAttribute)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        JwtBearerDefaults.AuthenticationScheme,
                        context.Document)
                ] = [],
            });

        return Task.CompletedTask;
    }
}
