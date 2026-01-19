using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SRKT.WPF.Controls
{
    public partial class ToastNotification : UserControl
    {
        private System.Timers.Timer _autoCloseTimer;
        private const int AUTO_CLOSE_MS = 5000; // 5 sekund

        public event EventHandler Closed;

        public ToastNotification()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wyświetla Toast z podanym tytułem i treścią
        /// </summary>
        public void Show(string tytul, string tresc)
        {
            TytulText.Text = tytul;
            TrescText.Text = tresc;
            CzasText.Text = DateTime.Now.ToString("HH:mm");

            // Uruchom animację wjazdu
            var slideIn = (Storyboard)FindResource("SlideIn");
            slideIn.Begin(this);

            // Uruchom timer auto-zamknięcia
            StartAutoCloseTimer();
        }

        /// <summary>
        /// Wyświetla Toast z ikoną określonego typu
        /// </summary>
        public void Show(string tytul, string tresc, ToastType typ)
        {
            Show(tytul, tresc);
            // Możliwość rozszerzenia o różne ikony/kolory w przyszłości
        }

        private void StartAutoCloseTimer()
        {
            _autoCloseTimer?.Stop();
            _autoCloseTimer?.Dispose();

            _autoCloseTimer = new System.Timers.Timer(AUTO_CLOSE_MS);
            _autoCloseTimer.Elapsed += (s, e) =>
            {
                _autoCloseTimer?.Stop();
                Dispatcher.Invoke(() => Hide());
            };
            _autoCloseTimer.AutoReset = false;
            _autoCloseTimer.Start();
        }

        public void Hide()
        {
            _autoCloseTimer?.Stop();

            var slideOut = (Storyboard)FindResource("SlideOut");
            slideOut.Completed += (s, e) =>
            {
                Closed?.Invoke(this, EventArgs.Empty);
            };
            slideOut.Begin(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }

    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
