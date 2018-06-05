using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fuar_Organizasyon.Models;//veritabanı erişim için
using Fuar_Organizasyon.Controllers;//yönetim controllerına erişim için
using System.IO;//servera resim kaydetmek için
using System.Web.Security;//md5 şifreleme
using System.Data.Entity.Validation;//entity framework veritabanı işlemleri hataları için
using System.Diagnostics;

namespace Fuar_Organizasyon.Controllers
{
    public class BasinController : Controller
    {
        FuarDatabaseEntities veritabani;//veritabanı modeli nesnesi tanımlandı
        YonetimController yonetim;//yönetim controller nesnesi tanımlandı
        int kullaniciId;
        public BasinController()
        {//constructor methodta tanımlandı
            veritabani = new FuarDatabaseEntities();
            yonetim = new YonetimController();
        }


        // GET: Basin
        public ActionResult Index()
        {//basin controller index sayfası

            kullaniciId = Convert.ToInt32(Session["kullanici"]);//sessiondan kullanıcı id alınır
            if (yonetim.kullaniciKontrol("basin",kullaniciId))
            {//yönetim de kullanıcı kontrol fonksiypnuna yollanır
                //eğer kullanıcı türü basin ise bu controllera girebilir

                //bu kullanıcı hangi kişiye ait o veritabanından çekilir
                var kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciId == kullaniciId).
                   Select(x => x.kisiIsim).FirstOrDefault();

                //viewbag nesnesine atılır
                ViewBag.ad = kisiler;
                
                //index.cshtml çalıştırır
                return View();
            }
            else
            {
                //eğer kullanıcı türü basin değilse anasayfaya yönlendirir
                return RedirectToAction("index", "Home");
            }
        }

        public ActionResult kisiselBilgi()
        {//kişisel bilgi düzenleme
            //kullanıcı id sessiondan alınır
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("basin", kullaniciId))
            {//kullanıcı türü kontrol edilir

                //hangi kişi olduğu bulunur
                var kisiler = veritabani.tbl_Kisiler.
                    Where(x => x.kullaniciId == kullaniciId).FirstOrDefault();

                //dropdownlist için veritanandan ilçeler seçilir
                //ve seleclist iteme atılır
                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    //eğer sehirid kisilerdeki id ile eşleşirse
                    //dropdownlistte o şehir seçili olarak gelir
                    Selected = v.SehirId == kisiler.SehirId,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();
                ViewBag.Sehirler = sehirler;

                //ilçelerde aynı şekilde sehirler gibi
                var ilceler = veritabani.Ilceler.Where(v => v.SehirId == kisiler.SehirId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.ilceId == kisiler.ilceId,
                        Text = v.IlceAdi,
                        Value = v.ilceId.ToString()
                    }).ToList();
                ViewBag.Ilceler = ilceler;

                //mahallelerde şehirler gibi
                var mahalleler = veritabani.SemtMah.Where(v => v.SemtMahId == kisiler.SemtMahId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.SemtMahId == kisiler.SemtMahId,
                        Text = v.MahalleAdi,
                        Value = v.SemtMahId.ToString()
                    }).ToList();
                ViewBag.Mahalleler = mahalleler;

                //departman tablosundaki departmanid ve departman adları çekilir
                var departman = veritabani.tbl_Departman.Select(v => new SelectListItem
                {
                    Selected = v.departmanId == kisiler.departmanId,
                    Text = v.departmanAdi,
                    Value = v.departmanId.ToString()
                }).ToList();
                ViewBag.Departman = departman;

                //Pozisyon tablosundaki pozisyon id ve pozisyon adları çekilir
                var pozisyon = veritabani.tbl_Pozisyon.Select(v => new SelectListItem
                {
                    Selected = v.pozisyonId == kisiler.pozisyonId,
                    Text = v.pozisyonAdi,
                    Value = v.pozisyonId.ToString()
                }).ToList();
                ViewBag.Pozisyon = pozisyon;

                //resim url viewbag ile yollanır
                ViewBag.resimUrl = "~/Content/Resimler/KisiResim/" + kisiler.kisiResimUrl;
                return View(kisiler);//kisiselBilgi.cshtml model olarak tbl_kisiler tablosunu kullanır
            }
            else
            {//kullanıcı türü basin değil ise ana sayfaya yönlendirir
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult kisiselBilgi(tbl_Kisiler kisiler, HttpPostedFileBase resimYukle)
        {//kullanıcı kişisel bilgileri doldurup post ederse bu method çalışır
            var resimYolu = veritabani.tbl_Kisiler.
                Where(v => v.kisiId == kisiler.kisiId).
                Select(v => v.kisiResimUrl).FirstOrDefault();
            //kullanıcıya ait resim yolu tablodan çekilir

            if (resimYolu != null)
            {//veritabanında resim var ise
                if (resimYukle != null)
                {//güncelleme ekrananında resim yüklenmiş ise
                    if (System.IO.File.Exists(Server.MapPath("~/Content/Resimler/KisiResim/" + resimYolu)))
                    {//sistem klasörüünde resim var ise
                        System.IO.File.Delete(Server.MapPath("~/Content/Resimler/KisiResim/" + resimYolu));
                        //dosyayı sil
                    }
                }
            }
            string benzersiz = " ";
            if(resimYukle!=null)
            {//resim yüklenmiş ise 
                //resmin ismi benzersiz bir string belirlenir
                benzersiz = Guid.NewGuid().ToString() + "_" + Path.GetFileName(resimYukle.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Content/Resimler/KisiResim"), benzersiz);
                //dosya yolu ayarlanır
                resimYukle.SaveAs(filePath);
                //servera kayıt edilir
            }


            tbl_Kisiler guncelKisi = veritabani.tbl_Kisiler.
                Where(v => v.kisiId == kisiler.kisiId).FirstOrDefault();
            //guncel kişi nesnesi oluşuturulur 
            //guncellenen kişi bilgileri buraya atılır

            guncelKisi.kisiIsim = kisiler.kisiIsim;
            guncelKisi.kisiSoyisim = kisiler.kisiSoyisim;
            guncelKisi.kisiTelefon = kisiler.kisiTelefon;
            guncelKisi.kisiTcNo = kisiler.kisiTcNo;
            guncelKisi.SehirId = kisiler.SehirId;
            guncelKisi.ilceId = kisiler.ilceId;
            guncelKisi.SemtMahId = kisiler.SemtMahId;
            guncelKisi.kisiMail = kisiler.kisiMail;
            guncelKisi.kisiResimUrl = kisiler.kisiResimUrl;
            guncelKisi.departmanId = kisiler.departmanId;
            guncelKisi.pozisyonId = kisiler.pozisyonId;
            guncelKisi.kisiResimUrl = benzersiz;

            //veritabanı save edilidiğinde update işlemi gerçekleşir
            veritabani.SaveChanges();

            //kullanıcı tekrardan kisiselbiligi actionuna yollanır
            return RedirectToAction("kisiselBilgi", "Basin");

        }

        public ActionResult kullaniciSifre()
        {//kullanıcı şifre değiştirme bölümü
            //kullanıcı id alınır
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("basin", kullaniciId))
            {//kullanıcı türü basın ise kullaniciSifre.cshml sayfası gösterilir
                return View();
            }
            else
            {//değilse ana sayfaya yollanır
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult kullaniciSifre(FormCollection sifreler)
        {//kullanıcı şifre post edilidiğinde
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            //kullanıcıId çekilir


            if (sifreler["eskiSifre"] != null && sifreler["ysifre"] != null && sifreler["ysifreTekrar"]!=null)
            {

                string eskiSifre = FormsAuthentication.HashPasswordForStoringInConfigFile(sifreler[1], "MD5");
                //eski şifre md5 ile şifrenir

                //bu kullanıcı varmı diye bakılır
                var kullaniciSifre = veritabani.tbl_Kullanici.
                    Where(v => v.kullaniciSifre == eskiSifre && v.kullaniciId==kullaniciId).
                    Select(v => v.kullaniciSifre).FirstOrDefault();
                if (kullaniciSifre != null)
                {//eğer böyle bir kullanıcı var ise
                    if (sifreler[2].Length >= 6 && sifreler[2].Equals(sifreler[3]))
                    {//şifre aynımı ve 6 karakterden büyük mü diye bakılır

                        var kullanici = veritabani.tbl_Kullanici.
                            Where(v => v.kullaniciId == kullaniciId).FirstOrDefault();
                        //kullanıcı çekilir veritabanında
                        eskiSifre = FormsAuthentication.HashPasswordForStoringInConfigFile(sifreler[2], "MD5").ToString();
                        //oluşturulan yeni şifre md5 ile şifrelenir

                        kullanici.kullaniciSifre = eskiSifre;
                        veritabani.SaveChanges();
                        //veritabanı güncelleme işlemi yapılır
                    }
                }
                else
                {

                }
                return RedirectToAction("kullaniciSifre", "Basin");
            }
            else
            {
                return View();
            }
        }

        public ActionResult FirmaBilgi()
        {//firma bilgileri ekleme kısmı
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();


            if (yonetim.kullaniciKontrol("basin", kullaniciId))
            {


                //Firma varmı diye kontrol et firma id alınır
                var firmaVarmi = veritabani.tbl_BasinFirma.
                    Join(veritabani.tbl_BasinFirmaCalisanlar,
                    ku => ku.bId,
                    ki => ki.bId,
                    (ku, ki) => new { tbl_BasinFirma = ku, tbl_BasinFirmaCalisanlar = ki }).
                    Where(x => x.tbl_BasinFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_BasinFirma.bId == x.tbl_BasinFirmaCalisanlar.bId).
                    Select(x => x.tbl_BasinFirma.bId).FirstOrDefault();



                //Bu kullanıcıya ait firmayı çek
                var tbl_BasinFirma = veritabani.tbl_BasinFirma.
                     Where(x => x.bId == firmaVarmi).FirstOrDefault();
                if (tbl_BasinFirma == null)
                {
                    tbl_BasinFirma = new tbl_BasinFirma();
                    tbl_BasinFirma.bFirmaAdi = " ";
                    tbl_BasinFirma.bWebSiteUrl = " ";
                    tbl_BasinFirma.bTelefon = " ";
                    tbl_BasinFirma.bFax = " ";
                    tbl_BasinFirma.bMail = " ";
                    tbl_BasinFirma.bOnay = kisiId.ToString();
                    tbl_BasinFirma.bResimUrl = " ";
                    tbl_BasinFirma.SehirId = 1;
                    tbl_BasinFirma.ilceId = 1;
                    tbl_BasinFirma.SemtMahId = 1;

                    veritabani.tbl_BasinFirma.Add(tbl_BasinFirma);
                    veritabani.SaveChanges();

                    var basinId = veritabani.tbl_BasinFirma.
                        Where(x => x.bOnay == kisiId.ToString()).
                        Select(x => x.bId).FirstOrDefault();

                    tbl_BasinFirmaCalisanlar calisanlar = new tbl_BasinFirmaCalisanlar();
                    calisanlar.bId = basinId;
                    calisanlar.kisiId = kisiId;
                    veritabani.tbl_BasinFirmaCalisanlar.Add(calisanlar);
                    veritabani.SaveChanges();

                }


                //Şehileri çek viewbag ile yolla
                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    Selected = v.SehirId == tbl_BasinFirma.SehirId,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();

                ViewBag.Sehirler = sehirler;

                //ilçerleri çek viewbag ile yolla
                var ilceler = veritabani.Ilceler.
                    Where(x => x.ilceId == tbl_BasinFirma.ilceId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.ilceId == tbl_BasinFirma.ilceId,
                        Text = v.IlceAdi,
                        Value = v.ilceId.ToString()
                    }).ToList();

                ViewBag.Ilceler = ilceler;

                //mahalleleri çek viewbag ile yolla
                var mahalle = veritabani.SemtMah.
                    Where(x => x.SemtMahId == tbl_BasinFirma.SemtMahId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.SemtMahId == tbl_BasinFirma.SemtMahId,
                        Text = v.MahalleAdi,
                        Value = v.SemtMahId.ToString()
                    }).ToList();

                ViewBag.Mahalleler = mahalle;


                //Bu firmada çalısanları çek ve viewbag ile yolla
                var tbl_kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciTuru.Equals("basin")).
                     Select(v => new SelectListItem
                     {
                         Selected = false,
                         Text = v.kisiIsim + "  " + v.kisiSoyisim,
                         Value = v.kisiId.ToString()
                     }).ToList();

                var basinCalisanlar = veritabani.tbl_BasinFirmaCalisanlar.
                    Where(x => x.bId == firmaVarmi).Select(v => v.kisiId).ToList();

                //basin firmada calisanların selected ının true yap
                foreach (int id in basinCalisanlar)
                {
                    foreach (SelectListItem kisi in tbl_kisiler)
                    {
                        if (id.ToString() == kisi.Value)
                        {
                            kisi.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.Calisanlar = tbl_kisiler;

                ViewBag.resimUrl = "~/Content/Resimler/FirmaResim/" + tbl_BasinFirma.bResimUrl;

                return View(tbl_BasinFirma);
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FirmaBilgi(tbl_BasinFirma basinFirma,
          HttpPostedFileBase resimYukle, FormCollection deneme)
        {

            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

            //Firma varmı diye kontrol et firma id alınır
            var firmaVarmi = veritabani.tbl_BasinFirma.
                Join(veritabani.tbl_BasinFirmaCalisanlar,
                ku => ku.bId,
                ki => ki.bId,
                (ku, ki) => new { tbl_BasinFirma= ku, tbl_BasinFirmaCalisanlar= ki }).
                Where(x => x.tbl_BasinFirmaCalisanlar.kisiId == kisiId
                && x.tbl_BasinFirma.bId == x.tbl_BasinFirmaCalisanlar.bId).
                Select(x => x.tbl_BasinFirma.bId).FirstOrDefault();

            //Resim Kayıt edilir
            var resimYolu = veritabani.tbl_BasinFirma
                .Where(v => v.bId == basinFirma.bId).
                Select(v => v.bResimUrl).FirstOrDefault();
            string benzersiz = null;

            if (resimYukle != null)
            {//güncelleme ekrananında resim yüklenmiş ise
                if (System.IO.File.Exists(Server.MapPath("~/Content/Resimler/FirmaResim/" + resimYolu)) || resimYolu==" ")
                {//sistem klasörüünde resim var ise
                    if(resimYolu != " ")
                        System.IO.File.Delete(Server.MapPath("~/Content/Resimler/FirmaResim/" + resimYolu));
                    //dosyayı sil

                    benzersiz = Guid.NewGuid().ToString() + "_" + Path.GetFileName(resimYukle.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Content/Resimler/FirmaResim"), benzersiz);
                    resimYukle.SaveAs(filePath);
                }
            }
            else
            {
                benzersiz = resimYolu;
            }




            tbl_BasinFirma firma = veritabani.tbl_BasinFirma.
                Where(x => x.bId == firmaVarmi).FirstOrDefault();

            if (firma != null)
            {
                //Firma güncelleme kısmı
                firma.bFirmaAdi = basinFirma.bFirmaAdi;
                firma.bWebSiteUrl = basinFirma.bWebSiteUrl;
                firma.bTelefon = basinFirma.bTelefon;
                firma.bFax = basinFirma.bFax;
                firma.bMail = basinFirma.bMail;
                firma.bResimUrl = benzersiz;
                firma.SehirId = basinFirma.SehirId;
                firma.ilceId = basinFirma.ilceId;
                firma.SemtMahId = basinFirma.SemtMahId;
                firma.bOnay = "Beklemede";
            }
            else
            {
                basinFirma.bResimUrl = benzersiz;
                basinFirma.bOnay = "Beklemede";
                veritabani.tbl_BasinFirma.Add(basinFirma);

            }

            try
            {//entity hata bulma
                // doing here my logic
                veritabani.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            if(deneme["calisan"]!=null)
            {
                //gelen seçili elemana göre veri silme ve ekleme kısmı 
                //çalışan için
                string[] cparcala = deneme["calisan"].Split(',');//kişi idleri çek
                int[] kisiIdler = new int[cparcala.Count()];
                int i = 0;
                foreach (string id in cparcala)
                {
                    kisiIdler[i] = Convert.ToInt32(id);
                    i++;
                }

                var basinCalisan = veritabani.tbl_BasinFirmaCalisanlar.
                    Where(x => x.bId == firmaVarmi && !kisiIdler.Contains(x.kisiId)).ToList();
                foreach (tbl_BasinFirmaCalisanlar fa in basinCalisan)
                {
                    veritabani.tbl_BasinFirmaCalisanlar.Remove(fa);
                }//seçili olmayan yani silinmek istenen çalışan veritabanından silinir

                var yeniCalisan = veritabani.tbl_BasinFirmaCalisanlar.
                    Where(x => x.bId == firmaVarmi && kisiIdler.Contains(x.kisiId)).
                    Select(x => x.kisiId).ToList();

                foreach (int id in kisiIdler)
                {
                    if (!yeniCalisan.Contains(id))
                    {
                        tbl_BasinFirmaCalisanlar calisanekle = new tbl_BasinFirmaCalisanlar();
                        calisanekle.bId = firmaVarmi;
                        calisanekle.kisiId = id;
                        veritabani.tbl_BasinFirmaCalisanlar.Add(calisanekle);
                    }
                }
            }
            else
            {
                var basinCalisanlar = veritabani.tbl_BasinFirmaCalisanlar.
                    Where(x => x.bId == firmaVarmi).ToList();
                foreach (tbl_BasinFirmaCalisanlar fa in basinCalisanlar)
                {
                    veritabani.tbl_BasinFirmaCalisanlar.Remove(fa);
                }
            }

            try
            {//entity hata bulma
                // doing here my logic
                veritabani.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            return RedirectToAction("FirmaBilgi", "Basin");
        }
    }
}