using Microsoft.EntityFrameworkCore;
using SRKT.Core.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace SRKT.DataAccess
{
    public class SRKTDbContext : DbContext
    {
        public SRKTDbContext(DbContextOptions<SRKTDbContext> options) : base(options) { }

        public DbSet<Uzytkownik> Uzytkownicy { get; set; }
        public DbSet<Rola> Role { get; set; }
        public DbSet<ObiektSportowy> ObiektySprtowe { get; set; }
        public DbSet<TypObiektu> TypyObiektu { get; set; }
        public DbSet<Kort> Korty { get; set; }
        public DbSet<TypKortu> TypyKortu { get; set; }
        public DbSet<Rezerwacja> Rezerwacje { get; set; }
        public DbSet<StatusRezerwacji> StatusyRezerwacji { get; set; }
        public DbSet<Platnosc> Platnosci { get; set; }
        public DbSet<MetodaPlatnosci> MetodyPlatnosci { get; set; }
        public DbSet<Powiadomienie> Powiadomienia { get; set; }
        public DbSet<TypPowiadomienia> TypyPowiadomien { get; set; }
        public DbSet<StatusPowiadomienia> StatusyPowiadomien { get; set; }
        public DbSet<GodzinyOtwarcia> GodzinyOtwarcia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja tabel
            modelBuilder.Entity<Uzytkownik>().ToTable("UZYTKOWNIK");
            modelBuilder.Entity<Rola>().ToTable("ROLA");
            modelBuilder.Entity<ObiektSportowy>().ToTable("OBIEKT_SPORTOWY");
            modelBuilder.Entity<TypObiektu>().ToTable("TYP_OBIEKTU");
            modelBuilder.Entity<Kort>().ToTable("KORT");
            modelBuilder.Entity<TypKortu>().ToTable("TYP_KORTU");
            modelBuilder.Entity<Rezerwacja>().ToTable("REZERWACJA");
            modelBuilder.Entity<StatusRezerwacji>().ToTable("STATUS_REZERWACJI");
            modelBuilder.Entity<Platnosc>().ToTable("PLATNOSC");
            modelBuilder.Entity<MetodaPlatnosci>().ToTable("METODA_PLATNOSCI");
            modelBuilder.Entity<Powiadomienie>().ToTable("POWIADOMIENIE");
            modelBuilder.Entity<TypPowiadomienia>().ToTable("TYP_POWIADOMIENIA");
            modelBuilder.Entity<StatusPowiadomienia>().ToTable("STATUS_POWIADOMIENIA");
            modelBuilder.Entity<GodzinyOtwarcia>().ToTable("GODZINY_OTWARCIA");

            // Relacje
            modelBuilder.Entity<Uzytkownik>()
                .HasOne(u => u.Rola)
                .WithMany(r => r.Uzytkownicy)
                .HasForeignKey(u => u.RolaId);

            modelBuilder.Entity<ObiektSportowy>()
                .HasOne(o => o.TypObiektu)
                .WithMany(t => t.ObiektySprtowe)
                .HasForeignKey(o => o.TypObiektuId);

            modelBuilder.Entity<Kort>()
                .HasOne(k => k.ObiektSportowy)
                .WithMany(o => o.Korty)
                .HasForeignKey(k => k.ObiektSportowyId);

            modelBuilder.Entity<Kort>()
                .HasOne(k => k.TypKortu)
                .WithMany(t => t.Korty)
                .HasForeignKey(k => k.TypKortuId);

            modelBuilder.Entity<Rezerwacja>()
                .HasOne(r => r.Kort)
                .WithMany(k => k.Rezerwacje)
                .HasForeignKey(r => r.KortId);

            modelBuilder.Entity<Rezerwacja>()
                .HasOne(r => r.Uzytkownik)
                .WithMany(u => u.Rezerwacje)
                .HasForeignKey(r => r.UzytkownikId);

            modelBuilder.Entity<Rezerwacja>()
                .HasOne(r => r.StatusRezerwacji)
                .WithMany(s => s.Rezerwacje)
                .HasForeignKey(r => r.StatusRezerwacjiId);

            modelBuilder.Entity<Platnosc>()
                .HasOne(p => p.Rezerwacja)
                .WithMany(r => r.Platnosci)
                .HasForeignKey(p => p.RezerwacjaId);

            modelBuilder.Entity<Platnosc>()
                .HasOne(p => p.MetodaPlatnosci)
                .WithMany(m => m.Platnosci)
                .HasForeignKey(p => p.MetodaPlatnosciId);

            modelBuilder.Entity<Powiadomienie>()
                .HasOne(p => p.Uzytkownik)
                .WithMany(u => u.Powiadomienia)
                .HasForeignKey(p => p.UzytkownikId);

            modelBuilder.Entity<Powiadomienie>()
                .HasOne(p => p.Rezerwacja)
                .WithMany(r => r.Powiadomienia)
                .HasForeignKey(p => p.RezerwacjaId)
                .IsRequired(false);
        }
    }
}
