using SRKT.Business.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Serwis działający w tle, który sprawdza i wysyła zaległe przypomnienia
    /// </summary>
    public class ReminderBackgroundService : IDisposable
    {
        private readonly IPrzypomnienieService _przypomnienieService;
        private Timer _timer;
        private bool _isRunning;
        private readonly object _lock = new object();

        // Interwał sprawdzania (domyślnie co 1 minutę)
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public ReminderBackgroundService(IPrzypomnienieService przypomnienieService)
        {
            _przypomnienieService = przypomnienieService ?? throw new ArgumentNullException(nameof(przypomnienieService));
        }

        /// <summary>
        /// Uruchamia serwis sprawdzania przypomnień
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning) return;

                _timer = new Timer(
                    callback: async _ => await SprawdzPrzypomnieniAsync(),
                    state: null,
                    dueTime: TimeSpan.Zero, // Uruchom natychmiast
                    period: _checkInterval);

                _isRunning = true;
                System.Diagnostics.Debug.WriteLine("ReminderBackgroundService: Uruchomiono serwis sprawdzania przypomnień.");
            }
        }

        /// <summary>
        /// Zatrzymuje serwis
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning) return;

                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();
                _timer = null;
                _isRunning = false;

                System.Diagnostics.Debug.WriteLine("ReminderBackgroundService: Zatrzymano serwis sprawdzania przypomnień.");
            }
        }

        /// <summary>
        /// Sprawdza i wysyła zaległe przypomnienia
        /// </summary>
        private async Task SprawdzPrzypomnieniAsync()
        {
            try
            {
                if (_przypomnienieService == null) return;

                await _przypomnienieService.SprawdzIWyslijPrzypomnieniAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReminderBackgroundService: Błąd sprawdzania przypomnień: {ex.Message}");
            }
        }

        /// <summary>
        /// Wymusza natychmiastowe sprawdzenie przypomnień
        /// </summary>
        public async Task ForceCheckAsync()
        {
            await SprawdzPrzypomnieniAsync();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}