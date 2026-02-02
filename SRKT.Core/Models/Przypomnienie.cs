namespace SRKT.Core.Models
{
    public class Przypomnienie
    {
        public int Id { get; set; }
        public int RezerwacjaId { get; set; }
        public int UzytkownikId { get; set; }
        public DateTime DataPrzypomnienia { get; set; }
        public string Tytul { get; set; }
        public string Tresc { get; set; }
        public bool CzyWyslane { get; set; }
        public bool CzyAktywne { get; set; }
        public DateTime DataUtworzenia { get; set; }

        // Nawigacja
        public virtual Rezerwacja Rezerwacja { get; set; }
        public virtual Uzytkownik Uzytkownik { get; set; }

        // Pomocnicze
        public string StatusTekst => CzyWyslane ? "Wysłane" : (CzyAktywne ? "Oczekuje" : "Anulowane");
        public bool MoznaEdytowac => !CzyWyslane && CzyAktywne;
    }
}
