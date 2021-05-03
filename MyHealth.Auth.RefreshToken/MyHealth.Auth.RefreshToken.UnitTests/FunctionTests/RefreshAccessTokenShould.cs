using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Auth.RefreshToken.Functions;
using MyHealth.Auth.RefreshToken.Models;
using MyHealth.Auth.RefreshToken.Services;
using MyHealth.Common;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyHealth.Auth.RefreshToken.UnitTests.FunctionTests
{
    public class RefreshAccessTokenShould
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IRefreshTokenService> _mockRefreshTokenService;
        private Mock<IKeyVaultService> _mockKeyVaultService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<ILogger> _mockLogger;

        private RefreshAccessToken _func;

        public RefreshAccessTokenShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockRefreshTokenService = new Mock<IRefreshTokenService>();
            _mockKeyVaultService = new Mock<IKeyVaultService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockLogger = new Mock<ILogger>();

            _func = new RefreshAccessToken(
                _mockConfiguration.Object,
                _mockRefreshTokenService.Object,
                _mockKeyVaultService.Object,
                _mockServiceBusHelpers.Object);
        }

        [Fact]
        public async Task RefreshAndSaveTokenToKeyVaultSuccessfully()
        {
            // Arrange
            var timer = default(TimerInfo);
            var testRefreshTokenResponse = new RefreshTokenResponse
            {
                AccessToken = "TestAccessToken",
                RefreshToken = "TestRefreshToken"
            };

            _mockRefreshTokenService.Setup(x => x.RefreshToken()).ReturnsAsync(testRefreshTokenResponse);
            _mockKeyVaultService.Setup(x => x.SaveTokensToKeyVault(It.IsAny<RefreshTokenResponse>())).Returns(Task.CompletedTask);

            // Act
            await _func.Run(timer, _mockLogger.Object);

            // Assert
            _mockRefreshTokenService.Verify(x => x.RefreshToken(), Times.Once);
            _mockKeyVaultService.Verify(x => x.SaveTokensToKeyVault(It.IsAny<RefreshTokenResponse>()), Times.Once);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task CatchAndThrowExceptionWhenRefreshTokenServiceThrowsException()
        {
            // Arrange
            var timer = default(TimerInfo);

            _mockRefreshTokenService.Setup(x => x.RefreshToken()).ThrowsAsync(new Exception());

            // Act
            Func<Task> refreshAccessTokenAction = async () => await _func.Run(timer, _mockLogger.Object);

            // Assert
            await refreshAccessTokenAction.Should().ThrowAsync<Exception>();
            _mockKeyVaultService.Verify(x => x.SaveTokensToKeyVault(It.IsAny<RefreshTokenResponse>()), Times.Never);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task CatchAndThrowExceptionWhenKeyVaultServiceThrowsException()
        {
            // Arrange
            var timer = default(TimerInfo);
            var testRefreshTokenResponse = new RefreshTokenResponse
            {
                AccessToken = "TestAccessToken",
                RefreshToken = "TestRefreshToken"
            };

            _mockRefreshTokenService.Setup(x => x.RefreshToken()).ReturnsAsync(testRefreshTokenResponse);
            _mockKeyVaultService.Setup(x => x.SaveTokensToKeyVault(It.IsAny<RefreshTokenResponse>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> refreshAccessTokenAction = async () => await _func.Run(timer, _mockLogger.Object);

            // Assert
            await refreshAccessTokenAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
