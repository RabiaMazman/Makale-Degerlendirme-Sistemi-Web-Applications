using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.ViewModels
{
    public class HakemMakaleDegerlendirModel
    {
        public int MakaleID { get; set; }
        public string RevizyonIstegi { get; set; }
        
        public string RevizyonIstenmisMi { get; set; }
        public string MakaleDurum { get; set; }
        
        public string MakaleDegisimTarihi { get; set; }
        public Nullable<int> Not { get; set; }
        public HttpPostedFileBase HakemDegerlendirmeRaporuYol { get; set; }

    }
}