using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Typ powiadomienia In-App Toast
    /// </summary>
    public enum InAppToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Serwis do wyświetlania powiadomień Toast wewnątrz aplikacji
    /// </summary>
    public class InAppToastService
    {
        private static InAppToastService _instance;
        private static readonly object _lock = new object();

        private StackPanel _container;
        private const int MaxVisibleToasts = 5;
        private const double DefaultDurationSeconds = 5.0;

        public static InAppToastService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new InAppToastService();
                    }
                }
                return _instance;
            }
        }

        private InAppToastService() { }

        /// <summary>
        /// Inicjalizuje serwis z kontenerem z MainWindow
        /// </summary>
        public void Initialize(StackPanel container)
        {
            _container = container;
        }

        /// <summary>
        /// Wyświetla powiadomienie informacyjne
        /// </summary>
        public void ShowInfo(string title, string message, double durationSeconds = DefaultDurationSeconds)
        {
            Show(title, message, InAppToastType.Info, durationSeconds);
        }

        /// <summary>
        /// Wyświetla powiadomienie o sukcesie
        /// </summary>
        public void ShowSuccess(string title, string message, double durationSeconds = DefaultDurationSeconds)
        {
            Show(title, message, InAppToastType.Success, durationSeconds);
        }

        /// <summary>
        /// Wyświetla powiadomienie ostrzegawcze
        /// </summary>
        public void ShowWarning(string title, string message, double durationSeconds = DefaultDurationSeconds)
        {
            Show(title, message, InAppToastType.Warning, durationSeconds);
        }

        /// <summary>
        /// Wyświetla powiadomienie o błędzie
        /// </summary>
        public void ShowError(string title, string message, double durationSeconds = DefaultDurationSeconds)
        {
            Show(title, message, InAppToastType.Error, durationSeconds);
        }

        /// <summary>
        /// Główna metoda wyświetlająca Toast
        /// </summary>
        public void Show(string title, string message, InAppToastType type, double durationSeconds = DefaultDurationSeconds)
        {
            if (_container == null)
            {
                System.Diagnostics.Debug.WriteLine("InAppToastService: Kontener nie został zainicjalizowany!");
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Ogranicz liczbę widocznych toastów
                while (_container.Children.Count >= MaxVisibleToasts)
                {
                    _container.Children.RemoveAt(0);
                }

                var toast = CreateToastElement(title, message, type);
                _container.Children.Add(toast);

                // Animacja wejścia
                AnimateIn(toast);

                // Timer do automatycznego usunięcia
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(durationSeconds)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    AnimateOut(toast, () =>
                    {
                        if (_container.Children.Contains(toast))
                        {
                            _container.Children.Remove(toast);
                        }
                    });
                };
                timer.Start();
            });
        }

        /// <summary>
        /// Tworzy element Toast
        /// </summary>
        private Border CreateToastElement(string title, string message, InAppToastType type)
        {
            // Kolory w zależności od typu
            var (backgroundColor, iconColor, icon) = GetTypeStyles(type);

            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10),
                Opacity = 0,
                RenderTransform = new TranslateTransform(50, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 15,
                    ShadowDepth = 3,
                    Opacity = 0.2,
                    Color = Colors.Black
                }
            };

            // Pasek kolorowy z lewej strony
            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Kolorowy pasek
            var colorBar = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconColor)),
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, 0, 10, 0)
            };
            Grid.SetColumn(colorBar, 0);
            mainGrid.Children.Add(colorBar);

            // Ikona
            var iconBorder = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor)),
                Margin = new Thickness(5, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.Child = iconText;
            Grid.SetColumn(iconBorder, 1);
            mainGrid.Children.Add(iconBorder);

            // Treść
            var contentStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                TextWrapping = TextWrapping.Wrap
            };
            contentStack.Children.Add(titleBlock);

            if (!string.IsNullOrWhiteSpace(message))
            {
                var messageBlock = new TextBlock
                {
                    Text = message,
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 3, 0, 0)
                };
                contentStack.Children.Add(messageBlock);
            }

            // Czas
            var timeBlock = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                Margin = new Thickness(0, 5, 0, 0)
            };
            contentStack.Children.Add(timeBlock);

            Grid.SetColumn(contentStack, 2);
            mainGrid.Children.Add(contentStack);

            // Przycisk zamknięcia
            var closeButton = new Button
            {
                Content = "✕",
                Width = 24,
                Height = 24,
                FontSize = 12,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 0, 0, 0)
            };
            closeButton.Click += (s, e) =>
            {
                AnimateOut(border, () =>
                {
                    if (_container.Children.Contains(border))
                    {
                        _container.Children.Remove(border);
                    }
                });
            };
            Grid.SetColumn(closeButton, 3);
            mainGrid.Children.Add(closeButton);

            border.Child = mainGrid;
            return border;
        }

        /// <summary>
        /// Zwraca style dla danego typu powiadomienia
        /// </summary>
        private (string backgroundColor, string iconColor, string icon) GetTypeStyles(InAppToastType type)
        {
            return type switch
            {
                InAppToastType.Success => ("#E8F8F5", "#27AE60", "✓"),
                InAppToastType.Warning => ("#FEF9E7", "#F39C12", "⚠"),
                InAppToastType.Error => ("#FDEDEC", "#E74C3C", "✕"),
                _ => ("#EBF5FB", "#3498DB", "ℹ") // Info
            };
        }

        /// <summary>
        /// Animacja wejścia
        /// </summary>
        private void AnimateIn(Border toast)
        {
            var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var translateAnimation = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            toast.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            ((TranslateTransform)toast.RenderTransform).BeginAnimation(TranslateTransform.XProperty, translateAnimation);
        }

        /// <summary>
        /// Animacja wyjścia
        /// </summary>
        private void AnimateOut(Border toast, Action onCompleted)
        {
            var opacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var translateAnimation = new DoubleAnimation(0, 50, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            opacityAnimation.Completed += (s, e) => onCompleted?.Invoke();

            toast.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            ((TranslateTransform)toast.RenderTransform).BeginAnimation(TranslateTransform.XProperty, translateAnimation);
        }

        /// <summary>
        /// Czyści wszystkie widoczne powiadomienia
        /// </summary>
        public void ClearAll()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _container?.Children.Clear();
            });
        }
    }
}