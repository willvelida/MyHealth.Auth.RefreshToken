using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.Auth.RefreshToken.Services;
using MyHealth.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MyHealth.Auth.RefreshToken.UnitTests.ServiceTests
{
    public class RefreshTokenServiceShould
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IKeyVaultHelper> _mockKeyVaultHelper;
        private Mock<HttpClient> _mockHttpClient;

        private RefreshTokenService _sut;

        public RefreshTokenServiceShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockKeyVaultHelper = new Mock<IKeyVaultHelper>();
            _mockHttpClient = new Mock<HttpClient>();

            _sut = new RefreshTokenService(
                _mockConfiguration.Object,
                _mockKeyVaultHelper.Object,
                _mockHttpClient.Object);
        }

        [Fact]
        public async Task CatchAndThrowExceptionWhenRetrieveSecretFromKeyVaultFails()
        {
            // Arrange
            _mockKeyVaultHelper.Setup(x => x.RetrieveSecretFromKeyVaultAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> refreshTokenAction = async () => await _sut.RefreshToken();

            // Assert
            await refreshTokenAction.Should().ThrowAsync<Exception>();
        }
    }
}
