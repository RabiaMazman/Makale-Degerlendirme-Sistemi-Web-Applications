using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.ViewModels
{
    public class BasEditorMakaleListeleModel
    {
        public int MakaleID { get; set; }
        [StringLength(100), Required]
        public string MakaleBaslik { get; set; }

        [StringLength(500), Required]
        public string MakaleAciklama { get; set; }

        public string MakaleDosyaYol { get; set; }
        public string RevizyonIstenmisMi { get; set; }
        public string MakaleDurum { get; set; }
        public string YazarKullaniciAdi { get; set; }
        public string Hakemler { get; set; }
        public string AlanEditoru { get; set; }
        public string MakaleDegisimTarihi { get; set; }
        public Nullable<int> Not1 { get; set; }
        public Nullable<int> Not2 { get; set; }
        public Nullable<int> Not3 { get; set; }
        public Nullable<System.Double> NotOrt { get; set; }
        public HttpPostedFileBase HakemDegerlendirmeRaporuYol { get; set; }

    }
}