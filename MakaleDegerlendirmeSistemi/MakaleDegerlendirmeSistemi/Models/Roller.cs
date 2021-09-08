using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.Models
{
    [Table("Roller")]
    public class Roller
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RolID{ get; set; }
        [StringLength(20), Required]
        public string RolAd { get; set; }
        
    }
}