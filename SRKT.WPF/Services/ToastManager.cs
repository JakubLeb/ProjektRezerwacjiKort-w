using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Singleton do zarządzania powiadomieniami Toast/Pop-up w aplikacji
    /// </summary>
    public class ToastManager
    {
        private static ToastManager _instance;
        private static readonly object _lock = new object();

        private Panel _container;
        private readonly int _maxToasts = 5;
        private readonly double _toastDuration = 5.0; // sekundy

        private ToastManager() { }

        public static ToastManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ToastManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Inicjalizuje ToastManager z kontenerem do wyświetlania powiadomień
        /// </summary>
        public void Initialize(Panel container)
        {
            _container = container;
        }

        /// <summary>
        /// Wyświetla powiadomienie informacyjne (niebieskie)
        /// </summary>
        public void ShowInfo(string title, string message)
        {
            ShowToast(title, message, ToastType.Info);
        }

        /// <summary>
        /// Wyświetla powiadomienie sukcesu (zielone)
        /// </summary>
        public void ShowSuccess(string title, string message)
        {
            ShowToast(title, message, ToastType.Success);
        }

        /// <summary>
        /// Wyświetla powiadomienie ostrzeżenia (pomarańczowe)
        /// </summary>
        public void ShowWarning(string title, string message)
        {
            ShowToast(title, message, ToastType.Warning);
        }

        /// <summary>
        /// Wyświetla powiadomienie błędu (czerwone)
        /// </summary>
        public void ShowError(string title, string message)
        {
            ShowToast(title, message, ToastType.Error);
        }

        private void ShowToast(string title, string message, ToastType type)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_container == null)
                {
                    // Fallback - użyj MessageBox jeśli kontener nie jest ustawiony
                    MessageBox.Show(message, title, MessageBoxButton.OK,
                        type == ToastType.Error ? MessageBoxImage.Error : MessageBoxImage.Information);
                    return;
                }

                // Ogranicz liczbę toastów
                while (_container.Children.Count >= _maxToasts)
                {
                    _container.Children.RemoveAt(0);
                }

                // Utwórz toast
                var toast = CreateToast(title, message, type);
                _container.Children.Add(toast);

                // Auto-ukryj po czasie
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(_toastDuration)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    HideToast(toast);
                };
                timer.Start();
            });
        }

        private Border CreateToast(string title, string message, ToastType type)
        {
            // Kolory w zależności od typu
            var (bgColor, iconText) = type switch
            {
                ToastType.Success => ("#27AE60", "✓"),
                ToastType.Warning => ("#F39C12", "⚠"),
                ToastType.Error => ("#E74C3C", "✕"),
                _ => ("#3498DB", "🔔")
            };

            // Główny kontener
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Width = 320,
                MinHeight = 70,
                HorizontalAlignment = HorizontalAlignment.Right,
                Opacity = 0,
                RenderTransform = new TranslateTransform(50, 0)
            };

            // Efekt cienia
            border.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 3,
                Opacity = 0.3
            };

            // Grid wewnętrzny
            var grid = new Grid { Margin = new Thickness(15) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Ikona
            var iconBorder = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(17),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                VerticalAlignment = VerticalAlignment.Center
            };
            var iconText2 = new TextBlock
            {
                Text = iconText,
                FontSize = 16,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.Child = iconText2;
            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            // Treść
            var contentStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            contentStack.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 40,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 3, 0, 0)
            });
            Grid.SetColumn(contentStack, 1);
            grid.Children.Add(contentStack);

            // Przycisk zamknięcia
            var closeBtn = new Button
            {
                Content = "✕",
                Width = 25,
                Height = 25,
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 12
            };
            closeBtn.Click += (s, e) => HideToast(border);
            Grid.SetColumn(closeBtn, 2);
            grid.Children.Add(closeBtn);

            // Pasek koloru z lewej strony
            var colorBar = new Border
            {
                Width = 4,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(8, 0, 0, 8)
            };

            // Główny grid z paskiem i zawartością
            var mainGrid = new Grid();
            mainGrid.Children.Add(colorBar);
            mainGrid.Children.Add(grid);

            border.Child = mainGrid;

            // Animacja wejścia
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            var slideIn = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            border.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            ((TranslateTransform)border.RenderTransform).BeginAnimation(TranslateTransform.XProperty, slideIn);

            return border;
        }

        private void HideToast(Border toast)
        {
            if (toast == null || !_container.Children.Contains(toast))
                return;

            // Animacja wyjścia
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            var slideOut = new DoubleAnimation(0, 50, TimeSpan.FromMilliseconds(200));

            fadeOut.Completed += (s, e) =>
            {
                if (_container.Children.Contains(toast))
                {
                    _container.Children.Remove(toast);
                }
            };

            toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            ((TranslateTransform)toast.RenderTransform).BeginAnimation(TranslateTransform.XProperty, slideOut);
        }

        private enum ToastType
        {
            Info,
            Success,
            Warning,
            Error
        }
    }
}