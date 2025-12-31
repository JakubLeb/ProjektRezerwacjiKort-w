using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore; // Potrzebne do FirstOrDefaultAsync
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

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
            var users = await _uzytkownikRepository.GetAllAsync();

            // Oblicz hash wpisanego hasła, aby porównać go z tym w bazie
            string hashWpisanegoHasla = ObliczHash(haslo);

            foreach (var user in users)
            {
                // Porównujemy Email oraz Hashe haseł
                if (user.Email.ToLower() == email.ToLower() && user.HasloHash == hashWpisanegoHasla)
                {
                    return user;
                }
            }
            return null;
        }
        private string ObliczHash(string haslo)
        {
            using (var sha256 = SHA256.Create())
            {
                // Zamień string na tablicę bajtów
                var bytes = Encoding.UTF8.GetBytes(haslo);
                // Oblicz hash
                var hash = sha256.ComputeHash(bytes);
                // Zamień tablicę bajtów z powrotem na string (Base64)
                return Convert.ToBase64String(hash);
            }
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
                HasloHash = ObliczHash(haslo), // UWAGA: W wersji produkcyjnej tutaj należy hasło zahaszować!
                Imie = imie,
                Nazwisko = nazwisko,
                RolaId = 2, // Zakładamy, że 2 to ID roli "Klient" lub "Użytkownik"
                DataUtworzenia = DateTime.Now
            };

            // 3. Zapisz w bazie
            await _uzytkownikRepository.AddAsync(nowyUzytkownik);
            return true;
        }

    }
}