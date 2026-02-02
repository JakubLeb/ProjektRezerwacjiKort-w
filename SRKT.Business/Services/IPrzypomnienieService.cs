using SRKT.Core.Models;

namespace SRKT.Business.Services
{

    public interface IPrzypomnienieService
    {
        Task<Przypomnienie> UtworzPrzypomnienieAsync(int rezerwacjaId, int uzytkownikId, DateTime dataPrzypomnienia, string tytul, string tresc);

        Task<Przypomnienie> UtworzPrzypomnienieAutomatyczneAsync(int rezerwacjaId, int minutPrzed = 60);

        Task<IEnumerable<Przypomnienie>> GetPrzypomnieniUzytkownikaAsync(int uzytkownikId);

        Task<IEnumerable<Przypomnienie>> GetAktywnePrzypomnieniAsync(int uzytkownikId);

        Task<IEnumerable<Przypomnienie>> GetPrzypomnieniaDowyslaniaAsync();

        Task<IEnumerable<Przypomnienie>> GetPrzypomnieniaDlaRezerwacjiAsync(int rezerwacjaId);

        Task<bool> AktualizujPrzypomnienieAsync(int przypomnienieId, DateTime nowaData, string nowyTytul, string nowaTresc);


        Task<bool> AnulujPrzypomnienieAsync(int przypomnienieId);


        Task<bool> UsunPrzypomnienieAsync(int przypomnienieId);

        Task<bool> OznaczJakoWyslaneAsync(int przypomnienieId);

        Task<int> WyslijZaleglePrzypomnieniAsync();

        Task SprawdzIWyslijPrzypomnieniAsync();
    }
}
