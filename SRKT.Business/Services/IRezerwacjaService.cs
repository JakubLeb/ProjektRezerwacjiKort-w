using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    public interface IRezerwacjaService
    {
        Task<IEnumerable<Rezerwacja>> GetRezerwacjeUzytkownikaAsync(int uzytkownikId);
        Task<IEnumerable<Rezerwacja>> GetDostepneTerminyAsync(int kortId, DateTime data);
        Task<Rezerwacja> UtworzRezerwacjeAsync(int kortId, int uzytkownikId, DateTime dataRezerwacji, decimal iloscGodzin, string uwagi = null);
        Task<bool> AnulujRezerwacjeAsync(int rezerwacjaId);
        Task<bool> PotwierdRezerwacjeAsync(int rezerwacjaId);
        Task<IEnumerable<Rezerwacja>> PobierzWszystkieRezerwacjeZDatyAsync(DateTime data);

        Task<IEnumerable<TimeSlot>> GetWolneTerminyAsync(int kortId, DateTime data, decimal dlugoscSesji);
    }

    public class TimeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Dostepny { get; set; }

        public int KortId { get; set; }
        public string NazwaKortu { get; set; }
        public string OpisKortu { get; set; }
        public decimal Dlugosc { get; set; }
    }
}