using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;

namespace SRKT.Business.Services
{
    public class RezerwacjaService : IRezerwacjaService
    {
        private readonly IRezerwacjaRepository _rezerwacjaRepo;
        private readonly IKortRepository _kortRepo;
        private readonly IPowiadomienieService _powiadomienieService;

        public RezerwacjaService(
            IRezerwacjaRepository rezerwacjaRepo,
            IKortRepository kortRepo,
            IPowiadomienieService powiadomienieService)
        {
            _rezerwacjaRepo = rezerwacjaRepo;
            _kortRepo = kortRepo;
            _powiadomienieService = powiadomienieService;
        }

        public async Task<IEnumerable<Rezerwacja>> GetRezerwacjeUzytkownikaAsync(int uzytkownikId)
        {
            return await _rezerwacjaRepo.GetRezerwacjeByUzytkownikAsync(uzytkownikId);
        }

        public async Task<IEnumerable<Rezerwacja>> GetDostepneTerminyAsync(int kortId, DateTime data)
        {
            return await _rezerwacjaRepo.GetRezerwacjeByKortAsync(kortId, data);
        }

        public async Task<Rezerwacja> UtworzRezerwacjeAsync(int kortId, int uzytkownikId, DateTime dataRezerwacji, decimal iloscGodzin, string uwagi = null)
        {
            // Sprawdź dostępność
            var czyDostepny = await _rezerwacjaRepo.CzyKortDostepnyAsync(kortId, dataRezerwacji, iloscGodzin);
            if (!czyDostepny)
                throw new Exception("Kort nie jest dostępny w wybranym terminie.");

            // Pobierz kort dla ceny
            var kort = await _kortRepo.GetByIdAsync(kortId);
            if (kort == null || !kort.CzyAktywny)
                throw new Exception("Kort nie istnieje lub jest nieaktywny.");

            // Utwórz rezerwację
            var rezerwacja = new Rezerwacja
            {
                KortId = kortId,
                UzytkownikId = uzytkownikId,
                DataRezerwacji = dataRezerwacji,
                IloscGodzin = iloscGodzin,
                KosztCalkowity = kort.CenaZaGodzine * iloscGodzin,
                CzyOplacona = false,
                StatusRezerwacjiId = 1, // Oczekująca
                DataUtworzenia = DateTime.Now,
                Uwagi = uwagi
            };

            var nowaRezerwacja = await _rezerwacjaRepo.AddAsync(rezerwacja);

            // Wyślij powiadomienie
            await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                nowaRezerwacja.Id,
                "Nowa rezerwacja",
                $"Twoja rezerwacja na {dataRezerwacji:dd.MM.yyyy HH:mm} została utworzona."
            );

            return nowaRezerwacja;
        }

        public async Task<bool> AnulujRezerwacjeAsync(int rezerwacjaId)
        {
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
                return false;

            rezerwacja.StatusRezerwacjiId = 3; // Anulowana
            rezerwacja.DataModyfikacji = DateTime.Now;
            await _rezerwacjaRepo.UpdateAsync(rezerwacja);

            await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                rezerwacjaId,
                "Rezerwacja anulowana",
                "Twoja rezerwacja została anulowana."
            );

            return true;
        }

        public async Task<bool> PotwierdRezerwacjeAsync(int rezerwacjaId)
        {
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
                return false;

            rezerwacja.StatusRezerwacjiId = 2; // Potwierdzona
            rezerwacja.DataModyfikacji = DateTime.Now;
            await _rezerwacjaRepo.UpdateAsync(rezerwacja);

            await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                rezerwacjaId,
                "Rezerwacja potwierdzona",
                "Twoja rezerwacja została potwierdzona."
            );

            return true;
        }

        public async Task<IEnumerable<TimeSlot>> GetWolneTerminyAsync(int kortId, DateTime data)
        {
            var rezerwacje = await _rezerwacjaRepo.GetRezerwacjeByKortAsync(kortId, data);
            var slots = new List<TimeSlot>();

            var godzinyPracy = new TimeSpan(9, 0, 0); // 9:00
            var godzinaKonca = new TimeSpan(21, 0, 0); // 21:00

            var currentTime = data.Date.Add(godzinyPracy);
            var endTime = data.Date.Add(godzinaKonca);

            while (currentTime < endTime)
            {
                var slotEnd = currentTime.AddHours(1);
                var rezerwacjaWSlot = rezerwacje.Any(r =>
                    (r.DataRezerwacji <= currentTime && r.DataRezerwacji.AddHours((double)r.IloscGodzin) > currentTime) ||
                    (r.DataRezerwacji < slotEnd && r.DataRezerwacji >= currentTime)
                );

                slots.Add(new TimeSlot
                {
                    Start = currentTime,
                    End = slotEnd,
                    Dostepny = !rezerwacjaWSlot
                });

                currentTime = slotEnd;
            }

            return slots;
        }
    }
}