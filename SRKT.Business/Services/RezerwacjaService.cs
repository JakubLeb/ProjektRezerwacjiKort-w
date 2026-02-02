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
            if (kort == null)
                throw new Exception("Kort nie istnieje.");

            if (!kort.CzyAktywny)
                throw new Exception("Kort jest nieaktywny.");

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

            // Wyślij powiadomienie (z obsługą null)
            if (_powiadomienieService != null)
            {
                try
                {
                    await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                        nowaRezerwacja.Id,
                        "Nowa rezerwacja",
                        $"Twoja rezerwacja na {dataRezerwacji:dd.MM.yyyy HH:mm} została utworzona."
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd wysyłania powiadomienia: {ex.Message}");
                }
            }

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

            if (_powiadomienieService != null)
            {
                try
                {
                    await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                        rezerwacjaId,
                        "Rezerwacja anulowana",
                        "Twoja rezerwacja została anulowana."
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd wysyłania powiadomienia: {ex.Message}");
                }
            }

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

            if (_powiadomienieService != null)
            {
                try
                {
                    await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                        rezerwacjaId,
                        "Rezerwacja potwierdzona",
                        "Twoja rezerwacja została potwierdzona."
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd wysyłania powiadomienia: {ex.Message}");
                }
            }

            return true;
        }

        /// <summary>
        /// Oznacza rezerwację jako opłaconą (symulacja płatności gotówką na miejscu)
        /// </summary>
        public async Task<bool> OznaczJakoOplaconeAsync(int rezerwacjaId)
        {
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
                return false;

            // Sprawdź czy już nie jest opłacona
            if (rezerwacja.CzyOplacona)
                return true; // Już opłacona - sukces

            rezerwacja.CzyOplacona = true;
            rezerwacja.DataModyfikacji = DateTime.Now;
            await _rezerwacjaRepo.UpdateAsync(rezerwacja);

            // Wyślij powiadomienie do klienta
            if (_powiadomienieService != null)
            {
                try
                {
                    await _powiadomienieService.WyslijPowiadomienieDlaRezerwacjiAsync(
                        rezerwacjaId,
                        "Płatność przyjęta",
                        $"Płatność za rezerwację na {rezerwacja.DataRezerwacji:dd.MM.yyyy HH:mm} została przyjęta. Dziękujemy!"
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd wysyłania powiadomienia: {ex.Message}");
                }
            }

            return true;
        }

        /// <summary>
        /// Pobiera wszystkie rezerwacje z danego dnia z pełnymi danymi relacyjnymi
        /// (Kort, ObiektSportowy, Uzytkownik, StatusRezerwacji)
        /// </summary>
        public async Task<IEnumerable<Rezerwacja>> PobierzWszystkieRezerwacjeZDatyAsync(DateTime data)
        {
            // Używamy nowej metody z repozytorium, która pobiera pełne dane
            var dataOd = data.Date;
            var dataDo = data.Date.AddDays(1).AddTicks(-1);

            return await _rezerwacjaRepo.GetRezerwacjeByDataAsync(dataOd, dataDo);
        }

        public async Task<IEnumerable<TimeSlot>> GetWolneTerminyAsync(int kortId, DateTime data, decimal dlugoscSesji)
        {
            // Pobierz kort z pełnymi danymi
            var kort = await _kortRepo.GetByIdAsync(kortId);

            // WALIDACJA: Sprawdź czy kort istnieje
            if (kort == null)
            {
                System.Diagnostics.Debug.WriteLine($"GetWolneTerminyAsync: Kort o ID {kortId} nie istnieje.");
                return Enumerable.Empty<TimeSlot>();
            }

            string nazwaKortu = kort.Nazwa ?? "Nieznany";
            string opisKortu = $"{kort.Nazwa ?? "Nieznany"} - {kort.TypKortu?.Nazwa ?? "Brak typu"}";

            var rezerwacje = await _rezerwacjaRepo.GetRezerwacjeByKortAsync(kortId, data);
            var slots = new List<TimeSlot>();

            var godzinyPracy = new TimeSpan(8, 0, 0);
            var godzinaKonca = new TimeSpan(22, 0, 0);

            var currentTime = data.Date.Add(godzinyPracy);
            var endTime = data.Date.Add(godzinaKonca);

            while (currentTime.AddHours((double)dlugoscSesji) <= endTime)
            {
                var slotEnd = currentTime.AddHours((double)dlugoscSesji);

                // Sprawdzenie kolizji dla całego zakresu (Start -> Start + Dlugosc)
                var rezerwacjaWSlot = rezerwacje.Any(r =>
                    r.StatusRezerwacjiId != 3 &&
                    (
                        (r.DataRezerwacji < slotEnd && r.DataRezerwacji.AddHours((double)r.IloscGodzin) > currentTime)
                    )
                );

                slots.Add(new TimeSlot
                {
                    Start = currentTime,
                    End = slotEnd,
                    Dostepny = !rezerwacjaWSlot,
                    KortId = kortId,
                    NazwaKortu = nazwaKortu,
                    OpisKortu = opisKortu,
                    Dlugosc = dlugoscSesji
                });

                // Przesuwamy się o 1h 
                currentTime = currentTime.AddHours(1);
            }

            return slots;
        }
    }
}