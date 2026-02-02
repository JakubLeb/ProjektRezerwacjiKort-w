using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;

namespace SRKT.Business.Services
{
    /// <summary>
    /// Implementacja serwisu przypomnień z obsługą natywnych powiadomień Windows
    /// </summary>
    public class PrzypomnienieService : IPrzypomnienieService
    {
        private readonly IRepository<Przypomnienie> _przypomnienieRepo;
        private readonly IRezerwacjaRepository _rezerwacjaRepo;
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;
        private readonly IPowiadomienieService _powiadomienieService;

        public PrzypomnienieService(
            IRepository<Przypomnienie> przypomnienieRepo,
            IRezerwacjaRepository rezerwacjaRepo,
            IRepository<Uzytkownik> uzytkownikRepo,
            IPowiadomienieService powiadomienieService)
        {
            _przypomnienieRepo = przypomnienieRepo;
            _rezerwacjaRepo = rezerwacjaRepo;
            _uzytkownikRepo = uzytkownikRepo;
            _powiadomienieService = powiadomienieService;
        }

        public async Task<Przypomnienie> UtworzPrzypomnienieAsync(
            int rezerwacjaId,
            int uzytkownikId,
            DateTime dataPrzypomnienia,
            string tytul,
            string tresc)
        {
            var przypomnienie = new Przypomnienie
            {
                RezerwacjaId = rezerwacjaId,
                UzytkownikId = uzytkownikId,
                DataPrzypomnienia = dataPrzypomnienia,
                Tytul = tytul,
                Tresc = tresc,
                CzyWyslane = false,
                CzyAktywne = true,
                DataUtworzenia = DateTime.Now
            };

            return await _przypomnienieRepo.AddAsync(przypomnienie);
        }

        public async Task<Przypomnienie> UtworzPrzypomnienieAutomatyczneAsync(int rezerwacjaId, int minutPrzed = 60)
        {
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
                return null;

            var dataPrzypomnienia = rezerwacja.DataRezerwacji.AddMinutes(-minutPrzed);

            // Nie twórz przypomnienia jeśli data już minęła
            if (dataPrzypomnienia <= DateTime.Now)
                return null;

            var tytul = "Przypomnienie o rezerwacji";
            var tresc = $"Za {minutPrzed} minut rozpoczyna się Twoja rezerwacja kortu.\n" +
                       $"Data: {rezerwacja.DataRezerwacji:dd.MM.yyyy HH:mm}\n" +
                       $"Czas trwania: {rezerwacja.IloscGodzin}h";

            return await UtworzPrzypomnienieAsync(
                rezerwacjaId,
                rezerwacja.UzytkownikId,
                dataPrzypomnienia,
                tytul,
                tresc);
        }

        public async Task<IEnumerable<Przypomnienie>> GetPrzypomnieniUzytkownikaAsync(int uzytkownikId)
        {
            var wszystkie = await _przypomnienieRepo.FindAsync(p => p.UzytkownikId == uzytkownikId);
            return wszystkie.OrderByDescending(p => p.DataPrzypomnienia);
        }

        public async Task<IEnumerable<Przypomnienie>> GetAktywnePrzypomnieniAsync(int uzytkownikId)
        {
            var aktywne = await _przypomnienieRepo.FindAsync(p =>
                p.UzytkownikId == uzytkownikId &&
                p.CzyAktywne &&
                !p.CzyWyslane);
            return aktywne.OrderBy(p => p.DataPrzypomnienia);
        }

        public async Task<IEnumerable<Przypomnienie>> GetPrzypomnieniaDowyslaniaAsync()
        {
            var teraz = DateTime.Now;
            var doWyslania = await _przypomnienieRepo.FindAsync(p =>
                p.CzyAktywne &&
                !p.CzyWyslane &&
                p.DataPrzypomnienia <= teraz);
            return doWyslania;
        }

        public async Task<IEnumerable<Przypomnienie>> GetPrzypomnieniaDlaRezerwacjiAsync(int rezerwacjaId)
        {
            return await _przypomnienieRepo.FindAsync(p => p.RezerwacjaId == rezerwacjaId);
        }

        public async Task<bool> AktualizujPrzypomnienieAsync(
            int przypomnienieId,
            DateTime nowaData,
            string nowyTytul,
            string nowaTresc)
        {
            try
            {
                var przypomnienie = await _przypomnienieRepo.GetByIdAsync(przypomnienieId);
                if (przypomnienie == null || przypomnienie.CzyWyslane)
                    return false;

                przypomnienie.DataPrzypomnienia = nowaData;
                przypomnienie.Tytul = nowyTytul;
                przypomnienie.Tresc = nowaTresc;

                await _przypomnienieRepo.UpdateAsync(przypomnienie);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AnulujPrzypomnienieAsync(int przypomnienieId)
        {
            try
            {
                var przypomnienie = await _przypomnienieRepo.GetByIdAsync(przypomnienieId);
                if (przypomnienie == null)
                    return false;

                przypomnienie.CzyAktywne = false;
                await _przypomnienieRepo.UpdateAsync(przypomnienie);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UsunPrzypomnienieAsync(int przypomnienieId)
        {
            try
            {
                await _przypomnienieRepo.DeleteAsync(przypomnienieId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> OznaczJakoWyslaneAsync(int przypomnienieId)
        {
            try
            {
                var przypomnienie = await _przypomnienieRepo.GetByIdAsync(przypomnienieId);
                if (przypomnienie == null)
                    return false;

                przypomnienie.CzyWyslane = true;
                await _przypomnienieRepo.UpdateAsync(przypomnienie);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> WyslijZaleglePrzypomnieniAsync()
        {
            var doWyslania = await GetPrzypomnieniaDowyslaniaAsync();
            int wyslane = 0;

            foreach (var przypomnienie in doWyslania)
            {
                try
                {
                    // Wyślij powiadomienie wszystkimi kanałami (w tym natywny Windows Toast)
                    if (_powiadomienieService != null)
                    {
                        await _powiadomienieService.WyslijPowiadomienieWszystkimiKanalamiAsync(
                            przypomnienie.UzytkownikId,
                            przypomnienie.Tytul,
                            przypomnienie.Tresc,
                            przypomnienie.RezerwacjaId);
                    }

                    // Oznacz jako wysłane
                    await OznaczJakoWyslaneAsync(przypomnienie.Id);
                    wyslane++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd wysyłania przypomnienia {przypomnienie.Id}: {ex.Message}");
                }
            }

            return wyslane;
        }

        public async Task SprawdzIWyslijPrzypomnieniAsync()
        {
            var wyslane = await WyslijZaleglePrzypomnieniAsync();
            if (wyslane > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Wysłano {wyslane} przypomnień.");
            }
        }
    }
}