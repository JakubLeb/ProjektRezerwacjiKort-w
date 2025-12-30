using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    public interface IAuthService
    {
        Task<Uzytkownik> LoginAsync(string email, string haslo);
        Task<Uzytkownik> RegisterAsync(string imie, string nazwisko, string email, string haslo);
        string HashPassword(string haslo);
        bool VerifyPassword(string haslo, string hash);
    }
}