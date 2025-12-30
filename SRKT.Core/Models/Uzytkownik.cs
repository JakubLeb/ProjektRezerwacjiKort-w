namespace SRKT.Core.Models
{
    public class Uzytkownik
    {
        public int Id { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Email { get; set; }
        public string HasloHash { get; set; }
        public int RolaId { get; set; }
        public DateTime DataUtworzenia { get; set; }

        public virtual Rola Rola { get; set; }
        public virtual ICollection<Rezerwacja> Rezerwacje { get; set; }
        public virtual ICollection<Powiadomienie> Powiadomienia { get; set; }

        public string PelneImieNazwisko => $"{Imie} {Nazwisko}";
    }

    public class Rola
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<Uzytkownik> Uzytkownicy { get; set; }
    }
}