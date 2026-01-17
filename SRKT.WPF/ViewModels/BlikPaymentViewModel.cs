using SRKT.Business.Services;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class BlikPaymentViewModel : BaseViewModel
    {
        private readonly IPlatnoscService _platnoscService;
        private readonly int _rezerwacjaId;
        private readonly Action<bool> _onClose;

        private string _kodBlik;
        private string _komunikat;
        private bool _czyPrzetwarzanie;
        private decimal _kwota;

        public BlikPaymentViewModel(
            IPlatnoscService platnoscService,
            int rezerwacjaId,
            decimal kwota,
            Action<bool> onClose)
        {
            _platnoscService = platnoscService;
            _rezerwacjaId = rezerwacjaId;
            _kwota = kwota;
            _onClose = onClose;

            ZaplacCommand = new RelayCommand(async _ => await ZaplacAsync(), _ => MozeZaplacic());
            AnulujCommand = new RelayCommand(_ => Anuluj());
        }

        public string KodBlik
        {
            get => _kodBlik;
            set
            {
                if (SetProperty(ref _kodBlik, value))
                {
                    Komunikat = string.Empty;
                }
            }
        }

        public string Komunikat
        {
            get => _komunikat;
            set => SetProperty(ref _komunikat, value);
        }

        public bool CzyPrzetwarzanie
        {
            get => _czyPrzetwarzanie;
            set
            {
                if (SetProperty(ref _czyPrzetwarzanie, value))
                {
                    // Powiadom o zmianie widoczności
                    OnPropertyChanged(nameof(WidocznoscFormularza));
                    OnPropertyChanged(nameof(WidocznoscPrzetwarzania));
                }
            }
        }

        public decimal Kwota
        {
            get => _kwota;
            set => SetProperty(ref _kwota, value);
        }

        // Właściwości dla Visibility (zamiast konwertera)
        public Visibility WidocznoscFormularza => CzyPrzetwarzanie ? Visibility.Collapsed : Visibility.Visible;
        public Visibility WidocznoscPrzetwarzania => CzyPrzetwarzanie ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ZaplacCommand { get; }
        public ICommand AnulujCommand { get; }

        private bool MozeZaplacic()
        {
            return !CzyPrzetwarzanie &&
                   !string.IsNullOrWhiteSpace(KodBlik) &&
                   KodBlik.Length == 6 &&
                   KodBlik.All(char.IsDigit);
        }

        private async Task ZaplacAsync()
        {
            // Walidacja
            if (string.IsNullOrWhiteSpace(KodBlik) || KodBlik.Length != 6 || !KodBlik.All(char.IsDigit))
            {
                Komunikat = "Kod BLIK musi składać się z 6 cyfr.";
                return;
            }

            try
            {
                CzyPrzetwarzanie = true;
                Komunikat = string.Empty;

                // Wywołaj serwis płatności
                var sukces = await _platnoscService.PrzetworzPlatnoscBlikAsync(_rezerwacjaId, KodBlik, Kwota);

                if (sukces)
                {
                    MessageBox.Show(
                        "Płatność została zrealizowana pomyślnie!",
                        "Sukces",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    _onClose(true); // Zamknij z sukcesem
                }
                else
                {
                    Komunikat = "Płatność nie powiodła się. Spróbuj ponownie.";
                }
            }
            catch (Exception ex)
            {
                Komunikat = $"Błąd płatności: {ex.Message}";
            }
            finally
            {
                CzyPrzetwarzanie = false;
            }
        }

        private void Anuluj()
        {
            _onClose(false); // Zamknij bez płatności
        }
    }
}