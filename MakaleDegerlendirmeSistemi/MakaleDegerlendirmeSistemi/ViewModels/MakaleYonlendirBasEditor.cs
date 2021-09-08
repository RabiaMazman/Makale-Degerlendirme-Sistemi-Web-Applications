using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MakaleDegerlendirmeSistemi.ViewModels
{
    public class MakaleYonlendirBasEditor
    {
        public int MakaleID { get; set; }
        public string MakaleBaslik { get; set; }
        public string MakaleAciklama { get; set; }
        public string AlanEditorleri { get; set; }
    }
}