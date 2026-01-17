using SRKT.Core.Models;

namespace SRKT.Business.Services
{
    public interface IPlatnoscService
    {
        Task<bool> PrzetworzPlatnoscBlikAsync(int rezerwacjaId, string kodBlik, decimal kwota);
        Task<Platnosc> GetPlatnoscByRezerwacjaAsync(int rezerwacjaId);
    }
}