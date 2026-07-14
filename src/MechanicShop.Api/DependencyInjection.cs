using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using MechanicShop.Api.Infrastructure;
using MechanicShop.Api.OpenApi;
using MechanicShop.Api.Services;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace MechanicShop.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddCustomProblemDetails()
            .AddCustomApiVersioning()
            .AddApiDocumentation()
            .AddExceptionHandling()
            .AddControllerWithJsonConfig()
            .AddConfiguredCors(configuration)
            .AddIdentityInfrastructure()
            .AddAppRateLimiting()
            .AddOutputCaching()
            .AddAppOpenTelemetry()
            .AddCompression()
            .AddSignalR();

        return services;
    }

    private static IServiceCollection AddCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options => options.EnableForHttps = true);

        return services;
    }

    private static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = (context) =>
            {
                context.ProblemDetails.Extensions.Add(
                    "requestId",
                    context.HttpContext.TraceIdentifier
                );
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            }
        );
        return services;
    }

    private static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        string[] versions = ["v1"];

        foreach (var version in versions)
        {
            services.AddOpenApi(
                version,
                options =>
                {
                    options.AddDocumentTransformer<VersionInfoTransformer>();
                    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                    options.AddOperationTransformer<BearerSecurityOperationTransformer>();
                }
            );
        }

        return services;
    }

    private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    private static IServiceCollection AddControllerWithJsonConfig(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull
            );
        return services;
    }

    private static IServiceCollection AddConfiguredCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var allowedOrigins =
            configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy(
                "DefaultCorsPolicy",
                policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            )
        );

        return services;
    }

    private static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }

    private static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddSlidingWindowLimiter(
                "SlidingWindow",
                options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.SegmentsPerWindow = 6;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 10;
                    options.AutoReplenishment = true;
                }
            );
        });
        return services;
    }

    private static IServiceCollection AddOutputCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100 mb
            options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(60)));
        });

        return services;
    }

    private static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(res => res.AddService("overservice"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();

                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();

                metrics.AddOtlpExporter().AddPrometheusExporter(); // /metrics
            });

        return services;
    }

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseCoreMiddlewares()
        {
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseRequestLogContext();
            app.UseSerilogRequestLogging();
            app.UseCors("DefaultCorsPolicy");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOutputCache();

            return app;
        }
    }
}
