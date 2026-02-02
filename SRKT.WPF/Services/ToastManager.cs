using System;
using Microsoft.Toolkit.Uwp.Notifications;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Manager powiadomień Toast używający Microsoft.Toolkit.Uwp.Notifications
    /// Wyświetla natywne powiadomienia Windows (prawy dolny róg ekranu)
    /// </summary>
    public class ToastManager
    {
        private static ToastManager _instance;
        private static readonly object _lock = new object();

        public static ToastManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new ToastManager();
                    }
                }
                return _instance;
            }
        }

        private ToastManager()
        {
            // Konstruktor prywatny - singleton
        }

        /// <summary>
        /// Wyświetla powiadomienie informacyjne
        /// </summary>
        public void ShowInfo(string title, string message)
        {
            ShowToast("ℹ️ " + title, message);
        }

        /// <summary>
        /// Wyświetla powiadomienie o sukcesie
        /// </summary>
        public void ShowSuccess(string title, string message)
        {
            ShowToast("✅ " + title, message);
        }

        /// <summary>
        /// Wyświetla powiadomienie ostrzegawcze
        /// </summary>
        public void ShowWarning(string title, string message)
        {
            ShowToast("⚠️ " + title, message);
        }

        /// <summary>
        /// Wyświetla powiadomienie o błędzie
        /// </summary>
        public void ShowError(string title, string message)
        {
            ShowToast("❌ " + title, message);
        }

        /// <summary>
        /// Wyświetla powiadomienie o przypomnieniu
        /// </summary>
        public void ShowReminder(string title, string message, DateTime reminderTime)
        {
            var fullMessage = $"{message}\nCzas: {reminderTime:HH:mm dd.MM.yyyy}";
            ShowToast("⏰ " + title, fullMessage);
        }

        /// <summary>
        /// Wyświetla podstawowe powiadomienie Toast
        /// </summary>
        public void ShowToast(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .SetToastScenario(ToastScenario.Default)
                    .Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast: {ex.Message}");
                // Fallback - MessageBox
                ShowFallback(title, message);
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
                ShowFallback(title, message);
            }
        }

        /// <summary>
        /// Wyświetla powiadomienie Toast z obrazkiem
        /// </summary>
        public void ShowToastWithImage(string title, string message, string imagePath)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message);

                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    builder.AddInlineImage(new Uri(imagePath));
                }

                builder.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania Toast z obrazkiem: {ex.Message}");
                ShowFallback(title, message);
            }
        }

        /// <summary>
        /// Czyści wszystkie powiadomienia aplikacji
        /// </summary>
        public void ClearAll()
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
        /// Fallback - MessageBox gdy Toast nie działa
        /// </summary>
        private void ShowFallback(string title, string message)
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        message,
                        title,
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                });
            }
            catch
            {
                // Ignoruj błędy fallbacka
            }
        }
    }
}