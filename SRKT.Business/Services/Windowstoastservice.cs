using CommunityToolkit.WinUI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Serwis do wyświetlania natywnych powiadomień Windows (Toast Notifications)
    /// </summary>
    public class WindowsToastService
    {
        private static WindowsToastService _instance;
        private static readonly object _lock = new object();

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
            // Prywatny konstruktor dla singletona
        }

        /// <summary>
        /// Wyświetla proste powiadomienie Toast
        /// </summary>
        public void ShowToast(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast: {ex.Message}");
                // Fallback - użyj MessageBox jeśli Toast nie działa
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
                new ToastContentBuilder()
                    .AddText("ℹ️ " + title)
                    .AddText(message)
                    .Show();
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
                new ToastContentBuilder()
                    .AddText("✅ " + title)
                    .AddText(message)
                    .Show();
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
                new ToastContentBuilder()
                    .AddText("⚠️ " + title)
                    .AddText(message)
                    .Show();
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
                new ToastContentBuilder()
                    .AddText("❌ " + title)
                    .AddText(message)
                    .Show();
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
                var builder = new ToastContentBuilder()
                    .AddText("⏰ " + title)
                    .AddText(message)
                    .AddText($"Czas: {reminderTime:HH:mm dd.MM.yyyy}");

                builder.Show();
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
                var builder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message);

                foreach (var (text, actionId) in buttons)
                {
                    builder.AddButton(new ToastButton()
                        .SetContent(text)
                        .AddArgument("action", actionId));
                }

                builder.Show();
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
                    // Jeśli czas już minął, pokaż natychmiast
                    ShowToast(title, message);
                    return;
                }

                var builder = new ToastContentBuilder()
                    .AddText("⏰ " + title)
                    .AddText(message)
                    .AddText($"Zaplanowano na: {scheduledTime:HH:mm}");

                // Zaplanuj powiadomienie
                builder.Schedule(scheduledTime);
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
                ToastNotificationManagerCompat.History.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd czyszczenia Toast: {ex.Message}");
            }
        }

        /// <summary>
        /// Fallback - wyświetla powiadomienie jako MessageBox jeśli Toast nie działa
        /// </summary>
        private void ShowFallbackNotification(string title, string message)
        {
            try
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch
            {
                // Ignoruj błędy fallbacka
            }
        }
    }
}