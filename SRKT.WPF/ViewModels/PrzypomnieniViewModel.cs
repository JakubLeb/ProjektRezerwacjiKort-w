using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.WPF.Views;
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
        private bool _isLoading;

        // Filtry
        private bool _filtrWszystkie = true;
        private bool _filtrAktywne = false;
        private bool _filtrWyslane = false;
        private bool _filtrUsuniete = false;

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

            OdswiezCommand = new RelayCommand(async _ => await ZaladujPrzypomnieniAsync());
            DodajPrzypomnienieCommand = new RelayCommand(async _ => await DodajPrzypomnienieAsync());
            EdytujPrzypomnienieCommand = new RelayCommand(async param => await EdytujPrzypomnienieAsync(param as Przypomnienie), _ => !IsLoading);
            AnulujPrzypomnienieCommand = new RelayCommand(async param => await AnulujPrzypomnienieAsync(param as Przypomnienie), _ => !IsLoading);
            UsunPrzypomnienieCommand = new RelayCommand(async param => await UsunPrzypomnienieAsync(param as Przypomnienie), _ => !IsLoading);

            // Załaduj dane na starcie
            _ = ZaladujPrzypomnieniAsync();
        }

        #region Właściwości

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

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

        public bool FiltrUsuniete
        {
            get => _filtrUsuniete;
            set { if (SetProperty(ref _filtrUsuniete, value) && value) FiltrujPrzypomnienia(); }
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
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var przypomnienia = await _przypomnienieService.GetPrzypomnieniUzytkownikaAsync(_uzytkownik.Id);

                // Aktualizuj na wątku UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Przypomnienia.Clear();
                    if (przypomnienia != null)
                    {
                        foreach (var p in przypomnienia)
                        {
                            Przypomnienia.Add(p);
                        }
                    }

                    // Policz aktywne
                    LiczbaAktywnych = Przypomnienia.Count(p => p.CzyAktywne && !p.CzyWyslane);

                    FiltrujPrzypomnienia();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania przypomnień: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Błąd ładowania przypomnień: {ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FiltrujPrzypomnienia()
        {
            try
            {
                IEnumerable<Przypomnienie> przefiltrowane = Przypomnienia ?? Enumerable.Empty<Przypomnienie>();

                if (FiltrAktywne)
                {
                    przefiltrowane = przefiltrowane.Where(p => p.CzyAktywne && !p.CzyWyslane);
                }
                else if (FiltrWyslane)
                {
                    przefiltrowane = przefiltrowane.Where(p => p.CzyWyslane);
                }
                else if (FiltrUsuniete)
                {
                    // Filtr dla usuniętych/anulowanych przypomnień (CzyAktywne = false)
                    przefiltrowane = przefiltrowane.Where(p => !p.CzyAktywne);
                }

                PrzypomnieniFiltrowane.Clear();
                foreach (var p in przefiltrowane.OrderBy(x => x.DataPrzypomnienia))
                {
                    PrzypomnieniFiltrowane.Add(p);
                }

                OnPropertyChanged(nameof(BrakPrzypomnieŋ));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd filtrowania: {ex.Message}");
            }
        }

        private async Task DodajPrzypomnienieAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                // Pobierz rezerwacje użytkownika do wyboru
                var rezerwacje = await _rezerwacjaService.GetRezerwacjeUzytkownikaAsync(_uzytkownik.Id);

                if (rezerwacje == null)
                {
                    MessageBox.Show("Nie można pobrać listy rezerwacji.",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

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

                // Bezpieczne ustawienie Owner
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.IsLoaded && mainWindow != dialog)
                {
                    dialog.Owner = mainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    // Odśwież listę po dodaniu
                    await ZaladujPrzypomnieniAsync();
                    MessageBox.Show("Przypomnienie zostało dodane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania przypomnienia: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EdytujPrzypomnienieAsync(Przypomnienie przypomnienie)
        {
            if (przypomnienie == null || !przypomnienie.MoznaEdytowac || IsLoading) return;

            try
            {
                IsLoading = true;

                var dialog = new EdytujPrzypomnienieWindow(przypomnienie, _przypomnienieService);

                // Bezpieczne ustawienie Owner
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.IsLoaded && mainWindow != dialog)
                {
                    dialog.Owner = mainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    // Odśwież listę po edycji
                    await ZaladujPrzypomnieniAsync();
                    MessageBox.Show("Przypomnienie zostało zaktualizowane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas edycji przypomnienia: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AnulujPrzypomnienieAsync(Przypomnienie przypomnienie)
        {
            if (przypomnienie == null || IsLoading) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz anulować przypomnienie?\n\n{przypomnienie.Tytul}",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var sukces = await _przypomnienieService.AnulujPrzypomnienieAsync(przypomnienie.Id);

                    if (sukces)
                    {
                        // Odśwież listę po anulowaniu
                        await ZaladujPrzypomnieniAsync();
                        MessageBox.Show("Przypomnienie zostało anulowane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się anulować przypomnienia.",
                            "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas anulowania: {ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task UsunPrzypomnienieAsync(Przypomnienie przypomnienie)
        {
            if (przypomnienie == null || IsLoading) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz usunąć przypomnienie?\n\n{przypomnienie.Tytul}",
                "Potwierdzenie usunięcia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var sukces = await _przypomnienieService.UsunPrzypomnienieAsync(przypomnienie.Id);

                    if (sukces)
                    {
                        // Odśwież listę po usunięciu
                        await ZaladujPrzypomnieniAsync();
                        MessageBox.Show("Przypomnienie zostało usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się usunąć przypomnienia.",
                            "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas usuwania: {ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        #endregion
    }
}