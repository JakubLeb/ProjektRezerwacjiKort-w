using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore; // Potrzebne do FirstOrDefaultAsync
using System.Threading.Tasks;

namespace SRKT.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<Uzytkownik> _uzytkownikRepository;

        // Upewnij się, że konstruktor przyjmuje repozytorium
        public AuthService(IRepository<Uzytkownik> uzytkownikRepository)
        {
            _uzytkownikRepository = uzytkownikRepository;
        }

        public async Task<Uzytkownik> LoginAsync(string email, string haslo)
        {
            // Twoja istniejąca logika logowania...
            var users = await _uzytkownikRepository.GetAllAsync();
            // Prosta weryfikacja (w przyszłości warto dodać haszowanie haseł)
            foreach (var user in users)
            {
                if (user.Email == email && user.HasloHash == haslo)
                    return user;
            }
            return null;
        }

        // Nowa implementacja rejestracji
        public async Task<bool> RegisterAsync(string email, string haslo, string imie, string nazwisko)
        {
            // 1. Sprawdź, czy taki email już istnieje
            var allUsers = await _uzytkownikRepository.GetAllAsync();
            foreach (var u in allUsers)
            {
                if (u.Email.ToLower() == email.ToLower())
                    return false; // Użytkownik już istnieje
            }

            // 2. Utwórz nowy obiekt użytkownika
            var nowyUzytkownik = new Uzytkownik
            {
                Email = email,
                HasloHash = haslo, // UWAGA: W wersji produkcyjnej tutaj należy hasło zahaszować!
                Imie = imie,
                Nazwisko = nazwisko,
                RolaId = 2 // Zakładamy, że 2 to ID roli "Klient" lub "Użytkownik"
            };

            // 3. Zapisz w bazie
            await _uzytkownikRepository.AddAsync(nowyUzytkownik);
            return true;
        }
    }
}