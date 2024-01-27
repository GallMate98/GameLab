using GameLab.Models;

namespace GameLab.Services.Token
{
    public interface ITokenService
    {
       string CreateJWTToken(User user, List<string> roles);
    }
}
