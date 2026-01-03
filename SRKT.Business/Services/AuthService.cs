using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace SRKT.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<Uzytkownik> _uzytkownikRepository;

        public AuthService(IRepository<Uzytkownik> uzytkownikRepository)
        {
            _uzytkownikRepository = uzytkownikRepository;
        }

        public async Task<Uzytkownik> LoginAsync(string email, string haslo)
        {
            var users = await _uzytkownikRepository.GetAllAsync();

            string hashWpisanegoHasla = ObliczHash(haslo);

            foreach (var user in users)
            {
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
                var bytes = Encoding.UTF8.GetBytes(haslo);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        // Nowa implementacja rejestracji
        public async Task<bool> RegisterAsync(string email, string haslo, string imie, string nazwisko)
        {
            var allUsers = await _uzytkownikRepository.GetAllAsync();
            foreach (var u in allUsers)
            {
                if (u.Email.ToLower() == email.ToLower())
                    return false; 
            }

            var nowyUzytkownik = new Uzytkownik
            {
                Email = email,
                HasloHash = ObliczHash(haslo),
                Imie = imie,
                Nazwisko = nazwisko,
                RolaId = 2, 
                DataUtworzenia = DateTime.Now
            };

            await _uzytkownikRepository.AddAsync(nowyUzytkownik);
            return true;
        }

    }
}