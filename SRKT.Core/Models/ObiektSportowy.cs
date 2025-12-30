namespace SRKT.Core.Models
{
    public class ObiektSportowy
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public string Adres { get; set; }
        public decimal? SzerokoscGeo { get; set; }
        public decimal? DlugoscGeo { get; set; }
        public bool CzyAktywny { get; set; }
        public int TypObiektuId { get; set; }
        public DateTime DataUtworzenia { get; set; }
        public DateTime? DataModyfikacji { get; set; }

        public virtual TypObiektu TypObiektu { get; set; }
        public virtual ICollection<Kort> Korty { get; set; }
        public virtual ICollection<GodzinyOtwarcia> GodzinyOtwarcia { get; set; }
    }

    public class TypObiektu
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public virtual ICollection<ObiektSportowy> ObiektySprtowe { get; set; }
    }

    public class GodzinyOtwarcia
    {
        public int Id { get; set; }
        public int ObiektSportowyId { get; set; }
        public int DzienTygodnia { get; set; }
        public TimeSpan GodzinaOd { get; set; }
        public TimeSpan GodzinaDo { get; set; }

        public virtual ObiektSportowy ObiektSportowy { get; set; }
    }
}