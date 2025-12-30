using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    public interface IPowiadomienieService
    {
        Task WyslijPowiadomienieDlaRezerwacjiAsync(int rezerwacjaId, string tytul, string tresc);
        Task WyslijPowiadomienieEmailAsync(int uzytkownikId, string tytul, string tresc);
        Task<IEnumerable<Powiadomienie>> GetPowiadomieniaUzytkownikaAsync(int uzytkownikId);
    }
}