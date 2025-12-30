using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace SRKT.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;

        public AuthService(IRepository<Uzytkownik> uzytkownikRepo)
        {
            _uzytkownikRepo = uzytkownikRepo;
        }

        public async Task<Uzytkownik> LoginAsync(string email, string haslo)
        {
            var uzytkownicy = await _uzytkownikRepo.FindAsync(u => u.Email == email);
            var uzytkownik = uzytkownicy.FirstOrDefault();

            if (uzytkownik == null)
                return null;

            if (!VerifyPassword(haslo, uzytkownik.HasloHash))
                return null;

            return uzytkownik;
        }

        public async Task<Uzytkownik> RegisterAsync(string imie, string nazwisko, string email, string haslo)
        {
            var istniejacy = await _uzytkownikRepo.FindAsync(u => u.Email == email);
            if (istniejacy.Any())
                throw new Exception("Użytkownik o podanym adresie email już istnieje.");

            var uzytkownik = new Uzytkownik
            {
                Imie = imie,
                Nazwisko = nazwisko,
                Email = email,
                HasloHash = HashPassword(haslo),
                RolaId = 2, // Użytkownik
                DataUtworzenia = DateTime.Now
            };

            return await _uzytkownikRepo.AddAsync(uzytkownik);
        }

        public string HashPassword(string haslo)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(haslo));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string haslo, string hash)
        {
            var hashOfInput = HashPassword(haslo);
            return hashOfInput == hash;
        }
    }
}