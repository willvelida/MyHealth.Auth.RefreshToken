using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.Auth.RefreshToken.Models;
using MyHealth.Auth.RefreshToken.Services;
using MyHealth.Common;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyHealth.Auth.RefreshToken.UnitTests.ServiceTests
{
    public class KeyVaultServiceShould
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IKeyVaultHelper> _mockKeyVaultHelper;

        private KeyVaultService _sut;

        public KeyVaultServiceShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockKeyVaultHelper = new Mock<IKeyVaultHelper>();

            _sut = new KeyVaultService(
                _mockConfiguration.Object,
                _mockKeyVaultHelper.Object);
        }

        [Fact]
        public async Task SaveSecretToKeyVaultSuccessfully()
        {
            // Arrange
            RefreshTokenResponse testTokenResponse = new RefreshTokenResponse
            {
                AccessToken = "TestAccessToken",
                RefreshToken = "TestRefreshToken"
            };

            // Act
            Func<Task> keyVaultServiceAction = async () => await _sut.SaveTokensToKeyVault(testTokenResponse);

            // Assert
            await keyVaultServiceAction.Should().NotThrowAsync<Exception>();
            _mockKeyVaultHelper.Verify(x => x.SaveSecretToKeyVaultAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ThrowExceptionWhenSaveSecretToKeyVaultAsyncFails()
        {
            // Arrange
            RefreshTokenResponse testTokenResponse = new RefreshTokenResponse
            {
                AccessToken = "TestAccessToken",
                RefreshToken = "TestRefreshToken"
            };

            _mockKeyVaultHelper.Setup(x => x.SaveSecretToKeyVaultAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> keyVaultServiceAction = async () => await _sut.SaveTokensToKeyVault(testTokenResponse);

            // Assert
            await keyVaultServiceAction.Should().ThrowAsync<Exception>();
        }
    }
}
