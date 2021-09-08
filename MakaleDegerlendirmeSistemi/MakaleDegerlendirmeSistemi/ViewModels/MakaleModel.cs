using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.ViewModels
{
    public class MakaleModel
    {
        [StringLength(100), Required]
        public string MakaleBaslik { get; set; }

        [StringLength(500), Required]
        public string MakaleAciklama { get; set; }

        public HttpPostedFileBase MakaleDosyaYol { get; set; }
        public int RevizyonIstenmisMi { get; set; }
        public string MakaleDurum { get; set; }
        public string YazarKullaniciAdi { get; set; }
        public string Hakemler{ get; set; }
    }
}