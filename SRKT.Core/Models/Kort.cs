namespace SRKT.Core.Models
{
    public class Kort
    {
        public int Id { get; set; }
        public int ObiektSportowyId { get; set; }
        public string Nazwa { get; set; }
        public bool CzyAktywny { get; set; }
        public decimal CenaZaGodzine { get; set; }
        public int TypKortuId { get; set; }
        public string? SciezkaZdjecia { get; set; }
        public DateTime DataUtworzenia { get; set; }
        public DateTime? DataModyfikacji { get; set; }

        public virtual ObiektSportowy ObiektSportowy { get; set; }
        public virtual TypKortu TypKortu { get; set; }
        public virtual ICollection<Rezerwacja> Rezerwacje { get; set; }

        public string PelnaNazwa => $"{Nazwa} - {TypKortu?.Nazwa}";
    }

    public class TypKortu
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<Kort> Korty { get; set; }
    }
}