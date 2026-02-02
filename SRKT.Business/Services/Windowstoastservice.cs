using System;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Serwis do wyświetlania natywnych powiadomień Windows (Toast Notifications)
    /// </summary>
    public class WindowsToastService
    {
        private static WindowsToastService _instance;
        private static readonly object _lock = new object();
        private readonly string _appId;

        public static WindowsToastService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new WindowsToastService();
                        }
                    }
                }
                return _instance;
            }
        }

        private WindowsToastService()
        {
            // Użyj nazwy aplikacji jako AppId
            _appId = System.Windows.Application.Current?.MainWindow?.Title ?? "SRKT";
        }

        /// <summary>
        /// Wyświetla proste powiadomienie Toast
        /// </summary>
        public void ShowToast(string title, string message)
        {
            try
            {
                ShowToastNotification(title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie Toast z ikoną informacji
        /// </summary>
        public void ShowInfo(string title, string message)
        {
            try
            {
                ShowToastNotification("ℹ️ " + title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast Info: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie Toast z ikoną sukcesu
        /// </summary>
        public void ShowSuccess(string title, string message)
        {
            try
            {
                ShowToastNotification("✅ " + title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast Success: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie Toast z ikoną ostrzeżenia
        /// </summary>
        public void ShowWarning(string title, string message)
        {
            try
            {
                ShowToastNotification("⚠️ " + title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast Warning: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie Toast z ikoną błędu
        /// </summary>
        public void ShowError(string title, string message)
        {
            try
            {
                ShowToastNotification("❌ " + title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast Error: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie o przypomnieniu rezerwacji
        /// </summary>
        public void ShowReminderToast(string title, string message, DateTime reminderTime, int? reservationId = null)
        {
            try
            {
                var fullMessage = $"{message}\nCzas: {reminderTime:HH:mm dd.MM.yyyy}";
                ShowToastNotification("⏰ " + title, fullMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast Reminder: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie Toast z przyciskami akcji
        /// </summary>
        public void ShowToastWithActions(string title, string message, params (string text, string actionId)[] buttons)
        {
            try
            {
                var toastXml = CreateToastXmlWithActions(title, message, buttons);
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier(_appId).Show(toast);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast z akcjami: {ex.Message}");
                ShowFallbackNotification(title, message);
            }
        }

        /// <summary>
        /// Zaplanowane powiadomienie Toast (wyświetli się o określonej godzinie)
        /// </summary>
        public void ScheduleToast(string title, string message, DateTime scheduledTime, string tag = null)
        {
            try
            {
                if (scheduledTime <= DateTime.Now)
                {
                    ShowToast(title, message);
                    return;
                }

                var toastXml = CreateToastXml("⏰ " + title, $"{message}\nZaplanowano na: {scheduledTime:HH:mm}");
                var scheduledToast = new ScheduledToastNotification(toastXml, scheduledTime);

                if (!string.IsNullOrEmpty(tag))
                {
                    scheduledToast.Tag = tag;
                }

                ToastNotificationManager.CreateToastNotifier(_appId).AddToSchedule(scheduledToast);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd planowania Toast: {ex.Message}");
            }
        }

        /// <summary>
        /// Wyczyść wszystkie powiadomienia aplikacji
        /// </summary>
        public void ClearAllToasts()
        {
            try
            {
                ToastNotificationManager.History.Clear(_appId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd czyszczenia Toast: {ex.Message}");
            }
        }

        private void ShowToastNotification(string title, string message)
        {
            var toastXml = CreateToastXml(title, message);
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier(_appId).Show(toast);
        }

        private XmlDocument CreateToastXml(string title, string message)
        {
            var toastXmlString = $@"
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{EscapeXml(title)}</text>
                            <text>{EscapeXml(message)}</text>
                        </binding>
                    </visual>
                </toast>";

            var toastXml = new XmlDocument();
            toastXml.LoadXml(toastXmlString);
            return toastXml;
        }

        private XmlDocument CreateToastXmlWithActions(string title, string message, (string text, string actionId)[] buttons)
        {
            var actionsXml = string.Empty;
            foreach (var (text, actionId) in buttons)
            {
                actionsXml += $@"<action content='{EscapeXml(text)}' arguments='{EscapeXml(actionId)}' />";
            }

            var toastXmlString = $@"
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{EscapeXml(title)}</text>
                            <text>{EscapeXml(message)}</text>
                        </binding>
                    </visual>
                    <actions>
                        {actionsXml}
                    </actions>
                </toast>";

            var toastXml = new XmlDocument();
            toastXml.LoadXml(toastXmlString);
            return toastXml;
        }

        private string EscapeXml(string text)
        {
            return text?
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;") ?? string.Empty;
        }

        /// <summary>
        /// Fallback - wyświetla powiadomienie jako MessageBox jeśli Toast nie działa
        /// </summary>
        private void ShowFallbackNotification(string title, string message)
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                });
            }
            catch
            {
                // Ignoruj błędy fallbacka
            }
        }
    }
}