namespace SRKT.Core.Models
{
    public class Rezerwacja
    {
        public int Id { get; set; }
        public int KortId { get; set; }
        public int UzytkownikId { get; set; }
        public DateTime DataRezerwacji { get; set; }
        public decimal IloscGodzin { get; set; }
        public decimal KosztCalkowity { get; set; }
        public bool CzyOplacona { get; set; }
        public int StatusRezerwacjiId { get; set; }
        public DateTime DataUtworzenia { get; set; }
        public DateTime? DataModyfikacji { get; set; }
        public string Uwagi { get; set; }
        public string SciezkaZdjecia { get; set; }

        public virtual Kort Kort { get; set; }
        public virtual Uzytkownik Uzytkownik { get; set; }
        public virtual StatusRezerwacji StatusRezerwacji { get; set; }
        public virtual ICollection<Platnosc> Platnosci { get; set; }
        public virtual ICollection<Powiadomienie> Powiadomienia { get; set; }

        public DateTime DataZakonczenia => DataRezerwacji.AddHours((double)IloscGodzin);
    }

    public class StatusRezerwacji
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<Rezerwacja> Rezerwacje { get; set; }
    }
}