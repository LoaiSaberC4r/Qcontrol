using BuildingBlock.Application.Abstraction.Caching;
using BuildingBlock.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Qcontrol.Application.Features.Auth.Shared;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Security;
using QControl.Application.Features.TicketRuntime;

namespace QControl.Application.Bootstrap
{
    public static class Bootstrap
    {
        //Mediator Injection
        private static IServiceCollection AddMediatorInjection(this IServiceCollection services)
        {
            services.AddSingleton<KeyedSemaphore>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(TracingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ExceptionMappingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(QueryCacheBehavior<,>));
                cfg.AddOpenBehavior(typeof(CommandCacheInvalidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(ResilienceBehavior<,>));
                cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });
            return services;
        }

        private static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
            services.AddValidatorsFromAssembly(
                   AssemblyReference.Assembly,
                   includeInternalTypes: true);
            return services;
        }

        public static IServiceCollection AddApplicationBootstrap(this IServiceCollection services)
        {
            services.AddFluentValidation();
            services.AddScoped<ICurrentBranchContext, CurrentBranchContext>();
            services.AddScoped<IBranchAccessValidator, BranchAccessValidator>();
            services.AddScoped<IServiceDefinitionAccessValidator, ServiceDefinitionAccessValidator>();
            services.AddScoped<IServiceVisibilityPolicy, ServiceVisibilityPolicy>();
            services.AddScoped<ISegmentVisibilityPolicy, SegmentVisibilityPolicy>();
            services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
            services.AddScoped<TicketRuntimeRequestGuard>();
            services.AddScoped<UserTokenFactory>();
            services.AddMediatorInjection();
            return services;
        }
    }
}
