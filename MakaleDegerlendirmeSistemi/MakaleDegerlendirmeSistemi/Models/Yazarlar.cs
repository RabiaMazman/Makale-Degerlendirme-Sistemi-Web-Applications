using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.Models
{
    [Table("Yazarlar")]
    public class Yazarlar
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int YazarID { get; set; }
        [StringLength(20),Required]
        public string YazarAd { get; set; }

        [StringLength(20), Required]
        public string YazarSoyad { get; set; }

        [StringLength(100)]
        public string YazarKurum { get; set; }

        [StringLength(100)]
        public string YazarEmail { get; set; }
        [StringLength(100)]
        public string YazarSifre { get; set; }
        [StringLength(100)]
        public string YazarSifreTekrar { get; set; }

        public int RolIDFK { get; set; }
    }
}