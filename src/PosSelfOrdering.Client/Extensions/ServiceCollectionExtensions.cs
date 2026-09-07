using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PosSelfOrdering.Client.Api;
using PosSelfOrdering.Client.Mocks;
using PosSelfOrdering.Client.Repositories.Contracts;
using PosSelfOrdering.Client.Repositories.Implementations;
using PosSelfOrdering.Client.Services;
using PosSelfOrdering.Client.State;
using PosSelfOrdering.Client.Store;

namespace PosSelfOrdering.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPosServices(this IServiceCollection services, IConfiguration? config = null)
    {
        var useMockConfig = config?["ApiSettings:UseMock"];
        var useMock = string.IsNullOrEmpty(useMockConfig) || (bool.TryParse(useMockConfig, out var isMock) && isMock);

        // Core Shared In-Memory Store (Singleton across components)
        services.AddSingleton<SharedPosDataStore>();
        services.AddScoped<BroadcastChannelService>();

        if (useMock)
        {
            // Register In-Memory Mock Repositories
            services.AddSingleton<ISessionRepository, MockSessionRepository>();
            services.AddSingleton<IMenuRepository, MockMenuRepository>();
            services.AddSingleton<IOrderRepository, MockOrderRepository>();
            services.AddSingleton<IPaymentRepository, MockPaymentRepository>();
            services.AddSingleton<IAdminRepository, MockAdminRepository>();
        }
        else
        {
            // Register Real API Client & Repositories
            var baseUrl = config?["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
            services.AddScoped<IApiClient>(sp =>
            {
                var http = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(15)
                };
                return new ApiClient(http);
            });

            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IAdminRepository, RealAdminRepository>();
        }

        // State Containers
        services.AddScoped<AppState>();
        services.AddScoped<SessionState>();
        services.AddScoped<CartState>();
        services.AddScoped<AdminState>();

        // Domain Services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
