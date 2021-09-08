using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.Models.Managers
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Kisiler> Kisiler { get; set; }
        public DbSet<Makaleler> Makaleler{ get; set; }
        public DbSet<Roller> Roller { get; set; }
        public DbSet<Yazarlar> Yazarlar { get; set; }

        public DatabaseContext()
        {
            Database.SetInitializer(new VeritabaniOlusturucu());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DatabaseContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }

    public class VeritabaniOlusturucu : CreateDatabaseIfNotExists<DatabaseContext>
    {
        public VeritabaniOlusturucu()
        {


        }
        protected override void Seed(DatabaseContext context)
        {
            Debug.WriteLine("Seed başladı");

            for (int i = 0; i < 10; i++)
            {
                Kisiler kisi = new Kisiler();
                kisi.KisiAd = FakeData.NameData.GetFirstName();
                kisi.KisiSoyad = FakeData.NameData.GetSurname();
                kisi.KisiEmail = kisi.KisiAd.ToLower().ToString() + "_" + kisi.KisiSoyad.ToLower().ToString() + "@gmail.com";
                Roller rol = new Roller();
                rol.RolAd = "Yazar";
                kisi.RolIDFK = 1;
                context.Kisiler.Add(kisi);   
            }
            context.SaveChanges();

        }
        
    }
}