using SRKT.Business.Services;
using System.Windows.Threading;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Serwis działający w tle, sprawdzający przypomnienia co minutę
    /// </summary>
    public class ReminderBackgroundService : IDisposable
    {
        private readonly IPrzypomnienieService _przypomnienieService;
        private readonly DispatcherTimer _timer;
        private bool _isRunning;

        public ReminderBackgroundService(IPrzypomnienieService przypomnienieService)
        {
            _przypomnienieService = przypomnienieService;
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Sprawdzaj co minutę
            };
            _timer.Tick += async (s, e) => await SprawdzPrzypomnienia();
        }

        /// <summary>
        /// Uruchamia serwis w tle
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;
            
            _timer.Start();
            _isRunning = true;
            
            System.Diagnostics.Debug.WriteLine("ReminderBackgroundService uruchomiony.");
            
            // Sprawdź od razu przy starcie
            _ = SprawdzPrzypomnienia();
        }

        /// <summary>
        /// Zatrzymuje serwis
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _isRunning = false;
            System.Diagnostics.Debug.WriteLine("ReminderBackgroundService zatrzymany.");
        }

        private async Task SprawdzPrzypomnienia()
        {
            try
            {
                await _przypomnienieService.SprawdzIWyslijPrzypomnieniAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd sprawdzania przypomnień: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
