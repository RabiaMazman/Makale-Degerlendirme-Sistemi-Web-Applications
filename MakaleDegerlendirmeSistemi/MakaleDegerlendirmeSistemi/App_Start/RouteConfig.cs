using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MakaleDegerlendirmeSistemi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(name: "Login", url: "login/", defaults: new { controller = "Home", action = "Login" });
            routes.MapRoute(name: "HomePage", url: "anasayfa/", defaults: new { controller = "Home", action = "HomePage" });
            routes.MapRoute(name: "DergideYayinlananlar2", url: "dergide-yayınlananlar/", defaults: new { controller = "Home", action = "DergideYayinlananlar2" });

            #region Baş Editör Linkleri
            routes.MapRoute(name: "YeniKisiEkleBasEditor", url: "bas-editor-kisi-ekle/", defaults: new { controller = "Home", action = "YeniKisiEkleBasEditor" });
            routes.MapRoute(name: "MakaleListeleBasEditor", url: "bas-editor-makale-listele/", defaults: new { controller = "Home", action = "MakaleListeleBasEditor" });
            routes.MapRoute(name: "MakaleYonlendirBasEditor", url: "makale-yonlendir-bas-editor/", defaults: new { controller = "Home", action = "MakaleYonlendirBasEditor" });
            routes.MapRoute(name: "KisiListeleBasEditor", url: "bas-editor-kisi-listele/", defaults: new { controller = "Home", action = "KisiListeleBasEditor" });
            routes.MapRoute(name: "MakaleOnerideBulunBasEditor", url: "makale-oneride-bulun-bas-editor/", defaults: new { controller = "Home", action = "MakaleOnerideBulunBasEditor" });

            #endregion Baş Editör Linkleri

            #region Alan Editörü Linkleri

            routes.MapRoute(name: "YeniHakemEkleAlanEditoru", url: "alan-editoru-hakem-ekle/", defaults: new { controller = "Home", action = "YeniHakemEkleAlanEditoru" });
            routes.MapRoute(name: "MakaleListeleAlanEditoru", url: "alan-editoru-makale-listele/", defaults: new { controller = "Home", action = "MakaleListeleAlanEditoru" });
            routes.MapRoute(name: "MakaleYonlendirAlanEditoru", url: "makale-yonlendir-alan-editoru/", defaults: new { controller = "Home", action = "MakaleYonlendirAlanEditoru" });
            routes.MapRoute(name: "HakemListeleAlanEditoru", url: "alan-editoru-hakem-listele/", defaults: new { controller = "Home", action = "HakemListeleAlanEditoru" });

            #endregion Alan Editörü Linkleri

            #region Hakem Linkleri 

            routes.MapRoute(name: "MakaleYukleYazar", url: "yazar-makale-yukle/", defaults: new { controller = "Home", action = "MakaleYukleYazar" });
            routes.MapRoute(name: "MakaleListeleYazar", url: "yazar-makale-listele/", defaults: new { controller = "Home", action = "MakaleListeleYazar" });
            routes.MapRoute(name: "MakaleListeleDigerYazar", url: "diger-yazar-makale-listele/", defaults: new { controller = "Home", action = "MakaleListeleDigerYazar" });


            #endregion Hakem Linkleri
            routes.MapRoute(name: "DavetleriListeleHakem", url: "davetleri-listele/", defaults: new { controller = "Home", action = "DavetleriListeleHakem" });
            routes.MapRoute(name: "DavetiKabulEtHakem", url: "daveti-kabul-et/", defaults: new { controller = "Home", action = "DavetiKabulEtHakem" });
            routes.MapRoute(name: "DavetiReddetHakem", url: "daveti-reddet/", defaults: new { controller = "Home", action = "DavetiReddetHakem" });
            routes.MapRoute(name: "MakaleleleriListeleHakem", url: "makaleleri-listele-hakem/", defaults: new { controller = "Home", action = "MakaleleleriListeleHakem" });
            routes.MapRoute(name: "MakaleDegerlendirHakem", url: "makale-degerlendir-hakem/", defaults: new { controller = "Home", action = "MakaleDegerlendirHakem" });






            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
