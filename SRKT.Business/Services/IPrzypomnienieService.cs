using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    /// <summary>
    /// Serwis do zarządzania przypomnieniami
    /// </summary>
    public interface IPrzypomnienieService
    {
        /// <summary>
        /// Tworzy nowe przypomnienie dla rezerwacji
        /// </summary>
        Task<Przypomnienie> UtworzPrzypomnienieAsync(int rezerwacjaId, int uzytkownikId, DateTime dataPrzypomnienia, string tytul, string tresc);

        /// <summary>
        /// Tworzy automatyczne przypomnienie X minut przed rezerwacją
        /// </summary>
        Task<Przypomnienie> UtworzPrzypomnienieAutomatyczneAsync(int rezerwacjaId, int minutPrzed = 60);

        /// <summary>
        /// Pobiera wszystkie przypomnienia użytkownika
        /// </summary>
        Task<IEnumerable<Przypomnienie>> GetPrzypomnieniUzytkownikaAsync(int uzytkownikId);

        /// <summary>
        /// Pobiera aktywne przypomnienia użytkownika
        /// </summary>
        Task<IEnumerable<Przypomnienie>> GetAktywnePrzypomnieniAsync(int uzytkownikId);

        /// <summary>
        /// Pobiera przypomnienia do wysłania (data przypomnienia <= teraz)
        /// </summary>
        Task<IEnumerable<Przypomnienie>> GetPrzypomnieniaDowyslaniaAsync();

        /// <summary>
        /// Pobiera przypomnienia dla konkretnej rezerwacji
        /// </summary>
        Task<IEnumerable<Przypomnienie>> GetPrzypomnieniaDlaRezerwacjiAsync(int rezerwacjaId);

        /// <summary>
        /// Aktualizuje przypomnienie
        /// </summary>
        Task<bool> AktualizujPrzypomnienieAsync(int przypomnienieId, DateTime nowaData, string nowyTytul, string nowaTresc);

        /// <summary>
        /// Anuluje (dezaktywuje) przypomnienie
        /// </summary>
        Task<bool> AnulujPrzypomnienieAsync(int przypomnienieId);

        /// <summary>
        /// Usuwa przypomnienie
        /// </summary>
        Task<bool> UsunPrzypomnienieAsync(int przypomnienieId);

        /// <summary>
        /// Oznacza przypomnienie jako wysłane
        /// </summary>
        Task<bool> OznaczJakoWyslaneAsync(int przypomnienieId);

        /// <summary>
        /// Wysyła wszystkie zaległe przypomnienia
        /// </summary>
        Task<int> WyslijZaleglePrzypomnieniAsync();

        /// <summary>
        /// Sprawdza i wysyła przypomnienia (do wywołania okresowo)
        /// </summary>
        Task SprawdzIWyslijPrzypomnieniAsync();
    }
}
