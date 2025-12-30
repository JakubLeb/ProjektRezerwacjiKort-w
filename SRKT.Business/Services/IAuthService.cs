using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    public interface IAuthService
    {
        Task<Uzytkownik> LoginAsync(string email, string haslo);
        Task<bool> RegisterAsync(string email, string haslo, string imie, string nazwisko);
    }
}