using AniDrop.Domain;
namespace AniDrop.Services.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateToken(User user);
}
