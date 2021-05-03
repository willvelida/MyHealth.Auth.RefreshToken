using MyHealth.Auth.RefreshToken.Models;
using System.Threading.Tasks;

namespace MyHealth.Auth.RefreshToken.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResponse> RefreshToken();
    }
}
