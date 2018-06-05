using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fuar_Organizasyon.Models; //veritabanı erişim için
using System.Web.Security;      //md5 şifreleme için

namespace Fuar_Organizasyon.Controllers
{
    public class HomeController : Controller
    {
        //veritabanı tanımlandı
        FuarDatabaseEntities veritabani;

        public HomeController()
        {
            //veritabanı nesnesi oluşturuldu
            veritabani = new FuarDatabaseEntities();
        }
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            var egitim = veritabani.EgitimTablo.ToList();
            Random rnd = new Random();
            foreach (var item in egitim)
            {
                EgitimTablo et = new EgitimTablo();
                et.firmaid = item.firmaid;
                et.firmaadi = item.firmaadi;

                et.C1 = item.C1 = rnd.Next(11);
                et.C2 = item.C2 = rnd.Next(11);
                et.C3 = item.C3 = rnd.Next(11);
                et.C4 = item.C4 = rnd.Next(11);
                et.C5 = item.C5 = rnd.Next(11);
                et.C6 = item.C6 = rnd.Next(11);
                et.C7 = item.C7 = rnd.Next(11);
                et.C8 = item.C8 = rnd.Next(11);
                et.C9 = item.C9 = rnd.Next(11);
                et.C10 = item.C10 = rnd.Next(11);
                et.C11 = item.C11 = rnd.Next(11);
                et.C12 = item.C12 = rnd.Next(11);
                et.C13 = item.C13 = rnd.Next(11);
                et.C14 = item.C14 = rnd.Next(11);
                et.C15 = item.C15 = rnd.Next(11);

                veritabani.SaveChanges();
            }

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult GirisYap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Girisyap(tbl_Kullanici kullanici)
        {
            if (ModelState.IsValid)
            {

                //cshtml sayfasından alınan şifre md5 ile şifrelendi
                kullanici.kullaniciSifre = FormsAuthentication.HashPasswordForStoringInConfigFile(kullanici.kullaniciSifre, "MD5");

                //veritabanında karşılaştırıldı eşleşirse kullanıcı id alınır
                var kullaniciId = veritabani.tbl_Kullanici.
                    Where(x => x.kullaniciAdi == kullanici.kullaniciAdi
                    && x.kullaniciSifre == kullanici.kullaniciSifre).
                    Select(v => v.kullaniciId).FirstOrDefault();

                //eğer kullanıcı var ise 
                if (kullaniciId != 0)
                {
                    //kullanıcı türünü almak için kisiler tablosuyla kullanıcı
                    //tablosu birleştirilir ve kullanıcı türü çekilir
                    var tur = veritabani.tbl_Kullanici.
                    Join(veritabani.tbl_Kisiler,
                    ku => ku.kullaniciId,
                    ki => ki.kullaniciId,
                    (ku, ki) => new { tbl_Kullanici = ku, tbl_Kisiler = ki }).
                    Where(x => x.tbl_Kisiler.kullaniciId == kullaniciId).
                    Select(x => x.tbl_Kisiler.kullaniciTuru).FirstOrDefault();

                    //çekilen kullanıcı türüne ait controllera yollanır kullanıcı
                    if (tur.Equals("organizator"))
                    {
                        //oturum açılır kullanıcıya
                        Session["kullanici"] = kullaniciId;
                        return RedirectToAction("index", "Organizator");
                    }
                    else if (tur.Equals("ziyaretci"))
                    {
                        Session["kullanici"] = kullaniciId;
                        return RedirectToAction("index", "Ziyaretci");
                    }
                    else if (tur.Equals("basin"))
                    {
                        Session["kullanici"] = kullaniciId;
                        return RedirectToAction("index", "Basin");
                    }
                    else if (tur.Equals("katilimci"))
                    {
                        Session["kullanici"] = kullaniciId;
                        return RedirectToAction("index", "Katilimci");
                    }
                    else
                    {
                        //eğer eşleşme olmaz ise tekrar girmesi istenir
                        ViewBag.hataliGiris = "Hata!!";
                        return View();
                    }
                }
                else
                {
                    ViewBag.hataliGiris = "Hatalı giriş yaptınız tekrar deneyiniz";
                    return View();
                }
            }
            else
            {
                return View();
            }



           
        }
        public ActionResult Kayit()
        {
            ViewBag.Title = "Kayıt Ol / Fuar Organizasyonu";
            ViewBag.Message = "Fuar Organizasyonu Kayıt Ol";

            //Sehirler tablosundaki sehiradi ve idler çekilir
            var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
            {
                Selected = false,
                Text = v.SehirAdi,
                Value = v.SehirId.ToString()
            }).ToList();
            ViewBag.Sehirler = sehirler;

            //departman tablosundaki departmanid ve departman adları çekilir
            var departman = veritabani.tbl_Departman.Select(v => new SelectListItem
            {
                Selected = false,
                Text = v.departmanAdi,
                Value = v.departmanId.ToString()
            }).ToList();
            ViewBag.Departman = departman;

            //Pozisyon tablosundaki pozisyon id ve pozisyon adları çekilir
            var pozisyon = veritabani.tbl_Pozisyon.Select(v => new SelectListItem
            {
                Selected = false,
                Text = v.pozisyonAdi,
                Value = v.pozisyonId.ToString()
            }).ToList();
            ViewBag.Pozisyon = pozisyon;

            //Kullanıcı türü için selectlistitem oluşturulur
            List<SelectListItem> kullaniciTuru = new List<SelectListItem>();
            kullaniciTuru.Add(new SelectListItem { Selected = false, Text = "Organizator", Value = "organizator" });
            kullaniciTuru.Add(new SelectListItem { Selected = false, Text = "Basın", Value = "basin" });
            kullaniciTuru.Add(new SelectListItem { Selected = false, Text = "Ziyaretçi", Value = "ziyaretci" });
            kullaniciTuru.Add(new SelectListItem { Selected = false, Text = "Katılımcı", Value = "katilimci" });
            ViewBag.KullaniciTuru = kullaniciTuru;

            //ilceler ve semtmah selectlistleri oluşturlur hata vermemesi için
            List<SelectListItem> ilceler = new List<SelectListItem>();
            List<SelectListItem> semtmah = new List<SelectListItem>();

            ViewBag.ilceId = ilceler;
            ViewBag.SemtMahId = semtmah;

            //kayit ol actionresult çalıştığında bir tuple oluşturlur ve tbl_kullanici ve tbl_kisiler
            //tabloları bu tuple a atılarak aynı anda iki tabloya kayıt yapma işlemi gerçekleştirilir
            return View(Tuple.Create<tbl_Kullanici,tbl_Kisiler>(new tbl_Kullanici(),new tbl_Kisiler()));
        }

        //Kullanıcı kaydı ve kişisel bilgilerin girilmesi
        [HttpPost]
        public ActionResult Kayit([Bind(Prefix = "Item1")]tbl_Kullanici kullanici, [Bind(Prefix = "Item2")]tbl_Kisiler kisiler, string sifre)
        {//alınan boş değerlerine bakılarak gerekli işlemler yapılır
            if (ModelState.IsValid)
            {
                //kullanıcı id yi elle gir
                if (kullanici.kullaniciSifre.Equals(sifre) && sifre.Length >= 6)
                {//şifre kontrolu eşitmi ve 6 karakterden büyük mü diye bakılır

                    var veri = veritabani.tbl_Kullanici.
                        Where(v => v.kullaniciAdi == kullanici.kullaniciAdi).
                        Select(v => v.kullaniciAdi);

                    //veri tabanında böyle bir kullanıcı varmı diye bakılır
                    List<string> verim = veri.ToList();
                    if (verim.Count == 0)//var ise ife girmez
                    {
                        //kullanıcı şifresi şifrelenir
                        kullanici.kullaniciSifre = FormsAuthentication.HashPasswordForStoringInConfigFile(kullanici.kullaniciSifre,"MD5");
                        //kullanıcı tablosuna kayıt edilir
                        veritabani.tbl_Kullanici.Add(kullanici);
                        veritabani.SaveChanges();

                        //kayıt edilen kullanıcı kullanıcı tablosundaki id değeri çekilri
                        var kullaniciId = veritabani.tbl_Kullanici.Where(v => v.kullaniciAdi == kullanici.kullaniciAdi).Select(v => v.kullaniciId).FirstOrDefault();
                        //post ile gelen kisiler nesnesine atılır
                        kisiler.kullaniciId = kullaniciId;
                        //tbl_kisiler tablosuna kayıt aktarılır
                        veritabani.tbl_Kisiler.Add(kisiler);
                        veritabani.SaveChanges();
                        //kişinin kullanıcı türüne göre atama yapılır
                        if (kisiler.kullaniciTuru.Equals("organizator"))
                        {
                            Session["kullanici"] = kisiler.kullaniciId;
                            return RedirectToAction("Index", "Organizator", kisiler.kullaniciId);
                        }
                        else if (kisiler.kullaniciTuru.Equals("basin"))
                        {
                            Session["kullanici"] = kisiler.kullaniciId;
                            return RedirectToAction("Index", "Basin", kisiler.kullaniciId);
                        } 
                        else if (kisiler.kullaniciTuru.Equals("ziyaretci"))
                        {
                            Session["kullanici"] = kisiler.kullaniciId;
                            return RedirectToAction("Index", "Ziyaretci", kisiler.kullaniciId);
                        }                        
                        else if (kisiler.kullaniciTuru.Equals("katilimci"))
                        {
                            Session["kullanici"] = kisiler.kullaniciId;
                            return RedirectToAction("Index", "Katilimci", kisiler.kullaniciId);
                        }
                        else
                        {//eğer giçbiri olmaz ise anasayfaya yönlendirilir
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {//eğer giçbiri olmaz ise anasayfaya yönlendirilir
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {//eğer giçbiri olmaz ise anasayfaya yönlendirilir
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {//eğer giçbiri olmaz ise anasayfaya yönlendirilir
                return RedirectToAction("Index", "Home");
            }
            
        }

        //javascript ajax   ilçe getir
        [HttpPost]
        public ActionResult ilce_getir(int sehirId)
        {
            //eğer kullanıcı bir şehir seçerse ajax bu actionresultu çağırı ve geriye json tipinde ilceleri geri döndürür
            var ilceler = veritabani.Ilceler.
                Where(v => v.SehirId == sehirId).
                Select(v => new { v.ilceId,v.IlceAdi}).ToList();
            return Json(ilceler);
        }

        //javascript ajax mahalle getir
        [HttpPost]
        public ActionResult mahalle_getir(int ilceId)
        {
            //eğer kullanıcı ilçe seçer ise ajax bu actionresultu çağırı ve geriye json tipinde mahalleler döner
            var mahalleler = veritabani.SemtMah.
                Where(v => v.ilceId == ilceId).
                Select(v => new { v.SemtMahId, v.MahalleAdi }).ToList();
            return Json(mahalleler);
        }

        public ActionResult CikisYap()
        {
            Session["kullanici"] = null;
            return RedirectToAction("index", "Home");
        }

    }
}