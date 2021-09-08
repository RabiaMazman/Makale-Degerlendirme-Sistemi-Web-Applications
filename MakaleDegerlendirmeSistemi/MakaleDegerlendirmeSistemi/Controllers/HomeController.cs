using MakaleDegerlendirmeSistemi.Models;
using MakaleDegerlendirmeSistemi.Models.Managers;
using MakaleDegerlendirmeSistemi.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Data.Entity;
using System.Net.Mail;

namespace MakaleDegerlendirmeSistemi.Controllers
{
    public class HomeController : Controller
    {
        DatabaseContext db = new DatabaseContext();
        //Proje içinde değişkenlerle çağrılacak. Buradaki isimler veritabanındaki değerlerdir.
        string rolBasEditorAd = "Baş Editör", rolAlanEditoruAd = "Alan Editörü", rolYazarAd = "Yazar", rolHakemAd = "Hakem";
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(Yazarlar yazarModel)
        {
            Yazarlar yazar = new Yazarlar();
            yazar.YazarAd = yazarModel.YazarAd;
            yazar.YazarSoyad = yazarModel.YazarSoyad;
            yazar.YazarEmail = yazarModel.YazarEmail;
            yazar.YazarKurum = yazar.YazarKurum;
            yazar.RolIDFK = 1;
            yazar.YazarSifre = yazarModel.YazarSifre;
            yazar.YazarSifreTekrar = yazarModel.YazarSifreTekrar;
            if (yazar.YazarSifreTekrar == yazar.YazarSifre)
            {
                db.Yazarlar.Add(yazar);
                db.SaveChanges();
                return View();
            }
            else
            {
                TempData["ParolaKontrol"] = "<script>alert('Şifre ile Tekrar Şifre alanı aynı değildir. Kayıt yapılamamıştır. Tekrar deneyiniz.');</script>";
                return View();
            }
        }

        public ActionResult DergideYayinlananlar2()
        {
            var makaleler = db.Makaleler.Where(a => a.NotOrt >= 75).ToList();
            List<BasEditorMakaleListeleModel> makaleModelListesi = new List<BasEditorMakaleListeleModel>();
            foreach (var item in makaleler)
            {
                BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();

                makaleModel.MakaleBaslik = item.MakaleBaslik;
                Debug.WriteLine("MakaleBaslik : " + makaleModel.MakaleBaslik);

                makaleModel.MakaleAciklama = item.MakaleAciklama;
                makaleModel.MakaleDosyaYol = @"\Content\uploadPDF\" + item.MakaleDosyaYol;
                Debug.WriteLine("makaleDosyaYol : " + makaleModel.MakaleDosyaYol);
                var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                string yazarAdiSoyadi = yazar.YazarAd + " " + yazar.YazarSoyad;
                makaleModel.YazarKullaniciAdi = yazarAdiSoyadi;
                makaleModelListesi.Add(makaleModel);
            }
            return View(makaleModelListesi);
        }
       
        public ActionResult Login(Kisiler kisiModel)
        {
            Kisiler kisi = db.Kisiler.Where(a => a.KisiEmail.Equals(kisiModel.KisiEmail)).FirstOrDefault();
            Yazarlar yazar = db.Yazarlar.Where(a => a.YazarEmail == kisiModel.KisiEmail).FirstOrDefault();
            if (kisi != null)
            {
                string password = kisi.KisiSifre;
                string rolAd = db.Roller.Where(a => a.RolID == kisi.RolIDFK).FirstOrDefault().RolAd.ToString();
                if(kisi.KisiEmail == kisiModel.KisiEmail && kisi.KisiSifre == kisiModel.KisiSifre)
                {
                    string kullaniciAdi =  kisi.KisiAd + " " + kisi.KisiSoyad;
                    ViewBag.KullaniciAdi = kullaniciAdi;
                    if (rolAd == rolBasEditorAd)
                        SessionBelirle(kullaniciAdi, rolAd, kisi.KisiEmail, "/bas-editor-kisi-ekle/", "/bas-editor-makale-listele/", "/bas-editor-kisi-listele/", "", "", "Kişi Ekle | ", "Makaleleri Listele | ", "Kişi Listele", "", "");
                    else if(rolAd == rolAlanEditoruAd)
                        SessionBelirle(kullaniciAdi, rolAd, kisi.KisiEmail, "/alan-editoru-hakem-ekle/", "/alan-editoru-makale-listele/", "/alan-editoru-hakem-listele/", "", "", "Hakem Ekle | ", "Makaleleri Listele | ", "Hakemleri Listele", "", "");
                    else if (rolAd == rolHakemAd)
                        SessionBelirle(kullaniciAdi, rolAd, kisi.KisiEmail, "/davetleri-listele/", "/makaleleri-listele-hakem/", "", "", "", "Davetleri Listele | ", "Makaleleri Listele", "", "", "");
                    return RedirectToAction("HomePage");
                }
                else
                {
                    return View();
                }
            }
            else if(yazar!= null)
            {
                string password = yazar.YazarSifre;
                string rolAd = db.Roller.Where(a => a.RolID == yazar.RolIDFK).FirstOrDefault().RolAd.ToString();
                if (yazar.YazarEmail == kisiModel.KisiEmail && yazar.YazarSifre == kisiModel.KisiSifre)
                {
                    string kullaniciAdi = yazar.YazarAd+ " " + yazar.YazarSoyad;
                    ViewBag.KullaniciAdi = kullaniciAdi;
                    SessionBelirle(kullaniciAdi, rolAd, yazar.YazarEmail, "/yazar-makale-yukle/", "/yazar-makale-listele/", "/diger-yazar-makale-listele/", "", "", "Makale Yükle | ", "Makaleleri Listele | ", "Diğer Yazarlar Makale Listele", "", "");
                    return RedirectToAction("HomePage");
                }
                else
                {
                    return View();
                }
            }
            return View();
        }
        public ActionResult HomePage()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
        #region Yazar sayfaları
        public ActionResult MakaleYukleYazar()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult MakaleYukleYazar(MakaleModel makaleModel)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolYazarAd)
            {
                Makaleler makale = new Makaleler();
                makale.MakaleBaslik = makaleModel.MakaleBaslik;
                makale.MakaleAciklama = makaleModel.MakaleAciklama;
                makale.RevizyonIstenmisMi = 0;
                string dosyaAdi = Path.GetFileName(makaleModel.MakaleDosyaYol.FileName);
                FileInfo ff = new FileInfo(makaleModel.MakaleDosyaYol.FileName);
                string dosyaUzantisi = ff.Extension;
                if (dosyaUzantisi != ".pdf" && dosyaUzantisi != ".PDF")
                {
                    TempData["dosyaUzantisi"] = "<script>alert('Sadece pdf uzantılı dosyalarla yarışmaya başvurabilirsiniz.');</script>";
                    return View();
                }
                if (dosyaAdi.Contains(dosyaUzantisi))
                {
                    dosyaAdi = dosyaAdi.Replace(dosyaUzantisi, "");
                }
                if(dosyaAdi.Length>40)
                {
                    dosyaAdi = dosyaAdi.Substring(0, 20);
                }
                dosyaAdi = dosyaAdi + "-" + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + dosyaUzantisi;
                 
                makaleModel.MakaleDosyaYol.SaveAs(@"C:\Users\yasü\Documents\Visual Studio 2015\Projects\MakaleDegerlendirmeSistemi\MakaleDegerlendirmeSistemi\Content\uploadPDF\"+dosyaAdi);
                makale.MakaleDosyaYol = dosyaAdi;
                makale.MakaleDurum = "Makale yüklendi. Baş editörden yönlendirme bekleniyor.";
                string sessionKullaniciEmail = Session["KullaniciEmail"].ToString();
                int yazarIDFK = db.Yazarlar.Where(a => a.YazarEmail.Equals(sessionKullaniciEmail)).FirstOrDefault().YazarID;
                makale.YazarIDFK = yazarIDFK;
                makale.AlanEditoruIDFK = -1;
                makale.MakaleDegisimTarihi = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                makale.Not1 = -1;
                makale.Not2 = -1;
                makale.Not3 = -1;
                makale.NotOrt = -1;

                db.Makaleler.Add(makale);
                db.SaveChanges();
                string mailBody = "<html><body>Sayın " + Session["KullaniciAdi"].ToString() + ", <br>" + "Makale Değerlendirme Sistemi'ne size ait bir makale yüklenmiştir. Bu makale sizle alakalı değilse " +
                    "lütfen rabis19691969@gmail.com adresine bildiriniz.</body></html>";
                MailAt(Session["KullaniciEmail"].ToString(), "Makale Kontrol", mailBody);
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public ActionResult MakaleListeleYazar()
        {

            //Yazar burada kendi makalelerini ve detaylarını görebilecek.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolYazarAd)
            {
                List<BasEditorMakaleListeleModel> makaleModelList = new List<BasEditorMakaleListeleModel>();
                string yazar_email = Session["KullaniciEmail"].ToString();
                var loginOlanYazar = db.Yazarlar.Where(a => a.YazarEmail == yazar_email).FirstOrDefault();
                //Baş editörün makaleleri görüntüleyebilmesi için yeni bir model oluşturduk. 
                List<Makaleler> makaleler = db.Makaleler.Where(a => a.YazarIDFK == loginOlanYazar.YazarID).ToList();
                foreach (var item in makaleler)
                {
                    BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();
                    makaleModel.MakaleID = item.MakaleID;
                    makaleModel.MakaleBaslik = item.MakaleBaslik;
                    makaleModel.MakaleAciklama = item.MakaleAciklama;
                    makaleModel.MakaleDosyaYol = item.MakaleDosyaYol;
                    makaleModel.RevizyonIstenmisMi = RevizyonIstenmisMiBelirle(item.RevizyonIstenmisMi);
                    makaleModel.MakaleDurum = item.MakaleDurum;
                    makaleModel.Not1 = item.Not1;
                    makaleModel.Not2 = item.Not2;
                    makaleModel.Not3 = item.Not3;
                    makaleModel.NotOrt = item.NotOrt;
                    //Yazarın sadece id'si elimizde olduğu için o id'den yazar adı ve soyadına erişiyoruz.
                    var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                    if (yazar != null)
                    {
                        makaleModel.YazarKullaniciAdi = yazar.YazarAd + " " + yazar.YazarSoyad;
                    }
                    //Hakem listesi Makaleler tablosunun Kisiler kolonunda string bir şekilde tutuluyor. Hakemlerin kişi id'leri aralarına virgül atılarak tutulmaktadır.
                    //O yüzden Makale tablosundan Kisiler bilgisi alınır. Virgüle göre parçalanarak id'ler bulunur. Sonra o id'ler veritabanından sorgulanarak id'lere ait
                    //hakemlerin ad ve soyadları bulunur.
                    //Model bu bilgiler ışığında doldurulur. Model, sayfaya gönderilecek olan model listesi ışığında doldurulur.
                    string[] hakemIDListString;
                    string hakemler = db.Makaleler.Where(a => a.Kisiler.Equals(item.Kisiler)).FirstOrDefault().Kisiler;
                    string hakemKullaniciAdiHepsi = "";
                    if (hakemler != null)
                    {
                        hakemIDListString = hakemler.Split(',');
                        foreach (string hakemID in hakemIDListString)
                        {
                            int hakem_id = Convert.ToInt32(hakemID);
                            var hakem = db.Kisiler.Where(a => a.KisiID.Equals(hakem_id)).FirstOrDefault();
                            if (hakem != null)
                            {
                                hakemKullaniciAdiHepsi += hakem.KisiAd + " " + hakem.KisiSoyad + ",";
                            }
                            else
                            {
                                hakemKullaniciAdiHepsi += "";
                            }
                        }
                        hakemKullaniciAdiHepsi = hakemKullaniciAdiHepsi.Remove(hakemKullaniciAdiHepsi.Length - 1, 1);
                    }
                    makaleModel.Hakemler = hakemKullaniciAdiHepsi;

                    Kisiler alanEditoru = db.Kisiler.Where(a => a.KisiID == item.AlanEditoruIDFK).FirstOrDefault();
                    if (alanEditoru != null)
                    {
                        string alanEditoruAdiSoyadi = alanEditoru.KisiAd + " " + alanEditoru.KisiSoyad;
                        makaleModel.AlanEditoru = alanEditoruAdiSoyadi;
                    }
                    else
                    {
                        makaleModel.AlanEditoru = "";
                    }
                    makaleModelList.Add(makaleModel);
                    makaleModel.MakaleDegisimTarihi = item.MakaleDegisimTarihi;
                }
                return View(makaleModelList);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [AllowAnonymous]
        public ActionResult MakaleListeleDigerYazar()
        {

            //Bütün yazarların makaleleri ana hatlarıya görülebilecek.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolYazarAd)
            {
                List<BasEditorMakaleListeleModel> makaleModelList = new List<BasEditorMakaleListeleModel>();
                //Baş editörün makaleleri görüntüleyebilmesi için yeni bir model oluşturduk. 
                List<Makaleler> makaleler = db.Makaleler.ToList();
                foreach (var item in makaleler)
                {
                    BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();
                    makaleModel.MakaleID = item.MakaleID;
                    makaleModel.MakaleBaslik = item.MakaleBaslik;
                    makaleModel.MakaleAciklama = item.MakaleAciklama;
                    makaleModel.MakaleDosyaYol = item.MakaleDosyaYol;
                    makaleModel.RevizyonIstenmisMi = RevizyonIstenmisMiBelirle(item.RevizyonIstenmisMi);
                    makaleModel.MakaleDurum = item.MakaleDurum;
                    makaleModel.Not1 = item.Not1;
                    makaleModel.Not2 = item.Not2;
                    makaleModel.Not3 = item.Not3;
                    makaleModel.NotOrt = item.NotOrt;
                    //Yazarın sadece id'si elimizde olduğu için o id'den yazar adı ve soyadına erişiyoruz.
                    var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                    if (yazar != null)
                    {
                        makaleModel.YazarKullaniciAdi = yazar.YazarAd + " " + yazar.YazarSoyad;
                    }
                    //Hakem listesi Makaleler tablosunun Kisiler kolonunda string bir şekilde tutuluyor. Hakemlerin kişi id'leri aralarına virgül atılarak tutulmaktadır.
                    //O yüzden Makale tablosundan Kisiler bilgisi alınır. Virgüle göre parçalanarak id'ler bulunur. Sonra o id'ler veritabanından sorgulanarak id'lere ait
                    //hakemlerin ad ve soyadları bulunur.
                    //Model bu bilgiler ışığında doldurulur. Model, sayfaya gönderilecek olan model listesi ışığında doldurulur.
                    string[] hakemIDListString;
                    string hakemler = db.Makaleler.Where(a => a.Kisiler.Equals(item.Kisiler)).FirstOrDefault().Kisiler;
                    string hakemKullaniciAdiHepsi = "";
                    if (hakemler != null)
                    {
                        hakemIDListString = hakemler.Split(',');
                        foreach (string hakemID in hakemIDListString)
                        {
                            int hakem_id = Convert.ToInt32(hakemID);
                            var hakem = db.Kisiler.Where(a => a.KisiID.Equals(hakem_id)).FirstOrDefault();
                            if (hakem != null)
                            {
                                hakemKullaniciAdiHepsi += hakem.KisiAd + " " + hakem.KisiSoyad + ",";
                            }
                            else
                            {
                                hakemKullaniciAdiHepsi += "";
                            }
                        }
                        hakemKullaniciAdiHepsi = hakemKullaniciAdiHepsi.Remove(hakemKullaniciAdiHepsi.Length - 1, 1);
                    }
                    makaleModel.Hakemler = hakemKullaniciAdiHepsi;

                    Kisiler alanEditoru = db.Kisiler.Where(a => a.KisiID == item.AlanEditoruIDFK).FirstOrDefault();
                    if (alanEditoru != null)
                    {
                        string alanEditoruAdiSoyadi = alanEditoru.KisiAd + " " + alanEditoru.KisiSoyad;
                        makaleModel.AlanEditoru = alanEditoruAdiSoyadi;
                    }
                    else
                    {
                        makaleModel.AlanEditoru = "";
                    }
                    makaleModelList.Add(makaleModel);
                    makaleModel.MakaleDegisimTarihi = item.MakaleDegisimTarihi;
                }
                return View(makaleModelList);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        #endregion Yazar sayfaları


        #region Baş Editör sayfaları
        [AllowAnonymous]
        public ActionResult MakaleListeleBasEditor()
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                List<BasEditorMakaleListeleModel> makaleModelList = new List<BasEditorMakaleListeleModel>();
                List<Makaleler> makaleler = db.Makaleler.ToList();

                //Baş editörün makaleleri görüntüleyebilmesi için yeni bir model oluşturduk. 
                foreach (var item in makaleler)
                {
                    BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();
                    makaleModel.MakaleID = item.MakaleID;
                    makaleModel.MakaleBaslik = item.MakaleBaslik;
                    makaleModel.MakaleAciklama = item.MakaleAciklama;
                    makaleModel.MakaleDosyaYol = item.MakaleDosyaYol;
                    makaleModel.RevizyonIstenmisMi = RevizyonIstenmisMiBelirle(item.RevizyonIstenmisMi);
                    makaleModel.MakaleDurum = item.MakaleDurum;
                    makaleModel.Not1 = item.Not1;
                    makaleModel.Not2 = item.Not2;
                    makaleModel.Not3 = item.Not3;
                    makaleModel.NotOrt = item.NotOrt;
                    //Yazarın sadece id'si elimizde olduğu için o id'den yazar adı ve soyadına erişiyoruz.
                    var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                    if (yazar!=null)
                    {
                        makaleModel.YazarKullaniciAdi = yazar.YazarAd + " " + yazar.YazarSoyad;
                    }
                    //Hakem listesi Makaleler tablosunun Kisiler kolonunda string bir şekilde tutuluyor. Hakemlerin kişi id'leri aralarına virgül atılarak tutulmaktadır.
                    //O yüzden Makale tablosundan Kisiler bilgisi alınır. Virgüle göre parçalanarak id'ler bulunur. Sonra o id'ler veritabanından sorgulanarak id'lere ait
                    //hakemlerin ad ve soyadları bulunur.
                    //Model bu bilgiler ışığında doldurulur. Model, sayfaya gönderilecek olan model listesi ışığında doldurulur.
                    string[] hakemIDListString;
                    string hakemler = db.Makaleler.Where(a => a.Kisiler.Equals(item.Kisiler)).FirstOrDefault().Kisiler;
                    string hakemKullaniciAdiHepsi = "";
                    if (hakemler!= null)
                    {
                        hakemIDListString = hakemler.Split(',');
                        foreach (string hakemID in hakemIDListString)
                        {
                            int hakem_id = Convert.ToInt32(hakemID);
                            var hakem = db.Kisiler.Where(a => a.KisiID.Equals(hakem_id)).FirstOrDefault();
                            if (hakem != null)
                            {
                                hakemKullaniciAdiHepsi += hakem.KisiAd + " " + hakem.KisiSoyad + ",";
                            }
                            else
                            {
                                hakemKullaniciAdiHepsi += "";
                            }
                        }
                        hakemKullaniciAdiHepsi = hakemKullaniciAdiHepsi.Remove(hakemKullaniciAdiHepsi.Length - 1, 1);
                    }
                    makaleModel.Hakemler = hakemKullaniciAdiHepsi;

                    Kisiler alanEditoru = db.Kisiler.Where(a => a.KisiID== item.AlanEditoruIDFK).FirstOrDefault();
                    if(alanEditoru != null)
                    {
                        string alanEditoruAdiSoyadi = alanEditoru.KisiAd + " " + alanEditoru.KisiSoyad;
                        makaleModel.AlanEditoru = alanEditoruAdiSoyadi;
                    }
                    else
                    {
                        makaleModel.AlanEditoru = "";
                    }
                    makaleModelList.Add(makaleModel);
                    makaleModel.MakaleDegisimTarihi = item.MakaleDegisimTarihi;
                }
                return View(makaleModelList);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public ActionResult MakaleYonlendirBasEditor(int? id)
        {
            List<SelectListItem> alanEditorList = new List<SelectListItem>();
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                
                MakaleYonlendirBasEditor makaleYonlendirModel = new MakaleYonlendirBasEditor();

                if (id == null)
                {
                    Debug.WriteLine("id null geldi");

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Makaleler makale = db.Makaleler.Where(a => a.MakaleID == id).FirstOrDefault();
                if (makale == null)
                {
                    Debug.WriteLine("makales null geldi");
                    return HttpNotFound();
                }
                makaleYonlendirModel.MakaleID = makale.MakaleID;
                makaleYonlendirModel.MakaleBaslik = makale.MakaleBaslik;
                makaleYonlendirModel.MakaleAciklama = makale.MakaleAciklama;

                foreach (var item in db.Kisiler.Where(a => a.RolIDFK == (db.Roller.Where(b => b.RolAd == rolAlanEditoruAd).FirstOrDefault().RolID)).ToList())
                {
                    alanEditorList.Add(
                   new SelectListItem
                   {
                       Text = item.KisiAd + " " + item.KisiSoyad,
                       Value = item.KisiID.ToString(),
                   });
                }
               
                makaleYonlendirModel.MakaleID =Convert.ToInt32(id);
                ViewBag.AlanEditorleri = alanEditorList;
                 return View(makaleYonlendirModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult MakaleYonlendirBasEditor(MakaleYonlendirBasEditor makaleYonlendirModel)
        {
            //makaleYonlendirModel.AlanEditorleri seçilen alan editörünün id'sini getirir.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                if (ModelState.IsValid)
                {
                    var makale = db.Makaleler.Where(a => a.MakaleID == makaleYonlendirModel.MakaleID).FirstOrDefault();
                    if (makale!=null)
                    {
                        makale.AlanEditoruIDFK =Convert.ToInt32(makaleYonlendirModel.AlanEditorleri);
                    }
                    db.SaveChanges();
                    return RedirectToAction("MakaleListeleBasEditor");
                }
                return View(makaleYonlendirModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
            
        }

        [AllowAnonymous]
        public ActionResult KisiListeleBasEditor()
        {
            //Hakemler ve alan editörleri veri tabanından çekilir. Liste olarak sayfaya yansıtılır.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                List<BasEditorKisiListeleModel> listBasEditorKisiListeleModel = new List<BasEditorKisiListeleModel>();
                var alanEditorleri = db.Kisiler.Where(a => a.RolIDFK == (db.Roller.Where(b => b.RolAd == rolAlanEditoruAd).FirstOrDefault().RolID)).ToList();
                var hakemler = db.Kisiler.Where(a => a.RolIDFK == (db.Roller.Where(b => b.RolAd == rolHakemAd).FirstOrDefault().RolID)).ToList();
                foreach (var item in hakemler)
                {
                    alanEditorleri.Add(item);
                }
                int hakemRolId = db.Roller.Where(a => a.RolAd == rolHakemAd).FirstOrDefault().RolID;
                int alanEditoruRolId = db.Roller.Where(a => a.RolAd == rolAlanEditoruAd).FirstOrDefault().RolID;
                foreach (var item in alanEditorleri)
                {
                    BasEditorKisiListeleModel basEditorKisiListeleModel = new BasEditorKisiListeleModel();
                    basEditorKisiListeleModel.KisiID = item.KisiID;
                    basEditorKisiListeleModel.KisiAd = item.KisiAd;
                    basEditorKisiListeleModel.KisiSoyad = item.KisiSoyad;
                    basEditorKisiListeleModel.KisiEmail = item.KisiEmail;
                    basEditorKisiListeleModel.KisiSifre = item.KisiSifre;
                    basEditorKisiListeleModel.KisiSifreTekrar = item.KisiSifreTekrar;

                    if (item.RolIDFK == hakemRolId)
                        basEditorKisiListeleModel.RolAdi = rolHakemAd;
                    else if (item.RolIDFK == alanEditoruRolId)
                        basEditorKisiListeleModel.RolAdi = rolAlanEditoruAd;
                    listBasEditorKisiListeleModel.Add(basEditorKisiListeleModel);
                }
                return View(listBasEditorKisiListeleModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public ActionResult YeniKisiEkleBasEditor()
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                List<SelectListItem> rolList = new List<SelectListItem>();
                rolList.Add(new SelectListItem { Text = "Alan Editörü", Value = "3" });
                rolList.Add(new SelectListItem { Text = "Hakem", Value = "4" });
                ViewBag.Roller = rolList;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult YeniKisiEkleBasEditor(BasEditorKisiEkleModel kisiEkleModel)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                Kisiler kisi = new Kisiler();
                Debug.WriteLine("rolID : " + kisiEkleModel.RolAdi.ToString()); Debug.WriteLine("kisiAd : " + kisiEkleModel.KisiAd);
                kisi.KisiAd = kisiEkleModel.KisiAd;
                kisi.KisiSoyad = kisiEkleModel.KisiSoyad;
                kisi.KisiEmail = kisiEkleModel.KisiEmail;
                kisi.KisiSifre = kisiEkleModel.KisiSifre;
                kisi.KisiSifreTekrar = kisiEkleModel.KisiSifreTekrar;
                kisi.RolIDFK = Convert.ToInt32(kisiEkleModel.RolAdi);
                if (ModelState.IsValid)
                {
                    if(kisi.KisiSifre == kisi.KisiSifreTekrar)
                    {
                        db.Kisiler.Add(kisi);
                        db.SaveChanges();
                    }
                    else
                    {
                        TempData["ParolaKontrol"] = "<script>alert('Şifre ile Tekrar Şifre alanı aynı değildir. Kayıt yapılamamıştır. Tekrar deneyiniz.');</script>";
                        return View();
                    }
                }
                return RedirectToAction("KisiListeleBasEditor");
            }
            else
            {
                return RedirectToAction("Index");
            }
              
        }

        // GET: Deneme/Delete/5
        public ActionResult KisiSilBasEditor(int? id)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Kisiler kisiler = db.Kisiler.Find(id);
                if (kisiler == null)
                {
                    return HttpNotFound();
                }
                BasEditorKisiListeleModel basEditorKisiListeleModel = new BasEditorKisiListeleModel();
                basEditorKisiListeleModel.KisiID = kisiler.KisiID;
                basEditorKisiListeleModel.KisiAd = kisiler.KisiAd;
                basEditorKisiListeleModel.KisiSoyad = kisiler.KisiSoyad;
                basEditorKisiListeleModel.KisiEmail = kisiler.KisiEmail;
                basEditorKisiListeleModel.KisiSifre = kisiler.KisiSifre;
                basEditorKisiListeleModel.KisiSifreTekrar = kisiler.KisiSifreTekrar;
                string rol_adi = db.Roller.Where(a => a.RolID == kisiler.RolIDFK).FirstOrDefault().RolAd.ToString();
                basEditorKisiListeleModel.RolAdi = rol_adi;

                return View(basEditorKisiListeleModel);
            }
            else
            {
                return RedirectToAction("Index");

            }

        }
        
        [HttpPost, ActionName("KisiSilBasEditor")]
        [ValidateAntiForgeryToken]
        public ActionResult KisiSilBasEditorConfirmed(int id)
        {
            Debug.WriteLine("KisiSilBasEditorConfirmed id : " + id.ToString());
            Kisiler kisi = db.Kisiler.Find(id);
            db.Kisiler.Remove(kisi);
            db.SaveChanges();
            return RedirectToAction("KisiListeleBasEditor");
        }

        [AllowAnonymous]
        public ActionResult MakaleOnerideBulunBasEditor(int? id)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {

                BasEditorDergiOnerModel makaleOnerideBulunModel = new BasEditorDergiOnerModel();

                if (id == null)
                {
                    Debug.WriteLine("id null geldi");

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Makaleler makale = db.Makaleler.Where(a => a.MakaleID == id).FirstOrDefault();
                if (makale == null)
                {
                    Debug.WriteLine("makales null geldi");
                    return HttpNotFound();
                }
                makaleOnerideBulunModel.MakaleID = makale.MakaleID;
                return View(makaleOnerideBulunModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult MakaleOnerideBulunBasEditor(BasEditorDergiOnerModel makaleOnerideBulunModel)
        {
            //makaleYonlendirModel.AlanEditorleri seçilen alan editörünün id'sini getirir.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolBasEditorAd)
            {
                if (ModelState.IsValid)
                {
                    var makale = db.Makaleler.Where(a => a.MakaleID == makaleOnerideBulunModel.MakaleID).FirstOrDefault();
                    var yazar = db.Yazarlar.Where(a => a.YazarID == makale.YazarIDFK).FirstOrDefault();
                   
                    if (makale != null)
                    {

                        string mailSubject = "Makale Dergi Önerisi";
                        string mailBody = "<html><body>Sayın"
                            + yazar.YazarAd + " " + yazar.YazarSoyad+ ",<br>"
                            + makale.MakaleBaslik + " başlıklı makaleniz bizim dergide yayınlanmaya uygun değildir. Ancak size bu makaleyi yayınlamanız için " 
                            + makaleOnerideBulunModel.Dergi1 +  " ve " + makaleOnerideBulunModel.Dergi2 + " dergilerini önerebilirim. Önerim şudur : <br>" 
                            + "<br>" + makaleOnerideBulunModel.Oneri 
                            + "<br><br>"
                            + "İyi çalışmalar,<br>"
                            + Session["KullaniciAdi"].ToString() + "<br>"
                            + "</body></html>";
                        MailAt(yazar.YazarEmail, mailSubject, mailBody);
                    }
                    db.SaveChanges();
                    return RedirectToAction("MakaleListeleBasEditor");
                }
                return RedirectToAction("MakaleListeleBasEditor");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }


        public string RevizyonIstenmisMiBelirle(int _revizyonIstenmisMi)
        {
            if (_revizyonIstenmisMi == 0)
            {
                return "Hayır";
            }
            else if(_revizyonIstenmisMi == 1)
            {
                return "Evet";
            }
            return "Hayır";
        }
        #endregion Baş Editör sayfaları


        #region Alan Editörü sayfaları

        [AllowAnonymous]
        public ActionResult HakemListeleAlanEditoru()
        {
            //Hakemler ve alan editörleri veri tabanından çekilir. Liste olarak sayfaya yansıtılır.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {
                List<BasEditorKisiListeleModel> listBasEditorKisiListeleModel = new List<BasEditorKisiListeleModel>();
                var hakemler = db.Kisiler.Where(a => a.RolIDFK == (db.Roller.Where(b => b.RolAd == rolHakemAd).FirstOrDefault().RolID)).ToList();
                 
                int hakemRolId = db.Roller.Where(a => a.RolAd == rolHakemAd).FirstOrDefault().RolID;
                foreach (var item in hakemler)
                {
                    BasEditorKisiListeleModel basEditorKisiListeleModel = new BasEditorKisiListeleModel();
                    basEditorKisiListeleModel.KisiID = item.KisiID;
                    basEditorKisiListeleModel.KisiAd = item.KisiAd;
                    basEditorKisiListeleModel.KisiSoyad = item.KisiSoyad;
                    basEditorKisiListeleModel.KisiEmail = item.KisiEmail;
                    basEditorKisiListeleModel.KisiSifre = item.KisiSifre;
                    basEditorKisiListeleModel.KisiSifreTekrar = item.KisiSifreTekrar;

                    if (item.RolIDFK == hakemRolId)
                        basEditorKisiListeleModel.RolAdi = rolHakemAd;
                    listBasEditorKisiListeleModel.Add(basEditorKisiListeleModel);
                }
                return View(listBasEditorKisiListeleModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public ActionResult YeniHakemEkleAlanEditoru()
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {
                List<SelectListItem> rolList = new List<SelectListItem>();
                rolList.Add(new SelectListItem { Text = "Hakem", Value = "4" });
                ViewBag.Roller = rolList;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult YeniHakemEkleAlanEditoru(BasEditorKisiEkleModel kisiEkleModel)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {
                Kisiler kisi = new Kisiler();
                kisi.KisiAd = kisiEkleModel.KisiAd;
                kisi.KisiSoyad = kisiEkleModel.KisiSoyad;
                kisi.KisiEmail = kisiEkleModel.KisiEmail;
                kisi.KisiSifre = kisiEkleModel.KisiSifre;
                kisi.KisiSifreTekrar = kisiEkleModel.KisiSifreTekrar;
                kisi.RolIDFK = Convert.ToInt32(kisiEkleModel.RolAdi);
                if (ModelState.IsValid)
                {
                    if(kisi.KisiSifre == kisi.KisiSifreTekrar)
                    {
                        db.Kisiler.Add(kisi);
                        db.SaveChanges();
                    }
                    else
                    {
                        TempData["ParolaKontrol"] = "<script>alert('Şifre ile Tekrar Şifre alanı aynı değildir. Kayıt yapılamamıştır. Tekrar deneyiniz.');</script>";
                        return View();
                    }
                }
                return RedirectToAction("HakemListeleAlanEditoru");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        // GET: Deneme/Delete/5
        public ActionResult HakemSilAlanEditoru(int? id)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Kisiler kisiler = db.Kisiler.Find(id);
                if (kisiler == null)
                {
                    return HttpNotFound();
                }
                BasEditorKisiListeleModel basEditorKisiListeleModel = new BasEditorKisiListeleModel();
                basEditorKisiListeleModel.KisiID = kisiler.KisiID;
                basEditorKisiListeleModel.KisiAd = kisiler.KisiAd;
                basEditorKisiListeleModel.KisiSoyad = kisiler.KisiSoyad;
                basEditorKisiListeleModel.KisiEmail = kisiler.KisiEmail;
                basEditorKisiListeleModel.KisiSifre = kisiler.KisiSifre;
                basEditorKisiListeleModel.KisiSifreTekrar = kisiler.KisiSifreTekrar;
                string rol_adi = db.Roller.Where(a => a.RolID == kisiler.RolIDFK).FirstOrDefault().RolAd.ToString();
                basEditorKisiListeleModel.RolAdi = rol_adi;
                return View(basEditorKisiListeleModel);
            }
            else
            {
                return RedirectToAction("Index");

            }

        }

        [HttpPost, ActionName("HakemSilAlanEditoru")]
        [ValidateAntiForgeryToken]
        public ActionResult HakemSilAlanEditoruConfirmed(int id)
        {
            Kisiler kisi = db.Kisiler.Find(id);
            db.Kisiler.Remove(kisi);
            db.SaveChanges();
            return RedirectToAction("HakemListeleAlanEditoru");
        }


        [AllowAnonymous]
        public ActionResult MakaleListeleAlanEditoru()
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {
                List<BasEditorMakaleListeleModel> makaleModelList = new List<BasEditorMakaleListeleModel>();
                //Alan editörü makaleleri listelediğinde sadece kendisine atanmış olan makaleleri görecektir.
                string sessionKullaniciEmail = Session["KullaniciEmail"].ToString();
                int alan_editoru_id_fk = db.Kisiler.Where(a => a.KisiEmail == sessionKullaniciEmail).FirstOrDefault().KisiID;
                List<Makaleler> makaleler = db.Makaleler.Where(a => a.AlanEditoruIDFK == alan_editoru_id_fk).ToList();

                //Alan editörünün makaleleri görüntüleyebilmesi için daha önceden baş editör için oluşturmuş olduğumuz BasEditorMakaleListeleModel modelini kullandık. 
                foreach (var item in makaleler)
                {
                    BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();
                    makaleModel.MakaleID = item.MakaleID;
                    makaleModel.MakaleBaslik = item.MakaleBaslik;
                    makaleModel.MakaleAciklama = item.MakaleAciklama;
                    makaleModel.MakaleDosyaYol = item.MakaleDosyaYol;
                    makaleModel.RevizyonIstenmisMi = RevizyonIstenmisMiBelirle(item.RevizyonIstenmisMi);
                    makaleModel.MakaleDurum = item.MakaleDurum;
                    makaleModel.Not1 = item.Not1;
                    makaleModel.Not2 = item.Not2;
                    makaleModel.Not3 = item.Not3;
                    makaleModel.NotOrt = item.NotOrt;
                    //Yazarın sadece id'si elimizde olduğu için o id'den yazar adı ve soyadına erişiyoruz.
                    var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                    if (yazar != null)
                    {
                        makaleModel.YazarKullaniciAdi = yazar.YazarAd + " " + yazar.YazarSoyad;
                    }
                    //Hakem listesi Makaleler tablosunun Kisiler kolonunda string bir şekilde tutuluyor. Hakemlerin kişi id'leri aralarına virgül atılarak tutulmaktadır.
                    //O yüzden Makale tablosundan Kisiler bilgisi alınır. Virgüle göre parçalanarak id'ler bulunur. Sonra o id'ler veritabanından sorgulanarak id'lere ait
                    //hakemlerin ad ve soyadları bulunur.
                    //Model bu bilgiler ışığında doldurulur. Model, sayfaya gönderilecek olan model listesi ışığında doldurulur.
                    string[] hakemIDListString;
                    string hakemler = db.Makaleler.Where(a => a.Kisiler.Equals(item.Kisiler)).FirstOrDefault().Kisiler;
                    string hakemKullaniciAdiHepsi = "";
                    if (hakemler!= null)
                    {
                        hakemIDListString = hakemler.Split(',');
                        foreach (string hakemID in hakemIDListString)
                        {
                            int hakem_id = Convert.ToInt32(hakemID);
                            var hakem = db.Kisiler.Where(a => a.KisiID.Equals(hakem_id)).FirstOrDefault();
                            if (hakem != null)
                            {
                                hakemKullaniciAdiHepsi += hakem.KisiAd + " " + hakem.KisiSoyad + ",";
                            }
                            else
                            {
                                hakemKullaniciAdiHepsi += "";
                            }
                        }
                        hakemKullaniciAdiHepsi = hakemKullaniciAdiHepsi.Remove(hakemKullaniciAdiHepsi.Length - 1, 1);
                    }
                    makaleModel.Hakemler = hakemKullaniciAdiHepsi;

                    Kisiler alanEditoru = db.Kisiler.Where(a => a.KisiID == item.AlanEditoruIDFK).FirstOrDefault();
                    if (alanEditoru != null)
                    {
                        string alanEditoruAdiSoyadi = alanEditoru.KisiAd + " " + alanEditoru.KisiSoyad;
                        makaleModel.AlanEditoru = alanEditoruAdiSoyadi;
                    }
                    else
                    {
                        makaleModel.AlanEditoru = "";
                    }
                    makaleModelList.Add(makaleModel);
                    makaleModel.MakaleDegisimTarihi = item.MakaleDegisimTarihi;
                }
                return View(makaleModelList);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [AllowAnonymous]
        public ActionResult MakaleYonlendirAlanEditoru(int? id)
        {
            List<SelectListItem> hakemList = new List<SelectListItem>();
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {

                MakaleYonlendirAlanEditoru makaleYonlendirModel = new MakaleYonlendirAlanEditoru();

                if (id == null)
                {
                    Debug.WriteLine("id null geldi");

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Makaleler makale = db.Makaleler.Where(a => a.MakaleID == id).FirstOrDefault();
                if (makale == null)
                {
                    Debug.WriteLine("makales null geldi");
                    return HttpNotFound();
                }
                makaleYonlendirModel.MakaleID = makale.MakaleID;
                makaleYonlendirModel.MakaleBaslik = makale.MakaleBaslik;
                makaleYonlendirModel.MakaleAciklama = makale.MakaleAciklama;

                foreach (var item in db.Kisiler.Where(a => a.RolIDFK == (db.Roller.Where(b => b.RolAd == rolHakemAd).FirstOrDefault().RolID)).ToList())
                {
                    hakemList.Add(
                   new SelectListItem
                   {
                       Text = item.KisiAd + " " + item.KisiSoyad,
                       Value = item.KisiID.ToString(),
                   });
                }
                makaleYonlendirModel.MakaleID = Convert.ToInt32(id);
                //Dropdownlist'e veri ViewBag üzerinden yollanır.
                ViewBag.Hakemler1 = hakemList;
                ViewBag.Hakemler2 = hakemList;
                ViewBag.Hakemler3 = hakemList;

                return View(makaleYonlendirModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult MakaleYonlendirAlanEditoru(MakaleYonlendirAlanEditoru makaleYonlendirModel)
        {
            //makaleYonlendirModel.Hakemler1 seçilen hakemin id'sini getirir.
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolAlanEditoruAd)
            {
                if (ModelState.IsValid)
                {
                    var makale = db.Makaleler.Where(a => a.MakaleID == makaleYonlendirModel.MakaleID).FirstOrDefault();
                    if (makale != null)
                    {
                        makale.Kisiler = makaleYonlendirModel.Hakemler1.ToString() + "," + makaleYonlendirModel.Hakemler2.ToString() + "," + makaleYonlendirModel.Hakemler3.ToString();
                        makale.MakaleDegisimTarihi = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                        makale.MakaleDurum = "Alan editörü hakemlere davet gönderdi. Hakemlerden rapor bekleniyor.";
                    }
                    db.SaveChanges();
                    Debug.WriteLine("hakemler1 : " + makaleYonlendirModel.Hakemler1);
                    int hakemler1Int= Convert.ToInt32(makaleYonlendirModel.Hakemler1), hakemler2Int= Convert.ToInt32(makaleYonlendirModel.Hakemler2), hakemler3Int = Convert.ToInt32(makaleYonlendirModel.Hakemler3);
                    var hakem1 = db.Kisiler.Where(a => a.KisiID == hakemler1Int).FirstOrDefault();
                    var hakem2 = db.Kisiler.Where(a => a.KisiID == hakemler2Int).FirstOrDefault();
                    var hakem3 = db.Kisiler.Where(a => a.KisiID == hakemler3Int).FirstOrDefault();

                    string mailSubject = "Makale İnceleme Daveti";
                    string mailBody = "<html><body>Sayın" 
                        + hakem1.KisiAd + " " + hakem1.KisiSoyad + ",<br>"
                        + makale.MakaleBaslik + " başlıklı makaleye hakem olarak davet edildiniz. İlgili davete, Makale Değerlendirme Sistemi'ndeki sayfanız üzerinden 10 gün içinde olumlu ya da olumsuz bir cevap vermeniz beklenmektedir. Lütfen en kısa zamanda cevap veriniz.<br>"
                        + "İyi çalışmalar,<br>"
                        +Session["KullaniciAdi"].ToString()+"<br>"
                        + "</body></html>";
                    MailAt(hakem1.KisiEmail, mailSubject, mailBody);

                    mailBody = "<html><body>"
                        + hakem2.KisiAd + " " + hakem2.KisiSoyad + ",<br>"
                        + makale.MakaleBaslik + " başlıklı makaleye hakem olarak davet edildiniz. İlgili davete, Makale Değerlendirme Sistemi'ndeki sayfanız üzerinden 10 gün içinde olumlu ya da olumsuz bir cevap vermeniz beklenmektedir. Lütfen en kısa zamanda cevap veriniz.<br>"
                        + "İyi çalışmalar,<br>"
                        + Session["KullaniciAdi"].ToString() + "<br>"
                        + "</body></html>";
                    MailAt(hakem2.KisiEmail, mailSubject, mailBody);

                    mailBody = "<html><body>"
                     + hakem3.KisiAd + " " + hakem3.KisiSoyad + ",<br>"
                     + makale.MakaleBaslik + " başlıklı makaleye hakem olarak davet edildiniz. İlgili davete, Makale Değerlendirme Sistemi'ndeki sayfanız üzerinden 10 gün içinde olumlu ya da olumsuz bir cevap vermeniz beklenmektedir. Lütfen en kısa zamanda cevap veriniz.<br>"
                     + "İyi çalışmalar,<br>"
                     + Session["KullaniciAdi"].ToString() + "<br>"
                     + "</body></html>";
                    MailAt(hakem3.KisiEmail, mailSubject, mailBody);
                    return RedirectToAction("MakaleListeleAlanEditoru");
                }
                return View(makaleYonlendirModel);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        #endregion Alan Editörü sayfaları



        #region Hakem Sayfaları

        [AllowAnonymous]
        public ActionResult DavetleriListeleHakem()
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolHakemAd)
            {
                List<BasEditorMakaleListeleModel> makaleModelList = new List<BasEditorMakaleListeleModel>();
                //Makalenin Kisiler kolonunda hakemlerin id'leri aralarına virgül atılarak koyulmuştur. Kisiler kolonu hakemin id'sini içeriyorsa o makale ilgili hakeme ait demektir ve
                //listelenmesi gerekmektedir. Önce email session'ından hakem id'si bulunur. Sonra o hakem_id Makaleler tablosunun Kisiler kolonunda aratılır.
                string hakem_email = Session["KullaniciEmail"].ToString();
                string loginOlanHakemID = db.Kisiler.Where(a => a.KisiEmail == hakem_email).FirstOrDefault().KisiID.ToString();
                List<Makaleler> makaleler = db.Makaleler.Where(a => a.Kisiler.Contains(loginOlanHakemID)).ToList();

                //Baş editörün makaleleri görüntüleyebilmesi için yeni bir model oluşturduk. 
                foreach (var item in makaleler)
                {
                    BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();
                    makaleModel.MakaleID = item.MakaleID;
                    makaleModel.MakaleBaslik = item.MakaleBaslik;
                    makaleModel.MakaleAciklama = item.MakaleAciklama;
                    makaleModel.MakaleDosyaYol = item.MakaleDosyaYol;
                    makaleModel.RevizyonIstenmisMi = RevizyonIstenmisMiBelirle(item.RevizyonIstenmisMi);
                    makaleModel.MakaleDurum = item.MakaleDurum;
                    makaleModel.Not1 = item.Not1;
                    makaleModel.Not2 = item.Not2;
                    makaleModel.Not3 = item.Not3;
                    makaleModel.NotOrt = item.NotOrt;
                    //Yazarın sadece id'si elimizde olduğu için o id'den yazar adı ve soyadına erişiyoruz.
                    var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                    if (yazar != null)
                    {
                        makaleModel.YazarKullaniciAdi = yazar.YazarAd + " " + yazar.YazarSoyad;
                    }
                    //Hakem listesi Makaleler tablosunun Kisiler kolonunda string bir şekilde tutuluyor. Hakemlerin kişi id'leri aralarına virgül atılarak tutulmaktadır.
                    //O yüzden Makale tablosundan Kisiler bilgisi alınır. Virgüle göre parçalanarak id'ler bulunur. Sonra o id'ler veritabanından sorgulanarak id'lere ait
                    //hakemlerin ad ve soyadları bulunur.
                    //Model bu bilgiler ışığında doldurulur. Model, sayfaya gönderilecek olan model listesi ışığında doldurulur.
                    string[] hakemIDListString;
                    string hakemler = db.Makaleler.Where(a => a.Kisiler.Equals(item.Kisiler)).FirstOrDefault().Kisiler;
                    string hakemKullaniciAdiHepsi = "";
                    if (hakemler != null)
                    {
                        hakemIDListString = hakemler.Split(',');
                        foreach (string hakemID in hakemIDListString)
                        {
                            int hakem_id = Convert.ToInt32(hakemID);
                            var hakem = db.Kisiler.Where(a => a.KisiID.Equals(hakem_id)).FirstOrDefault();
                            if (hakem != null)
                            {
                                hakemKullaniciAdiHepsi += hakem.KisiAd + " " + hakem.KisiSoyad + ",";
                            }
                            else
                            {
                                hakemKullaniciAdiHepsi += "";
                            }
                        }
                        hakemKullaniciAdiHepsi = hakemKullaniciAdiHepsi.Remove(hakemKullaniciAdiHepsi.Length - 1, 1);
                    }
                    makaleModel.Hakemler = hakemKullaniciAdiHepsi;

                    Kisiler alanEditoru = db.Kisiler.Where(a => a.KisiID == item.AlanEditoruIDFK).FirstOrDefault();
                    if (alanEditoru != null)
                    {
                        string alanEditoruAdiSoyadi = alanEditoru.KisiAd + " " + alanEditoru.KisiSoyad;
                        makaleModel.AlanEditoru = alanEditoruAdiSoyadi;
                    }
                    else
                    {
                        makaleModel.AlanEditoru = "";
                    }
                    makaleModelList.Add(makaleModel);
                    makaleModel.MakaleDegisimTarihi = item.MakaleDegisimTarihi;
                }
                return View(makaleModelList);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [AllowAnonymous]
        public ActionResult DavetiKabulEtHakem(int ?id)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolHakemAd)
            {
                string loginOlanEmail = Session["KullaniciEmail"].ToString();
                var loginOlanHakem = db.Kisiler.Where(a => a.KisiEmail == loginOlanEmail).FirstOrDefault();
                //Makaleden alan editörü bulundu. Alan editörüne davetin kabul edileceğine dair mail atılacak.
                var makale = db.Makaleler.Find(id);
                var alanEditoru = db.Kisiler.Where(a => a.KisiID == makale.AlanEditoruIDFK).FirstOrDefault();
                string subject = "Daveti Kabul Etme";
                string mailBody = "<html><body>"
                    + "Merhaba,<br>"
                    + "Davetinizi kabul etmekten mutluluk duyarım. <br>"
                    + "İyi çalışmalar<br>"
                    + loginOlanHakem.KisiAd + " " + loginOlanHakem.KisiSoyad
                    + "</body></html>";
                MailAt(alanEditoru.KisiEmail, subject, mailBody);
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [AllowAnonymous]
        public ActionResult DavetiReddetHakem(int? id)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolHakemAd)
            {
                string loginOlanEmail = Session["KullaniciEmail"].ToString();
                var loginOlanHakem = db.Kisiler.Where(a => a.KisiEmail == loginOlanEmail).FirstOrDefault();
                //Makaleden alan editörü bulundu. Alan editörüne davetin kabul edileceğine dair mail atılacak.
                var makale = db.Makaleler.Find(id);
                string[] kisilerDizi = makale.Kisiler.Split(',');
                string hakemIDler = "";
                for (int i = 0; i < kisilerDizi.Length; i++)
                {
                    if (kisilerDizi[i] != loginOlanHakem.KisiID.ToString())
                    {
                        hakemIDler += kisilerDizi[i].ToString() + ",";
                    }
                }
                hakemIDler = hakemIDler.Remove(hakemIDler.Length - 1, 1);
                makale.Kisiler = hakemIDler;
                db.SaveChanges();
                var alanEditoru = db.Kisiler.Where(a => a.KisiID == makale.AlanEditoruIDFK).FirstOrDefault();
                string subject = "Daveti Reddetme";
                string mailBody = "<html><body>"
                    + "Merhaba,<br>"
                    + "Davetinizi reddetmekten üzüntü duyarım. <br>"
                    + "İyi çalışmalar<br>"
                    + loginOlanHakem.KisiAd + " " + loginOlanHakem.KisiSoyad
                    + "</body></html>";
                MailAt(alanEditoru.KisiEmail, subject, mailBody);
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [AllowAnonymous]
        public ActionResult MakaleleleriListeleHakem(BasEditorMakaleListeleModel makaleListeleModel)
        {
            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolHakemAd)
            {
                string loginOlanEmail = Session["KullaniciEmail"].ToString();
                var loginOlanHakem = db.Kisiler.Where(a => a.KisiEmail == loginOlanEmail).FirstOrDefault();
                string loginOlanHakemID = loginOlanHakem.KisiID.ToString();

                List<BasEditorMakaleListeleModel> makaleModelList = new List<BasEditorMakaleListeleModel>();
                List<Makaleler> makaleler = db.Makaleler.Where(a => a.Kisiler.Contains(loginOlanHakemID)).ToList();

                //Baş editörün makaleleri görüntüleyebilmesi için yeni bir model oluşturduk. 
                foreach (var item in makaleler)
                {
                    BasEditorMakaleListeleModel makaleModel = new BasEditorMakaleListeleModel();
                    makaleModel.MakaleID = item.MakaleID;
                    makaleModel.MakaleBaslik = item.MakaleBaslik;
                    makaleModel.MakaleAciklama = item.MakaleAciklama;
                    makaleModel.MakaleDosyaYol = item.MakaleDosyaYol;
                    makaleModel.RevizyonIstenmisMi = RevizyonIstenmisMiBelirle(item.RevizyonIstenmisMi);
                    makaleModel.MakaleDurum = item.MakaleDurum;
                    makaleModel.Not1 = item.Not1;
                    makaleModel.Not2 = item.Not2;
                    makaleModel.Not3 = item.Not3;
                    makaleModel.NotOrt = item.NotOrt;
                    //Yazarın sadece id'si elimizde olduğu için o id'den yazar adı ve soyadına erişiyoruz.
                    var yazar = db.Yazarlar.Where(a => a.YazarID == item.YazarIDFK).FirstOrDefault();
                    if (yazar != null)
                    {
                        makaleModel.YazarKullaniciAdi = yazar.YazarAd + " " + yazar.YazarSoyad;
                    }
                    //Hakem listesi Makaleler tablosunun Kisiler kolonunda string bir şekilde tutuluyor. Hakemlerin kişi id'leri aralarına virgül atılarak tutulmaktadır.
                    //O yüzden Makale tablosundan Kisiler bilgisi alınır. Virgüle göre parçalanarak id'ler bulunur. Sonra o id'ler veritabanından sorgulanarak id'lere ait
                    //hakemlerin ad ve soyadları bulunur.
                    //Model bu bilgiler ışığında doldurulur. Model, sayfaya gönderilecek olan model listesi ışığında doldurulur.
                    string[] hakemIDListString;
                    string hakemler = db.Makaleler.Where(a => a.Kisiler.Equals(item.Kisiler)).FirstOrDefault().Kisiler;
                    string hakemKullaniciAdiHepsi = "";
                    if (hakemler != null)
                    {
                        hakemIDListString = hakemler.Split(',');
                        foreach (string hakemID in hakemIDListString)
                        {
                            int hakem_id = Convert.ToInt32(hakemID);
                            var hakem = db.Kisiler.Where(a => a.KisiID.Equals(hakem_id)).FirstOrDefault();
                            if (hakem != null)
                            {
                                hakemKullaniciAdiHepsi += hakem.KisiAd + " " + hakem.KisiSoyad + ",";
                            }
                            else
                            {
                                hakemKullaniciAdiHepsi += "";
                            }
                        }
                        hakemKullaniciAdiHepsi = hakemKullaniciAdiHepsi.Remove(hakemKullaniciAdiHepsi.Length - 1, 1);
                    }
                    makaleModel.Hakemler = hakemKullaniciAdiHepsi;

                    Kisiler alanEditoru = db.Kisiler.Where(a => a.KisiID == item.AlanEditoruIDFK).FirstOrDefault();
                    if (alanEditoru != null)
                    {
                        string alanEditoruAdiSoyadi = alanEditoru.KisiAd + " " + alanEditoru.KisiSoyad;
                        makaleModel.AlanEditoru = alanEditoruAdiSoyadi;
                    }
                    else
                    {
                        makaleModel.AlanEditoru = "";
                    }
                    makaleModel.MakaleDegisimTarihi = item.MakaleDegisimTarihi;
                    makaleModelList.Add(makaleModel);
                }
                return View(makaleModelList);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [AllowAnonymous]
        public ActionResult MakaleDegerlendirHakem(int? id)
        {
            Debug.WriteLine("id : " + id.ToString());

            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolHakemAd)
            {
                HakemMakaleDegerlendirModel hakemDegerlendirModel = new HakemMakaleDegerlendirModel();
                hakemDegerlendirModel.MakaleID =Convert.ToInt32(id);
                return View(hakemDegerlendirModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult MakaleDegerlendirHakem(HakemMakaleDegerlendirModel makaleModel)
        {

            Debug.WriteLine("id2 : " + makaleModel.MakaleID.ToString());

            if (Session["KullaniciRol"] != null && Session["KullaniciRol"].ToString() == rolHakemAd)
            {
                //Login olan hakemi buluruz.
                string loginOlanHakemEmail = Session["KullaniciEmail"].ToString();
                int loginOlanHakemID = db.Kisiler.Where(a => a.KisiEmail == loginOlanHakemEmail).FirstOrDefault().KisiID;
                string loginOlanHakemAdSoyad = db.Kisiler.Where(a => a.KisiEmail == loginOlanHakemEmail).FirstOrDefault().KisiAd + " " +
                    db.Kisiler.Where(a => a.KisiEmail == loginOlanHakemEmail).FirstOrDefault().KisiSoyad;
                var makale = db.Makaleler.Where(a => a.MakaleID == makaleModel.MakaleID).FirstOrDefault();
                Debug.WriteLine("revizyon isteği : " + makaleModel.RevizyonIstegi);
                if (String.IsNullOrEmpty(makaleModel.RevizyonIstegi))
                {
                    //Eğer revizyon isteği yapılmamışsa makale ile ilgili değişiklikler tabloya kaydedilir.
                    makale.RevizyonIstenmisMi = 0;
                    string dosyaAdi = Path.GetFileName(makaleModel.HakemDegerlendirmeRaporuYol.FileName);
                    FileInfo ff = new FileInfo(makaleModel.HakemDegerlendirmeRaporuYol.FileName);
                    string dosyaUzantisi = ff.Extension;
                    if (dosyaUzantisi != ".pdf" && dosyaUzantisi != ".PDF")
                    {
                        TempData["dosyaUzantisi"] = "<script>alert('Sadece pdf uzantılı dosyalarla yarışmaya başvurabilirsiniz.');</script>";
                        return View();
                    }
                    if (dosyaAdi.Contains(dosyaUzantisi))
                    {
                        dosyaAdi = dosyaAdi.Replace(dosyaUzantisi, "");
                    }
                    if (dosyaAdi.Length > 40)
                    {
                        dosyaAdi = dosyaAdi.Substring(0,10);
                    }
                    dosyaAdi = dosyaAdi + "-" + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + dosyaUzantisi;
                    Debug.WriteLine("dosyaAdi : " + dosyaAdi);
                    makaleModel.HakemDegerlendirmeRaporuYol.SaveAs(@"C:\Users\yasü\Documents\Visual Studio 2015\Projects\MakaleDegerlendirmeSistemi\MakaleDegerlendirmeSistemi\Content\uploadPDF\" + dosyaAdi);
                    //Makaleler tablosunun Kisiler kolonuna kaydedilen hakem id'sinin kaçıncı sırada olduğu bulunur. Kaçıncı hakemse değerlendirme raporu o hakemin kolonuna kaydedilir. Not
                    // da bu şekilde olacaktır.
                    int kacinciHakem = 0;
                    string[] kisilerDizi = makale.Kisiler.Split(',');
                    kacinciHakem = Array.IndexOf(kisilerDizi, loginOlanHakemID.ToString())+1;
                    Debug.WriteLine("makaleModel.Not : " + makaleModel.Not);
                    if (kacinciHakem == 1)
                    {
                        makale.Hakem1DegerlendirmeRaporuYol = dosyaAdi;
                        makale.Not1 = makaleModel.Not;
                    }
                    else if (kacinciHakem == 2)
                    {
                        makale.Hakem2DegerlendirmeRaporuYol = dosyaAdi;
                        makale.Not2 = makaleModel.Not;
                    }
                    else if (kacinciHakem == 3)
                    {
                        makale.Hakem3DegerlendirmeRaporuYol = dosyaAdi;
                        makale.Not3 = makaleModel.Not;
                    }
                    makale.NotOrt = (makale.Not1 + makale.Not2 + makale.Not3) / 3;
                    makale.MakaleDurum = "Hakemlerden değerlendirme raporları gelmeye başladı. ";
                    makale.MakaleDegisimTarihi = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    
                }
                else
                {
                    makale.RevizyonIstenmisMi = 1;

                    //Revizyon isteği yapılmışsa alan editörüne revizyon maili gider. 
                    string alan_editoru_email = db.Kisiler.Where(a => a.KisiID == makale.AlanEditoruIDFK).FirstOrDefault().KisiEmail;
                    string alan_editoru_ad_soyad = db.Kisiler.Where(a => a.KisiID == makale.AlanEditoruIDFK).FirstOrDefault().KisiAd + " "
                        + db.Kisiler.Where(a => a.KisiID == makale.AlanEditoruIDFK).FirstOrDefault().KisiSoyad;
                    int kacinciHakem = 0;
                    string[] kisilerDizi = makale.Kisiler.Split(',');
                    kacinciHakem = Array.IndexOf(kisilerDizi, loginOlanHakemID.ToString()) + 1;
                    if (kacinciHakem == 1)
                    {
                        makale.Hakem1DegerlendirmeRaporuYol = "";
                        makale.Not1 = -1;
                    }
                    else if (kacinciHakem == 2)
                    {
                        makale.Hakem2DegerlendirmeRaporuYol = "";
                        makale.Not2 = -1;
                    }
                    else if (kacinciHakem == 3)
                    {
                        makale.Hakem3DegerlendirmeRaporuYol = "";
                        makale.Not3 =-1;
                    }
                    makale.NotOrt = (makale.Not1 + makale.Not2 + makale.Not3) / 3;
                    makale.MakaleDurum = "Hakemlerden revizyon isteği geldi. Revizyon isteyen hakem : " + loginOlanHakemAdSoyad;
                    makale.MakaleDegisimTarihi = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    string mailSubject = "Makale Revizyon İsteği";
                    string mailBody = "<html><body>Sayın "
                        + alan_editoru_ad_soyad + ",<br>"
                        + makale.MakaleBaslik + " başlıklı makaleyle ilgili revizyon isteğim vardır. Lütfen yazara iletiniz.<br> Revizyon İsteği : <br>"
                        + makaleModel.RevizyonIstegi +"<br>"
                        + "İyi çalışmalar,<br>"
                        + loginOlanHakemAdSoyad + "<br>"
                        + "</body></html>";
                    MailAt(alan_editoru_email, mailSubject, mailBody);
                }
                db.SaveChanges();
                return RedirectToAction("MakaleleleriListeleHakem");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        #endregion Hakem Sayfaları

        public void SessionBelirle(string sessionKullaniciAdi, string sessionRol,string sessionEmail,string link1,string link2,string link3,string link4,string link5,string linkYazi1,
            string linkYazi2, string linkYazi3, string linkYazi4, string linkYazi5)
        {
            Session["KullaniciAdi"] = sessionKullaniciAdi;
            Session["KullaniciRol"] = sessionRol;
            Session["KullaniciEmail"] = sessionEmail;
            Session["Link1"] = link1;
            Session["LinkYazi1"] = linkYazi1;
            Session["Link2"] = link2;
            Session["LinkYazi2"] = linkYazi2;
            Session["Link3"] = link3;
            Session["LinkYazi3"] = linkYazi3;
            Session["Link4"] = link4;
            Session["LinkYazi4"] = linkYazi4;
            Session["Link5"] = link5;
            Session["LinkYazi5"] = linkYazi5;
        }
        public void MailAt(string _gonderilecekMailAdresi,string _subject,string _body)
        {
            var client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.Credentials = new NetworkCredential("rabis19691969@gmail.com", "Rabis1969-");
            client.EnableSsl = true;

            MailMessage msg = new MailMessage("rabis19691969@gmail.com", _gonderilecekMailAdresi, _subject, _body);
            msg.IsBodyHtml = true;


            client.Send(msg);
        }
    }
}