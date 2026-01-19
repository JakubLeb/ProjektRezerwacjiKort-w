using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class AdminUzytkownicyViewModel : BaseViewModel
    {
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;
        private readonly IRepository<Rola> _rolaRepo;

        private ObservableCollection<Uzytkownik> _uzytkownicy;
        private ObservableCollection<Uzytkownik> _wszyscyUzytkownicy;
        private ObservableCollection<Rola> _dostepneRole;
        private Uzytkownik _wybranyUzytkownik;
        private Rola _wybranaRolaFiltr;
        private string _frazaWyszukiwania;

        public AdminUzytkownicyViewModel(
            IRepository<Uzytkownik> uzytkownikRepo,
            IRepository<Rola> rolaRepo)
        {
            _uzytkownikRepo = uzytkownikRepo;
            _rolaRepo = rolaRepo;

            Uzytkownicy = new ObservableCollection<Uzytkownik>();
            _wszyscyUzytkownicy = new ObservableCollection<Uzytkownik>();
            DostepneRole = new ObservableCollection<Rola>();

            OdswiezCommand = new RelayCommand(async _ => await ZaladujDaneAsync());
            EdytujUzytkownikaCommand = new RelayCommand(async param => await EdytujUzytkownikaAsync(param as Uzytkownik));
            ZmienRoleCommand = new RelayCommand(async param => await ZmienRoleAsync(param as Uzytkownik));
            UsunUzytkownikaCommand = new RelayCommand(async param => await UsunUzytkownikaAsync(param as Uzytkownik));

            _ = ZaladujDaneAsync();
        }

        // Właściwości
        public ObservableCollection<Uzytkownik> Uzytkownicy
        {
            get => _uzytkownicy;
            set => SetProperty(ref _uzytkownicy, value);
        }

        public ObservableCollection<Rola> DostepneRole
        {
            get => _dostepneRole;
            set => SetProperty(ref _dostepneRole, value);
        }

        public Uzytkownik WybranyUzytkownik
        {
            get => _wybranyUzytkownik;
            set => SetProperty(ref _wybranyUzytkownik, value);
        }

        public Rola WybranaRolaFiltr
        {
            get => _wybranaRolaFiltr;
            set
            {
                if (SetProperty(ref _wybranaRolaFiltr, value))
                {
                    FiltrujUzytkownikow();
                }
            }
        }

        public string FrazaWyszukiwania
        {
            get => _frazaWyszukiwania;
            set
            {
                if (SetProperty(ref _frazaWyszukiwania, value))
                {
                    FiltrujUzytkownikow();
                }
            }
        }

        public int LiczbaUzytkownikow => Uzytkownicy?.Count ?? 0;

        public Visibility BrakUzytkownikow =>
            (Uzytkownicy?.Count ?? 0) == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Komendy
        public ICommand OdswiezCommand { get; }
        public ICommand EdytujUzytkownikaCommand { get; }
        public ICommand ZmienRoleCommand { get; }
        public ICommand UsunUzytkownikaCommand { get; }

        // Metody
        private async Task ZaladujDaneAsync()
        {
            try
            {
                // Załaduj role
                var role = await _rolaRepo.GetAllAsync();
                DostepneRole.Clear();
                DostepneRole.Add(new Rola { Id = 0, Nazwa = "Wszystkie" }); // Opcja "wszystkie"
                foreach (var rola in role)
                {
                    DostepneRole.Add(rola);
                }

                // Załaduj użytkowników
                var uzytkownicy = await _uzytkownikRepo.GetAllAsync();
                _wszyscyUzytkownicy.Clear();
                foreach (var uzytkownik in uzytkownicy.OrderBy(u => u.Id))
                {
                    // Przypisz rolę jeśli nie jest załadowana
                    if (uzytkownik.Rola == null)
                    {
                        uzytkownik.Rola = role.FirstOrDefault(r => r.Id == uzytkownik.RolaId);
                    }
                    _wszyscyUzytkownicy.Add(uzytkownik);
                }

                FiltrujUzytkownikow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrujUzytkownikow()
        {
            var przefiltrowane = _wszyscyUzytkownicy.AsEnumerable();

            // Filtr po roli
            if (WybranaRolaFiltr != null && WybranaRolaFiltr.Id > 0)
            {
                przefiltrowane = przefiltrowane.Where(u => u.RolaId == WybranaRolaFiltr.Id);
            }

            // Filtr po frazie wyszukiwania
            if (!string.IsNullOrWhiteSpace(FrazaWyszukiwania))
            {
                var fraza = FrazaWyszukiwania.ToLower();
                przefiltrowane = przefiltrowane.Where(u =>
                    u.Imie.ToLower().Contains(fraza) ||
                    u.Nazwisko.ToLower().Contains(fraza) ||
                    u.Email.ToLower().Contains(fraza) ||
                    u.PelneImieNazwisko.ToLower().Contains(fraza));
            }

            Uzytkownicy.Clear();
            foreach (var uzytkownik in przefiltrowane)
            {
                Uzytkownicy.Add(uzytkownik);
            }

            OnPropertyChanged(nameof(LiczbaUzytkownikow));
            OnPropertyChanged(nameof(BrakUzytkownikow));
        }

        private async Task EdytujUzytkownikaAsync(Uzytkownik uzytkownik)
        {
            if (uzytkownik == null) return;

            // Okno dialogowe do edycji danych użytkownika
            var dialog = new EdytujUzytkownikaWindow(uzytkownik);

            // Bezpieczne ustawienie Owner - znajdź aktywne okno
            var activeWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow != null && activeWindow != dialog)
            {
                dialog.Owner = activeWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Zaktualizuj dane
                    uzytkownik.Imie = dialog.NoweImie;
                    uzytkownik.Nazwisko = dialog.NoweNazwisko;
                    uzytkownik.Email = dialog.NowyEmail;

                    await _uzytkownikRepo.UpdateAsync(uzytkownik);

                    MessageBox.Show("Dane użytkownika zostały zaktualizowane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    await ZaladujDaneAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd aktualizacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ZmienRoleAsync(Uzytkownik uzytkownik)
        {
            if (uzytkownik == null) return;

            // Sprawdź czy to nie jest jedyny administrator
            if (uzytkownik.RolaId == 1)
            {
                var adminCount = _wszyscyUzytkownicy.Count(u => u.RolaId == 1);
                if (adminCount <= 1)
                {
                    MessageBox.Show(
                        "Nie można zmienić roli jedynemu administratorowi w systemie.",
                        "Ostrzeżenie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            var nowaRola = uzytkownik.RolaId == 1 ? "Użytkownik" : "Administrator";
            var result = MessageBox.Show(
                $"Czy na pewno chcesz zmienić rolę użytkownika {uzytkownik.PelneImieNazwisko} na '{nowaRola}'?",
                "Zmiana roli",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    uzytkownik.RolaId = uzytkownik.RolaId == 1 ? 2 : 1;
                    await _uzytkownikRepo.UpdateAsync(uzytkownik);

                    MessageBox.Show($"Rola użytkownika została zmieniona na '{nowaRola}'.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    await ZaladujDaneAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd zmiany roli: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task UsunUzytkownikaAsync(Uzytkownik uzytkownik)
        {
            if (uzytkownik == null) return;

            // Sprawdź czy to nie jest administrator
            if (uzytkownik.RolaId == 1)
            {
                var adminCount = _wszyscyUzytkownicy.Count(u => u.RolaId == 1);
                if (adminCount <= 1)
                {
                    MessageBox.Show(
                        "Nie można usunąć jedynego administratora w systemie.",
                        "Ostrzeżenie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            var result = MessageBox.Show(
                $"Czy na pewno chcesz USUNĄĆ użytkownika?\n\n" +
                $"Imię i nazwisko: {uzytkownik.PelneImieNazwisko}\n" +
                $"Email: {uzytkownik.Email}\n\n" +
                $"Ta operacja jest nieodwracalna!",
                "Potwierdzenie usunięcia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _uzytkownikRepo.DeleteAsync(uzytkownik.Id);

                    MessageBox.Show("Użytkownik został usunięty.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    await ZaladujDaneAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd usuwania użytkownika: {ex.Message}\n\nMoże mieć powiązane rezerwacje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}