using SRKT.WPF.Controls;
using System.Windows;
using System.Windows.Controls;

namespace SRKT.WPF.Services
{
    /// <summary>
    /// Singleton menedżer do wyświetlania Toast notifications
    /// Obsługuje kolejkowanie wielu powiadomień
    /// </summary>
    public class ToastManager
    {
        private static ToastManager _instance;
        private static readonly object _lock = new object();

        private Panel _container;
        private readonly Queue<(string Tytul, string Tresc, ToastType Typ)> _queue = new();
        private ToastNotification _currentToast;
        private const int MAX_VISIBLE = 3;
        private readonly List<ToastNotification> _visibleToasts = new();

        public static ToastManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ToastManager();
                    }
                }
                return _instance;
            }
        }

        private ToastManager() { }

        /// <summary>
        /// Inicjalizuje menedżer z kontenerem gdzie będą wyświetlane Toasty
        /// Kontener powinien być Grid lub StackPanel w głównym oknie
        /// </summary>
        public void Initialize(Panel container)
        {
            _container = container;
        }

        /// <summary>
        /// Wyświetla Toast notification
        /// </summary>
        public void ShowToast(string tytul, string tresc, ToastType typ = ToastType.Info)
        {
            if (_container == null)
            {
                System.Diagnostics.Debug.WriteLine("ToastManager: Kontener nie został zainicjalizowany!");
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Jeśli za dużo widocznych, dodaj do kolejki
                if (_visibleToasts.Count >= MAX_VISIBLE)
                {
                    _queue.Enqueue((tytul, tresc, typ));
                    return;
                }

                ShowToastInternal(tytul, tresc, typ);
            });
        }

        /// <summary>
        /// Skrót do wyświetlenia informacyjnego Toasta
        /// </summary>
        public void ShowInfo(string tytul, string tresc) => ShowToast(tytul, tresc, ToastType.Info);

        /// <summary>
        /// Skrót do wyświetlenia Toasta sukcesu
        /// </summary>
        public void ShowSuccess(string tytul, string tresc) => ShowToast(tytul, tresc, ToastType.Success);

        /// <summary>
        /// Skrót do wyświetlenia Toasta ostrzeżenia
        /// </summary>
        public void ShowWarning(string tytul, string tresc) => ShowToast(tytul, tresc, ToastType.Warning);

        /// <summary>
        /// Skrót do wyświetlenia Toasta błędu
        /// </summary>
        public void ShowError(string tytul, string tresc) => ShowToast(tytul, tresc, ToastType.Error);

        private void ShowToastInternal(string tytul, string tresc, ToastType typ)
        {
            var toast = new ToastNotification();
            
            // Pozycjonowanie - każdy kolejny niżej
            int index = _visibleToasts.Count;
            toast.Margin = new Thickness(0, 10 + (index * 100), 10, 0);
            toast.HorizontalAlignment = HorizontalAlignment.Right;
            toast.VerticalAlignment = VerticalAlignment.Top;

            toast.Closed += (s, e) =>
            {
                _container.Children.Remove(toast);
                _visibleToasts.Remove(toast);
                
                // Przepozycjonuj pozostałe
                RepositionToasts();

                // Pokaż następny z kolejki
                if (_queue.Count > 0)
                {
                    var next = _queue.Dequeue();
                    ShowToastInternal(next.Tytul, next.Tresc, next.Typ);
                }
            };

            _visibleToasts.Add(toast);
            _container.Children.Add(toast);
            toast.Show(tytul, tresc, typ);
        }

        private void RepositionToasts()
        {
            for (int i = 0; i < _visibleToasts.Count; i++)
            {
                var toast = _visibleToasts[i];
                toast.Margin = new Thickness(0, 10 + (i * 100), 10, 0);
            }
        }

        /// <summary>
        /// Zamyka wszystkie widoczne Toasty
        /// </summary>
        public void ClearAll()
        {
            _queue.Clear();
            var toastsToClose = _visibleToasts.ToList();
            foreach (var toast in toastsToClose)
            {
                toast.Hide();
            }
        }
    }
}
