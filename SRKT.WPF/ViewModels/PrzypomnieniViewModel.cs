using SRKT.Business.Services;
using SRKT.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class PrzypomnieniViewModel : BaseViewModel
    {
        private readonly IPrzypomnienieService _przypomnienieService;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly Uzytkownik _uzytkownik;

        private ObservableCollection<Przypomnienie> _przypomnienia;
        private ObservableCollection<Przypomnienie> _przypomnieniFiltrowane;
        private int _liczbaAktywnych;

        // Filtry
        private bool _filtrWszystkie = true;
        private bool _filtrAktywne;
        private bool _filtrWyslane;

        public PrzypomnieniViewModel(
            IPrzypomnienieService przypomnienieService, 
            IRezerwacjaService rezerwacjaService,
            Uzytkownik uzytkownik)
        {
            _przypomnienieService = przypomnienieService;
            _rezerwacjaService = rezerwacjaService;
            _uzytkownik = uzytkownik;

            Przypomnienia = new ObservableCollection<Przypomnienie>();
            PrzypomnieniFiltrowane = new ObservableCollection<Przypomnienie>();

            // Komendy
            OdswiezCommand = new RelayCommand(async _ => await ZaladujPrzypomnieniAsync());
            DodajPrzypomnienieCommand = new RelayCommand(async _ => await DodajPrzypomnienieAsync());
            EdytujPrzypomnienieCommand = new RelayCommand(async param => await EdytujPrzypomnienieAsync(param as Przypomnienie));
            AnulujPrzypomnienieCommand = new RelayCommand(async param => await AnulujPrzypomnienieAsync(param as Przypomnienie));
            UsunPrzypomnienieCommand = new RelayCommand(async param => await UsunPrzypomnienieAsync(param as Przypomnienie));

            // Załaduj dane
            _ = ZaladujPrzypomnieniAsync();
        }

        #region Właściwości

        public ObservableCollection<Przypomnienie> Przypomnienia
        {
            get => _przypomnienia;
            set => SetProperty(ref _przypomnienia, value);
        }

        public ObservableCollection<Przypomnienie> PrzypomnieniFiltrowane
        {
            get => _przypomnieniFiltrowane;
            set => SetProperty(ref _przypomnieniFiltrowane, value);
        }

        public int LiczbaAktywnych
        {
            get => _liczbaAktywnych;
            set => SetProperty(ref _liczbaAktywnych, value);
        }

        public bool BrakPrzypomnieŋ => PrzypomnieniFiltrowane?.Count == 0;

        // Filtry
        public bool FiltrWszystkie
        {
            get => _filtrWszystkie;
            set { if (SetProperty(ref _filtrWszystkie, value) && value) FiltrujPrzypomnienia(); }
        }

        public bool FiltrAktywne
        {
            get => _filtrAktywne;
            set { if (SetProperty(ref _filtrAktywne, value) && value) FiltrujPrzypomnienia(); }
        }

        public bool FiltrWyslane
        {
            get => _filtrWyslane;
            set { if (SetProperty(ref _filtrWyslane, value) && value) FiltrujPrzypomnienia(); }
        }

        #endregion

        #region Komendy

        public ICommand OdswiezCommand { get; }
        public ICommand DodajPrzypomnienieCommand { get; }
        public ICommand EdytujPrzypomnienieCommand { get; }
        public ICommand AnulujPrzypomnienieCommand { get; }
        public ICommand UsunPrzypomnienieCommand { get; }

        #endregion

        #region Metody

        private async Task ZaladujPrzypomnieniAsync()
        {
            try
            {
                var przypomnienia = await _przypomnienieService.GetPrzypomnieniUzytkownikaAsync(_uzytkownik.Id);

                Przypomnienia.Clear();
                foreach (var p in przypomnienia)
                {
                    Przypomnienia.Add(p);
                }

                // Policz aktywne
                LiczbaAktywnych = Przypomnienia.Count(p => p.CzyAktywne && !p.CzyWyslane);

                FiltrujPrzypomnienia();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania przypomnień: {ex.Message}");
            }
        }

        private void FiltrujPrzypomnienia()
        {
            IEnumerable<Przypomnienie> przefiltrowane = Przypomnienia;

            if (FiltrAktywne)
            {
                przefiltrowane = przefiltrowane.Where(p => p.CzyAktywne && !p.CzyWyslane);
            }
            else if (FiltrWyslane)
            {
                przefiltrowane = przefiltrowane.Where(p => p.CzyWyslane);
            }

            PrzypomnieniFiltrowane.Clear();
            foreach (var p in przefiltrowane.OrderBy(x => x.DataPrzypomnienia))
            {
                PrzypomnieniFiltrowane.Add(p);
            }

            OnPropertyChanged(nameof(BrakPrzypomnieŋ));
        }

        private async Task DodajPrzypomnienieAsync()
        {
            // Pobierz rezerwacje użytkownika do wyboru
            var rezerwacje = await _rezerwacjaService.GetRezerwacjeUzytkownikaAsync(_uzytkownik.Id);
            var przyszleRezerwacje = rezerwacje.Where(r => 
                r.DataRezerwacji > DateTime.Now && 
                r.StatusRezerwacjiId != 3).ToList();

            if (!przyszleRezerwacje.Any())
            {
                MessageBox.Show(
                    "Nie masz żadnych nadchodzących rezerwacji.\nNajpierw zarezerwuj kort.",
                    "Brak rezerwacji",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Otwórz okno dodawania przypomnienia
            var dialog = new DodajPrzypomnienieWindow(przyszleRezerwacje, _przypomnienieService);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                await ZaladujPrzypomnieniAsync();
                MessageBox.Show("Przypomnienie zostało dodane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task EdytujPrzypomnienieAsync(Przypomnienie przypomnienie)
        {
            if (przypomnienie == null || !przypomnienie.MoznaEdytowac) return;

            var dialog = new EdytujPrzypomnienieWindow(przypomnienie, _przypomnienieService);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                await ZaladujPrzypomnieniAsync();
                MessageBox.Show("Przypomnienie zostało zaktualizowane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task AnulujPrzypomnienieAsync(Przypomnienie przypomnienie)
        {
            if (przypomnienie == null) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz anulować przypomnienie?\n\n{przypomnienie.Tytul}",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var sukces = await _przypomnienieService.AnulujPrzypomnienieAsync(przypomnienie.Id);
                if (sukces)
                {
                    await ZaladujPrzypomnieniAsync();
                }
            }
        }

        private async Task UsunPrzypomnienieAsync(Przypomnienie przypomnienie)
        {
            if (przypomnienie == null) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz usunąć przypomnienie?\n\n{przypomnienie.Tytul}",
                "Potwierdzenie usunięcia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var sukces = await _przypomnienieService.UsunPrzypomnienieAsync(przypomnienie.Id);
                if (sukces)
                {
                    await ZaladujPrzypomnieniAsync();
                }
            }
        }

        #endregion
    }
}
