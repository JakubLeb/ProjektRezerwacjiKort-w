using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    public interface IPowiadomienieService
    {
        Task WyslijPowiadomienieDlaRezerwacjiAsync(int rezerwacjaId, string tytul, string tresc);
        Task WyslijPowiadomienieEmailAsync(int uzytkownikId, string tytul, string tresc);
        Task<IEnumerable<Powiadomienie>> GetPowiadomieniaUzytkownikaAsync(int uzytkownikId);

        Task<bool> WyslijPowiadomienieAsync(int uzytkownikId, string tytul, string tresc, TypPowiadomieniaEnum typ, int? rezerwacjaId = null);

        Task WyslijPowiadomienieWszystkimiKanalamiAsync(int uzytkownikId, string tytul, string tresc, int? rezerwacjaId = null);

        Task<IEnumerable<Powiadomienie>> GetNieprzeczytanePowiadomieniaAsync(int uzytkownikId);

        Task<bool> OznaczJakoPrzeczytaneAsync(int powiadomienieId);

        Task OznaczWszystkieJakoPrzeczytaneAsync(int uzytkownikId);

        Task<int> GetLiczbaNieprzeczytanychAsync(int uzytkownikId);

        Task<bool> UsunPowiadomienieAsync(int powiadomienieId);

        event EventHandler<PowiadomienieEventArgs> NowePowiadomienieToast;
    }

    public enum TypPowiadomieniaEnum
    {
        Email = 1,
        Systemowe = 2,
        Push = 3
    }

    public enum StatusPowiadomieniaEnum
    {
        Wyslane = 1,
        Oczekujace = 2,
        BladWysylki = 3,
        Przeczytane = 4
    }

    public class PowiadomienieEventArgs : EventArgs
    {
        public string Tytul { get; set; }
        public string Tresc { get; set; }
        public int UzytkownikId { get; set; }
        public TypPowiadomieniaEnum Typ { get; set; }
        public int? PowiadomienieId { get; set; }
    }
}
