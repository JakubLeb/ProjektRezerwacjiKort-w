using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;

namespace SRKT.Business.Services
{
    public class PlatnoscService : IPlatnoscService
    {
        private readonly IRepository<Platnosc> _platnoscRepo;
        private readonly IRezerwacjaRepository _rezerwacjaRepo;

        public PlatnoscService(
            IRepository<Platnosc> platnoscRepo,
            IRezerwacjaRepository rezerwacjaRepo)
        {
            _platnoscRepo = platnoscRepo;
            _rezerwacjaRepo = rezerwacjaRepo;
        }

        public async Task<bool> PrzetworzPlatnoscBlikAsync(int rezerwacjaId, string kodBlik, decimal kwota)
        {
            // Symulacja przetwarzania płatności (2 sekundy opóźnienia)
            await Task.Delay(2000);

            // Walidacja kodu BLIK (6 cyfr)
            if (string.IsNullOrWhiteSpace(kodBlik) || kodBlik.Length != 6 || !kodBlik.All(char.IsDigit))
            {
                return false;
            }

            // Pobierz rezerwację
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
            {
                return false;
            }

            // Utwórz rekord płatności
            var platnosc = new Platnosc
            {
                RezerwacjaId = rezerwacjaId,
                Kwota = kwota,
                CzyPlatnoscZatwierdzona = true,
                MetodaPlatnosciId = 1, // 1 = BLIK
                DataUtworzenia = DateTime.Now
            };

            await _platnoscRepo.AddAsync(platnosc);

            // Zaktualizuj status rezerwacji
            rezerwacja.CzyOplacona = true;
            rezerwacja.DataModyfikacji = DateTime.Now;
            await _rezerwacjaRepo.UpdateAsync(rezerwacja);

            return true;
        }

        public async Task<Platnosc> GetPlatnoscByRezerwacjaAsync(int rezerwacjaId)
        {
            var platnosci = await _platnoscRepo.FindAsync(p => p.RezerwacjaId == rezerwacjaId);
            return platnosci.FirstOrDefault();
        }
    }
}