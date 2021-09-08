using MakaleDegerlendirmeSistemi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MakaleDegerlendirmeSistemi.ViewModels
{
    public class BasEditorKisiListeleModel
    {
        public int KisiID { get; set; }

        public string KisiAd { get; set; }

        public string KisiSoyad { get; set; }

        public string KisiEmail { get; set; }
        public string KisiSifre { get; set; }
        public string KisiSifreTekrar { get; set; }

        public string RolAdi{ get; set; }

    }
}