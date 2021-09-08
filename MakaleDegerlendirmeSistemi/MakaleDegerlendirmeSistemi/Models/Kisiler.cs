using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.Models
{
    [Table("Kisiler")]
    public class Kisiler
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KisiID { get; set; }

        [StringLength(20), Required]
        public string KisiAd { get; set; }
        
        [StringLength(20), Required]
        public string KisiSoyad { get; set; }

        [StringLength(100)]
        public string KisiEmail { get; set; }
        [StringLength(100)]
        public string KisiSifre { get; set; }
        [StringLength(100)]
        public string KisiSifreTekrar { get; set; }

        public int RolIDFK { get; set; }

    }
}