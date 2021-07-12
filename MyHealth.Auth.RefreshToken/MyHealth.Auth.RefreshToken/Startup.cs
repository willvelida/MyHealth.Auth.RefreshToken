using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyHealth.Auth.RefreshToken;
using MyHealth.Auth.RefreshToken.Services;
using MyHealth.Common;
using Polly;
using Polly.Extensions.Http;
using System;
using System.IO;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyHealth.Auth.RefreshToken
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IServiceBusHelpers>(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                return new ServiceBusHelpers(configuration["ServiceBusConnectionString"]);
            });
            builder.Services.AddSingleton<IKeyVaultHelper>(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                return new KeyVaultHelper(configuration["KeyVaultName"]);
            });
            builder.Services.AddHttpClient<IRefreshTokenService, RefreshTokenService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(15))
                .AddPolicyHandler(GetRetryPolicy());
            builder.Services.AddScoped<IKeyVaultService, KeyVaultService>();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                            retryAttempt)));
        }
    }
}
