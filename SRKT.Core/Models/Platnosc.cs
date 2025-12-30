namespace SRKT.Core.Models
{
    public class Platnosc
    {
        public int Id { get; set; }
        public int RezerwacjaId { get; set; }
        public decimal Kwota { get; set; }
        public bool CzyPlatnoscZatwierdzona { get; set; }
        public DateTime DataUtworzenia { get; set; }
        public int MetodaPlatnosciId { get; set; }

        public virtual Rezerwacja Rezerwacja { get; set; }
        public virtual MetodaPlatnosci MetodaPlatnosci { get; set; }
    }

    public class MetodaPlatnosci
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<Platnosc> Platnosci { get; set; }
    }
}