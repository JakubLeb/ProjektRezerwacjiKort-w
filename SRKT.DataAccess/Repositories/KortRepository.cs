using Microsoft.EntityFrameworkCore;
using SRKT.Core.Models;

namespace SRKT.DataAccess.Repositories
{
    public interface IKortRepository : IRepository<Kort>
    {
        Task<IEnumerable<Kort>> GetAktywneKortyAsync();
        Task<IEnumerable<Kort>> GetKortyByObiektAsync(int obiektId);
        Task<IEnumerable<Kort>> GetKortyByTypAsync(int typKortuId);
    }

    public class KortRepository : Repository<Kort>, IKortRepository
    {
        public KortRepository(SRKTDbContext context) : base(context) { }

        public async Task<IEnumerable<Kort>> GetAktywneKortyAsync()
        {
            return await _dbSet
                .Include(k => k.TypKortu)
                .Include(k => k.ObiektSportowy)
                .Where(k => k.CzyAktywny)
                .ToListAsync();
        }

        public async Task<IEnumerable<Kort>> GetKortyByObiektAsync(int obiektId)
        {
            return await _dbSet
                .Include(k => k.TypKortu)
                .Where(k => k.ObiektSportowyId == obiektId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Kort>> GetKortyByTypAsync(int typKortuId)
        {
            return await _dbSet
                .Include(k => k.ObiektSportowy)
                .Where(k => k.TypKortuId == typKortuId && k.CzyAktywny)
                .ToListAsync();
        }
    }
}
