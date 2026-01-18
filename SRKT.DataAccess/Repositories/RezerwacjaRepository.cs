using Microsoft.EntityFrameworkCore;
using SRKT.Core.Models;

namespace SRKT.DataAccess.Repositories
{
    public interface IRezerwacjaRepository : IRepository<Rezerwacja>
    {
        Task<IEnumerable<Rezerwacja>> GetRezerwacjeByUzytkownikAsync(int uzytkownikId);
        Task<IEnumerable<Rezerwacja>> GetRezerwacjeByKortAsync(int kortId, DateTime data);
        Task<IEnumerable<Rezerwacja>> GetRezerwacjeByDataAsync(DateTime dataOd, DateTime dataDo);
        Task<bool> CzyKortDostepnyAsync(int kortId, DateTime dataRezerwacji, decimal iloscGodzin);
    }

    public class RezerwacjaRepository : Repository<Rezerwacja>, IRezerwacjaRepository
    {
        public RezerwacjaRepository(SRKTDbContext context) : base(context) { }

        public async Task<IEnumerable<Rezerwacja>> GetRezerwacjeByUzytkownikAsync(int uzytkownikId)
        {
            return await _dbSet
                .Include(r => r.Kort)
                    .ThenInclude(k => k.ObiektSportowy)
                .Include(r => r.Kort)
                    .ThenInclude(k => k.TypKortu)
                .Include(r => r.StatusRezerwacji)
                .Where(r => r.UzytkownikId == uzytkownikId)
                .OrderByDescending(r => r.DataRezerwacji)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezerwacja>> GetRezerwacjeByKortAsync(int kortId, DateTime data)
        {
            var dataStart = data.Date;
            var dataEnd = data.Date.AddDays(1);

            return await _dbSet
                .Include(r => r.Uzytkownik)
                .Include(r => r.StatusRezerwacji)
                .Include(r => r.Kort)
                    .ThenInclude(k => k.ObiektSportowy)
                .Where(r => r.KortId == kortId
                    && r.DataRezerwacji >= dataStart
                    && r.DataRezerwacji < dataEnd
                    && r.StatusRezerwacjiId != 3) // 3 = Anulowana
                .OrderBy(r => r.DataRezerwacji)
                .ToListAsync();
        }

        /// <summary>
        /// Pobiera rezerwacje z zakresu dat z pełnymi danymi relacyjnymi:
        /// - Kort (z ObiektSportowy i TypKortu)
        /// - Uzytkownik
        /// - StatusRezerwacji
        /// </summary>
        public async Task<IEnumerable<Rezerwacja>> GetRezerwacjeByDataAsync(DateTime dataOd, DateTime dataDo)
        {
            return await _dbSet
                .Include(r => r.Kort)
                    .ThenInclude(k => k.ObiektSportowy)
                .Include(r => r.Kort)
                    .ThenInclude(k => k.TypKortu)
                .Include(r => r.Uzytkownik)
                .Include(r => r.StatusRezerwacji)
                .Where(r => r.DataRezerwacji >= dataOd && r.DataRezerwacji <= dataDo)
                .OrderBy(r => r.DataRezerwacji)
                .ToListAsync();
        }

        public async Task<bool> CzyKortDostepnyAsync(int kortId, DateTime dataRezerwacji, decimal iloscGodzin)
        {
            var dataZakonczenia = dataRezerwacji.AddHours((double)iloscGodzin);

            var konflikty = await _dbSet
                .Where(r => r.KortId == kortId
                    && r.StatusRezerwacjiId != 3 // Nie anulowana
                    && ((r.DataRezerwacji < dataZakonczenia && r.DataRezerwacji.AddHours((double)r.IloscGodzin) > dataRezerwacji)))
                .AnyAsync();

            return !konflikty;
        }
    }
}