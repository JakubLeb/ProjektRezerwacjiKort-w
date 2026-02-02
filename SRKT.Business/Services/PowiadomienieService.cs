using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;

namespace SRKT.Business.Services
{
    public class PowiadomienieService : IPowiadomienieService
    {
        private readonly IRepository<Powiadomienie> _powiadomienieRepo;
        private readonly IRezerwacjaRepository _rezerwacjaRepo;
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;

        // Event dla powiadomień Toast/Push
        public event EventHandler<PowiadomienieEventArgs> NowePowiadomienieToast;

        // Symulacja konfiguracji email (w produkcji byłoby to w appsettings.json)
        private readonly bool _emailEnabled = true;

        public PowiadomienieService(
            IRepository<Powiadomienie> powiadomienieRepo,
            IRezerwacjaRepository rezerwacjaRepo,
            IRepository<Uzytkownik> uzytkownikRepo = null)
        {
            _powiadomienieRepo = powiadomienieRepo;
            _rezerwacjaRepo = rezerwacjaRepo;
            _uzytkownikRepo = uzytkownikRepo;
        }

        #region Istniejące metody (zachowane dla kompatybilności)

        public async Task WyslijPowiadomienieDlaRezerwacjiAsync(int rezerwacjaId, string tytul, string tresc)
        {
            var rezerwacja = await _rezerwacjaRepo.GetByIdAsync(rezerwacjaId);
            if (rezerwacja == null)
                return;

            // Wysyłamy powiadomienie systemowe (widoczne w aplikacji)
            await WyslijPowiadomienieAsync(
                rezerwacja.UzytkownikId,
                tytul,
                tresc,
                TypPowiadomieniaEnum.Systemowe,
                rezerwacjaId);

            // Dodatkowo wysyłamy natywny Windows Toast
            await WyslijPowiadomienieAsync(
                rezerwacja.UzytkownikId,
                tytul,
                tresc,
                TypPowiadomieniaEnum.Push,
                rezerwacjaId);
        }

        public async Task WyslijPowiadomienieEmailAsync(int uzytkownikId, string tytul, string tresc)
        {
            await WyslijPowiadomienieAsync(uzytkownikId, tytul, tresc, TypPowiadomieniaEnum.Email);
        }

        public async Task<IEnumerable<Powiadomienie>> GetPowiadomieniaUzytkownikaAsync(int uzytkownikId)
        {
            var wszystkie = await _powiadomienieRepo.FindAsync(p => p.UzytkownikId == uzytkownikId);
            return wszystkie.OrderByDescending(p => p.DataUtworzenia);
        }

        #endregion

        #region Nowe metody

        /// <summary>
        /// Główna metoda wysyłająca powiadomienie przez wybrany kanał
        /// </summary>
        public async Task<bool> WyslijPowiadomienieAsync(
            int uzytkownikId,
            string tytul,
            string tresc,
            TypPowiadomieniaEnum typ,
            int? rezerwacjaId = null)
        {
            try
            {
                // Utwórz rekord powiadomienia w bazie
                var powiadomienie = new Powiadomienie
                {
                    UzytkownikId = uzytkownikId,
                    RezerwacjaId = rezerwacjaId,
                    Tytul = tytul,
                    Tresc = tresc,
                    TypPowiadomieniaId = (int)typ,
                    StatusPowiadomieniaId = (int)StatusPowiadomieniaEnum.Oczekujace,
                    DataUtworzenia = DateTime.Now
                };

                // Obsługa różnych kanałów
                bool sukces = false;
                switch (typ)
                {
                    case TypPowiadomieniaEnum.Email:
                        sukces = await WyslijEmailAsync(uzytkownikId, tytul, tresc);
                        break;

                    case TypPowiadomieniaEnum.Systemowe:
                        sukces = true; // Systemowe = zapis do bazy
                        break;

                    case TypPowiadomieniaEnum.Push:
                        sukces = WyslijNatywnyWindowsToast(tytul, tresc, rezerwacjaId);
                        WyzwolToastEvent(uzytkownikId, tytul, tresc);
                        break;
                }

                // Aktualizuj status
                powiadomienie.StatusPowiadomieniaId = sukces
                    ? (int)StatusPowiadomieniaEnum.Wyslane
                    : (int)StatusPowiadomieniaEnum.BladWysylki;

                powiadomienie.DataWyslania = sukces ? DateTime.Now : null;

                // Zapisz do bazy
                await _powiadomienieRepo.AddAsync(powiadomienie);

                return sukces;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wysyłania powiadomienia: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Wysyła powiadomienie wszystkimi kanałami jednocześnie
        /// </summary>
        public async Task WyslijPowiadomienieWszystkimiKanalamiAsync(
            int uzytkownikId,
            string tytul,
            string tresc,
            int? rezerwacjaId = null)
        {
            // Email
            await WyslijPowiadomienieAsync(uzytkownikId, tytul, tresc, TypPowiadomieniaEnum.Email, rezerwacjaId);

            // Systemowe (w aplikacji)
            await WyslijPowiadomienieAsync(uzytkownikId, tytul, tresc, TypPowiadomieniaEnum.Systemowe, rezerwacjaId);

            // Natywny Windows Toast
            await WyslijPowiadomienieAsync(uzytkownikId, tytul, tresc, TypPowiadomieniaEnum.Push, rezerwacjaId);
        }

        /// <summary>
        /// Pobiera nieprzeczytane powiadomienia (systemowe)
        /// </summary>
        public async Task<IEnumerable<Powiadomienie>> GetNieprzeczytanePowiadomieniaAsync(int uzytkownikId)
        {
            var wszystkie = await _powiadomienieRepo.FindAsync(p =>
                p.UzytkownikId == uzytkownikId &&
                p.TypPowiadomieniaId == (int)TypPowiadomieniaEnum.Systemowe &&
                p.StatusPowiadomieniaId != (int)StatusPowiadomieniaEnum.Przeczytane);

            return wszystkie.OrderByDescending(p => p.DataUtworzenia);
        }

        /// <summary>
        /// Oznacza powiadomienie jako przeczytane
        /// </summary>
        public async Task<bool> OznaczJakoPrzeczytaneAsync(int powiadomienieId)
        {
            try
            {
                var powiadomienie = await _powiadomienieRepo.GetByIdAsync(powiadomienieId);
                if (powiadomienie == null)
                    return false;

                powiadomienie.StatusPowiadomieniaId = (int)StatusPowiadomieniaEnum.Przeczytane;
                await _powiadomienieRepo.UpdateAsync(powiadomienie);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Oznacza wszystkie powiadomienia użytkownika jako przeczytane
        /// </summary>
        public async Task OznaczWszystkieJakoPrzeczytaneAsync(int uzytkownikId)
        {
            var nieprzeczytane = await GetNieprzeczytanePowiadomieniaAsync(uzytkownikId);

            foreach (var p in nieprzeczytane)
            {
                p.StatusPowiadomieniaId = (int)StatusPowiadomieniaEnum.Przeczytane;
                await _powiadomienieRepo.UpdateAsync(p);
            }
        }

        /// <summary>
        /// Zwraca liczbę nieprzeczytanych powiadomień
        /// </summary>
        public async Task<int> GetLiczbaNieprzeczytanychAsync(int uzytkownikId)
        {
            var nieprzeczytane = await GetNieprzeczytanePowiadomieniaAsync(uzytkownikId);
            return nieprzeczytane.Count();
        }

        /// <summary>
        /// Usuwa powiadomienie
        /// </summary>
        public async Task<bool> UsunPowiadomienieAsync(int powiadomienieId)
        {
            try
            {
                await _powiadomienieRepo.DeleteAsync(powiadomienieId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Prywatne metody obsługi kanałów

        /// <summary>
        /// Wysyła natywne powiadomienie Windows Toast
        /// </summary>
        private bool WyslijNatywnyWindowsToast(string tytul, string tresc, int? rezerwacjaId = null)
        {
            try
            {
                // Użyj WindowsToastService z warstwy WPF (jeśli dostępny)
                // lub po prostu wyzwól event który zostanie obsłużony przez UI
                System.Diagnostics.Debug.WriteLine($"[WINDOWS TOAST] {tytul}: {tresc}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wysyłki Windows Toast: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Symulacja wysyłki email (w produkcji: SMTP)
        /// </summary>
        private async Task<bool> WyslijEmailAsync(int uzytkownikId, string tytul, string tresc)
        {
            try
            {
                // Pobierz email użytkownika
                string emailOdbiorcy = "unknown@srkt.pl";
                if (_uzytkownikRepo != null)
                {
                    var uzytkownik = await _uzytkownikRepo.GetByIdAsync(uzytkownikId);
                    if (uzytkownik != null)
                        emailOdbiorcy = uzytkownik.Email;
                }

                // Symulacja wysyłki - logowanie do pliku
                var logEntry = $"""
                    ========================================
                    DATA WYSYŁKI: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                    DO: {emailOdbiorcy}
                    TEMAT: {tytul}
                    ----------------------------------------
                    {tresc}
                    ========================================

                    """;

                // Zapisz do logu (symulacja)
                System.Diagnostics.Debug.WriteLine($"[EMAIL] Do: {emailOdbiorcy}, Temat: {tytul}");

                // Opcjonalnie zapisz do pliku
                try
                {
                    var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SRKT", "email_log.txt");
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                    await File.AppendAllTextAsync(logPath, logEntry);
                }
                catch
                {
                    // Ignoruj błędy zapisu logu
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wysyłki email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Wyzwala event Toast dla aplikacji (opcjonalne)
        /// </summary>
        private void WyzwolToastEvent(int uzytkownikId, string tytul, string tresc)
        {
            try
            {
                NowePowiadomienieToast?.Invoke(this, new PowiadomienieEventArgs
                {
                    UzytkownikId = uzytkownikId,
                    Tytul = tytul,
                    Tresc = tresc,
                    Typ = TypPowiadomieniaEnum.Push
                });
            }
            catch
            {
                // Ignoruj błędy eventu
            }
        }

        #endregion
    }
}