using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.Models
{
    [Table("Makaleler")]
    public class Makaleler
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MakaleID { get; set; }

        [StringLength(100), Required]
        public string MakaleBaslik { get; set; }

        [StringLength(500), Required]
        public string MakaleAciklama { get; set; }
         
        public string MakaleDosyaYol { get; set; }
        public int RevizyonIstenmisMi { get; set; }
        public string MakaleDurum { get; set; }
        public int YazarIDFK { get; set; }
        public int AlanEditoruIDFK{ get; set; }
        public string Kisiler { get; set; }
        public string MakaleDegisimTarihi{ get; set; }
        public Nullable<int> Not1 { get; set; }
        public Nullable<int> Not2 { get; set; }
        public Nullable<int> Not3 { get; set; }
        public Nullable<System.Double> NotOrt { get; set; }
        public string Hakem1DegerlendirmeRaporuYol { get; set; }
        public string Hakem2DegerlendirmeRaporuYol { get; set; }
        public string Hakem3DegerlendirmeRaporuYol { get; set; }


    }
}