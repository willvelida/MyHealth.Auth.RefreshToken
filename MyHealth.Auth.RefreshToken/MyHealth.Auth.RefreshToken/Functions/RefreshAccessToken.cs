using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Auth.RefreshToken.Models;
using MyHealth.Auth.RefreshToken.Services;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.Auth.RefreshToken.Functions
{
    public class RefreshAccessToken
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IKeyVaultService _keyVaultService;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public RefreshAccessToken(
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IKeyVaultService keyVaultService,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
            _keyVaultService = keyVaultService;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(RefreshAccessToken))]
        public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

                log.LogInformation("Refreshing Fitbit Refresh and Access Token.");
                RefreshTokenResponse refreshTokenResponse = await _refreshTokenService.RefreshToken();

                log.LogInformation("Refresh and Access Token refreshed. Saving to Key Vault");
                await _keyVaultService.SaveTokensToKeyVault(refreshTokenResponse);
                log.LogInformation("Tokens saved to Key Vault.");
            }
            catch (Exception ex)
            {
                log.LogError($"Exception thrown in {nameof(RefreshAccessToken)}: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                throw ex;
            }
        }
    }
}
