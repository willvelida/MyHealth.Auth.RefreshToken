using Microsoft.Extensions.Configuration;
using MyHealth.Auth.RefreshToken.Models;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyHealth.Auth.RefreshToken.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly HttpClient _httpClient;

        public RefreshTokenService(
            IConfiguration configuration,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _keyVaultHelper = keyVaultHelper;
            _httpClient = httpClient;
        }

        public async Task<RefreshTokenResponse> RefreshToken()
        {
            try
            {
                var fitbitRefreshTokenSecret = await _keyVaultHelper.RetrieveSecretFromKeyVaultAsync(_configuration["RefreshTokenName"]);
                var fitbitClientCredentials = await _keyVaultHelper.RetrieveSecretFromKeyVaultAsync(_configuration["FitbitCredentials"]);

                _httpClient.DefaultRequestHeaders.Clear();
                UriBuilder uri = new UriBuilder("https://api.fitbit.com/oauth2/token");
                uri.Query = $"grant_type=refresh_token&refresh_token={fitbitRefreshTokenSecret.Value}";
                var request = new HttpRequestMessage(HttpMethod.Post, uri.Uri);
                request.Content = new StringContent("");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", fitbitClientCredentials.Value);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(content);

                return refreshTokenResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
