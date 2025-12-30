namespace SRKT.Core.Models
{
    public class Powiadomienie
    {
        public int Id { get; set; }
        public int UzytkownikId { get; set; }
        public int? RezerwacjaId { get; set; }
        public string Tytul { get; set; }
        public string Tresc { get; set; }
        public int TypPowiadomieniaId { get; set; }
        public int StatusPowiadomieniaId { get; set; }
        public DateTime? DataWyslania { get; set; }
        public DateTime DataUtworzenia { get; set; }

        public virtual Uzytkownik Uzytkownik { get; set; }
        public virtual Rezerwacja Rezerwacja { get; set; }
        public virtual TypPowiadomienia TypPowiadomienia { get; set; }
        public virtual StatusPowiadomienia StatusPowiadomienia { get; set; }
    }

    public class TypPowiadomienia
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<Powiadomienie> Powiadomienia { get; set; }
    }

    public class StatusPowiadomienia
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<Powiadomienie> Powiadomienia { get; set; }
    }
}