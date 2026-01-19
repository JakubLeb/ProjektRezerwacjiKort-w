using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    /// <summary>
    /// Serwis obsługujący wysyłanie i zarządzanie powiadomieniami
    /// Obsługuje trzy kanały: Email, Systemowe (w aplikacji), Push (Toast)
    /// </summary>
    public interface IPowiadomienieService
    {
        // Istniejące metody
        Task WyslijPowiadomienieDlaRezerwacjiAsync(int rezerwacjaId, string tytul, string tresc);
        Task WyslijPowiadomienieEmailAsync(int uzytkownikId, string tytul, string tresc);
        Task<IEnumerable<Powiadomienie>> GetPowiadomieniaUzytkownikaAsync(int uzytkownikId);

        // Nowe metody
        /// <summary>
        /// Wysyła powiadomienie przez wybrany kanał
        /// </summary>
        Task<bool> WyslijPowiadomienieAsync(int uzytkownikId, string tytul, string tresc, TypPowiadomieniaEnum typ, int? rezerwacjaId = null);

        /// <summary>
        /// Wysyła powiadomienie wszystkimi dostępnymi kanałami
        /// </summary>
        Task WyslijPowiadomienieWszystkimiKanalamiAsync(int uzytkownikId, string tytul, string tresc, int? rezerwacjaId = null);

        /// <summary>
        /// Pobiera nieprzeczytane powiadomienia użytkownika
        /// </summary>
        Task<IEnumerable<Powiadomienie>> GetNieprzeczytanePowiadomieniaAsync(int uzytkownikId);

        /// <summary>
        /// Oznacza powiadomienie jako przeczytane
        /// </summary>
        Task<bool> OznaczJakoPrzeczytaneAsync(int powiadomienieId);

        /// <summary>
        /// Oznacza wszystkie powiadomienia użytkownika jako przeczytane
        /// </summary>
        Task OznaczWszystkieJakoPrzeczytaneAsync(int uzytkownikId);

        /// <summary>
        /// Pobiera liczbę nieprzeczytanych powiadomień
        /// </summary>
        Task<int> GetLiczbaNieprzeczytanychAsync(int uzytkownikId);

        /// <summary>
        /// Usuwa powiadomienie
        /// </summary>
        Task<bool> UsunPowiadomienieAsync(int powiadomienieId);

        /// <summary>
        /// Event wywoływany gdy przychodzi nowe powiadomienie typu Toast/Push
        /// </summary>
        event EventHandler<PowiadomienieEventArgs> NowePowiadomienieToast;
    }

    /// <summary>
    /// Typy powiadomień (zgodne z bazą danych)
    /// </summary>
    public enum TypPowiadomieniaEnum
    {
        Email = 1,
        Systemowe = 2,
        Push = 3
    }

    /// <summary>
    /// Statusy powiadomień (zgodne z bazą danych)
    /// </summary>
    public enum StatusPowiadomieniaEnum
    {
        Wyslane = 1,
        Oczekujace = 2,
        BladWysylki = 3,
        Przeczytane = 4
    }

    /// <summary>
    /// Argumenty eventu dla nowego powiadomienia Toast
    /// </summary>
    public class PowiadomienieEventArgs : EventArgs
    {
        public string Tytul { get; set; }
        public string Tresc { get; set; }
        public int UzytkownikId { get; set; }
        public TypPowiadomieniaEnum Typ { get; set; }
        public int? PowiadomienieId { get; set; }
    }
}
