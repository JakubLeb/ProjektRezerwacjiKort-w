using SRKT.Business.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Serwis działający w tle, sprawdzający przypomnienia do wysłania
    /// </summary>
    public class PrzypomnienieBackgroundService : IDisposable
    {
        private readonly IPrzypomnienieService _przypomnienieService;
        private readonly IPowiadomienieService _powiadomienieService;
        private readonly int _uzytkownikId;

        private Timer _timer;
        private bool _isRunning;
        private readonly int _intervalSeconds = 30; // Sprawdzaj co 30 sekund

        public PrzypomnienieBackgroundService(
            IPrzypomnienieService przypomnienieService,
            IPowiadomienieService powiadomienieService,
            int uzytkownikId)
        {
            _przypomnienieService = przypomnienieService;
            _powiadomienieService = powiadomienieService;
            _uzytkownikId = uzytkownikId;
        }

        /// <summary>
        /// Uruchamia serwis sprawdzania przypomnień
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _timer = new Timer(async _ => await SprawdzPrzypomnieniAsync(),
                              null,
                              TimeSpan.Zero,
                              TimeSpan.FromSeconds(_intervalSeconds));

            System.Diagnostics.Debug.WriteLine($"[PrzypomnienieBackgroundService] Uruchomiono dla użytkownika {_uzytkownikId}");
        }

        /// <summary>
        /// Zatrzymuje serwis
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;

            System.Diagnostics.Debug.WriteLine("[PrzypomnienieBackgroundService] Zatrzymano");
        }

        private async Task SprawdzPrzypomnieniAsync()
        {
            try
            {
                // Pobierz przypomnienia do wysłania
                var przypomnienia = await _przypomnienieService.GetPrzypomnieniaDowyslaniaAsync();

                foreach (var przypomnienie in przypomnienia)
                {
                    // Sprawdź czy przypomnienie dotyczy aktualnego użytkownika
                    if (przypomnienie.UzytkownikId != _uzytkownikId)
                        continue;

                    // Wyświetl Toast
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ToastManager.Instance.ShowInfo(przypomnienie.Tytul, przypomnienie.Tresc);
                    });

                    // Wyślij powiadomienie systemowe
                    await _powiadomienieService.WyslijPowiadomienieAsync(
                        przypomnienie.UzytkownikId,
                        przypomnienie.Tytul,
                        przypomnienie.Tresc,
                        TypPowiadomieniaEnum.Systemowe,
                        przypomnienie.RezerwacjaId);

                    // Oznacz jako wysłane
                    await _przypomnienieService.OznaczJakoWyslaneAsync(przypomnienie.Id);

                    System.Diagnostics.Debug.WriteLine($"[PrzypomnienieBackgroundService] Wysłano przypomnienie: {przypomnienie.Tytul}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PrzypomnienieBackgroundService] Błąd: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}