using Microsoft.Extensions.Configuration;
using MyHealth.Auth.RefreshToken.Models;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.Auth.RefreshToken.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultHelper _keyVaultHelper;

        public KeyVaultService(
            IConfiguration configuration,
            IKeyVaultHelper keyVaultHelper)
        {
            _configuration = configuration;
            _keyVaultHelper = keyVaultHelper;
        }

        public async Task SaveTokensToKeyVault(RefreshTokenResponse refreshedTokens)
        {
            try
            {
                await _keyVaultHelper.SaveSecretToKeyVaultAsync(_configuration["RefreshTokenName"], refreshedTokens.RefreshToken);
                await _keyVaultHelper.SaveSecretToKeyVaultAsync(_configuration["AccessTokenName"], refreshedTokens.AccessToken);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
