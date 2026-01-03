using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;

namespace SRKT.Business.Services
{
    public class PowiadomienieService : IPowiadomienieService
    {
        private readonly IRepository<Powiadomienie> _powiadomienieRepo;
        private readonly IRezerwacjaRepository _rezerwacjaRepo;

        public PowiadomienieService(
            IRepository<Powiadomienie> powiadomienieRepo,
            IRezerwacjaRepository rezerwacjaRepo)
        {
            _powiadomienieRepo = powiadomienieRepo;
            _rezerwacjaRepo = rezerwacjaRepo;
        }

        public async Task WyslijPowiadomienieDlaRezerwacjiAsync(int rezerwacjaId, string tytul, string tresc)
        {
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
                return;

            var powiadomienie = new Powiadomienie
            {
                UzytkownikId = rezerwacja.UzytkownikId,
                RezerwacjaId = rezerwacjaId,
                Tytul = tytul,
                Tresc = tresc,
                TypPowiadomieniaId = 2,
                StatusPowiadomieniaId = 1,
                DataWyslania = DateTime.Now,
                DataUtworzenia = DateTime.Now
            };

            await _powiadomienieRepo.AddAsync(powiadomienie);
        }

        public async Task WyslijPowiadomienieEmailAsync(int uzytkownikId, string tytul, string tresc)
        {
            var powiadomienie = new Powiadomienie
            {
                UzytkownikId = uzytkownikId,
                Tytul = tytul,
                Tresc = tresc,
                TypPowiadomieniaId = 1,
                StatusPowiadomieniaId = 2,
                DataUtworzenia = DateTime.Now
            };

            await _powiadomienieRepo.AddAsync(powiadomienie);
        }

        public async Task<IEnumerable<Powiadomienie>> GetPowiadomieniaUzytkownikaAsync(int uzytkownikId)
        {
            return await _powiadomienieRepo.FindAsync(p => p.UzytkownikId == uzytkownikId);
        }
    }
}