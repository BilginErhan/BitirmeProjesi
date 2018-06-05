using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fuar_Organizasyon.Models;
using Fuar_Organizasyon.Controllers;
using System.Web.Security;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Fuar_Organizasyon.Controllers
{
    public class KatilimciController : Controller
    {
        FuarDatabaseEntities veritabani;
        YonetimController yonetim;
        int kullaniciId;

        public KatilimciController()
        {
            veritabani = new FuarDatabaseEntities();
            yonetim = new YonetimController();
        }

        // GET: Katilimci
        public ActionResult Index()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("katilimci",kullaniciId))
            {
                var kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciId == kullaniciId).
                   Select(x => x.kisiIsim).FirstOrDefault();
                ViewBag.ad = kisiler;
                return View();
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        public ActionResult kullaniciSifre()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("katilimci", kullaniciId))
            {
                return View();
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult kullaniciSifre(FormCollection sifreler)
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            string eskiSifre = FormsAuthentication.HashPasswordForStoringInConfigFile(sifreler[1], "MD5");


            var kullaniciSifre = veritabani.tbl_Kullanici.
                Where(v => v.kullaniciSifre == eskiSifre).
                Select(v => v.kullaniciSifre).FirstOrDefault();
            if (kullaniciSifre != null)
            {
                if (sifreler[2].Length >= 6 && sifreler[2].Equals(sifreler[3]))
                {
                    var kullanici = veritabani.tbl_Kullanici.
                        Where(v => v.kullaniciId == kullaniciId).FirstOrDefault();

                    eskiSifre = FormsAuthentication.HashPasswordForStoringInConfigFile(sifreler[2], "MD5").ToString();

                    kullanici.kullaniciSifre = eskiSifre;
                    veritabani.SaveChanges();
                }
            }
            else
            {

            }
            return RedirectToAction("kullaniciSifre", "Katilimci");
        }

        public ActionResult kisiselBilgi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("katilimci", kullaniciId))
            {
                var kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciId == kullaniciId).FirstOrDefault();

                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    Selected = v.SehirId == kisiler.SehirId,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();
                ViewBag.Sehirler = sehirler;

                var ilceler = veritabani.Ilceler.Where(v => v.SehirId == kisiler.SehirId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.ilceId == kisiler.ilceId,
                        Text = v.IlceAdi,
                        Value = v.ilceId.ToString()
                    }).ToList();
                ViewBag.Ilceler = ilceler;

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


                ViewBag.resimUrl = "~/Content/Resimler/KisiResim/" + kisiler.kisiResimUrl;
                return View(kisiler);
            }
            else
            {
                return RedirectToAction("index", "Home");
            }

        }

        [HttpPost]
        public ActionResult kisiselBilgi(tbl_Kisiler kisiler, HttpPostedFileBase resimYukle)
        {

            var resimYolu = veritabani.tbl_Kisiler.Where(v => v.kisiId == kisiler.kisiId).Select(v => v.kisiResimUrl).FirstOrDefault();

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

            string benzersiz;
            if (resimYolu == null || resimYolu == " ")
                benzersiz = " ";
            else
                benzersiz = resimYolu;

            if (resimYukle != null)
            {//resim yüklenmiş ise 
                //resmin ismi benzersiz bir string belirlenir
                benzersiz = Guid.NewGuid().ToString() + "_" + Path.GetFileName(resimYukle.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Content/Resimler/KisiResim"), benzersiz);
                //dosya yolu ayarlanır
                resimYukle.SaveAs(filePath);
                //servera kayıt edilir
            }

            tbl_Kisiler guncelKisi = veritabani.tbl_Kisiler.Where(v => v.kisiId == kisiler.kisiId).FirstOrDefault();

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

            veritabani.SaveChanges();

            return RedirectToAction("kisiselBilgi", "Katilimci");

        }

        //Katılımcı firma bilgi güncelleme
        public ActionResult FirmaBilgi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();


            if (yonetim.kullaniciKontrol("katilimci", kullaniciId))
            {


                //Firma varmı diye kontrol et firma id alınır
                var firmaVarmi = veritabani.tbl_KatilimciFirma.
                    Join(veritabani.tbl_KatilimciFirmaCalisanlar,
                    ku => ku.kId,
                    ki => ki.kId,
                    (ku, ki) => new { tbl_KatilimciFirma = ku, tbl_KatilimciFirmaCalisanlar= ki }).
                    Where(x => x.tbl_KatilimciFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_KatilimciFirma.kId == x.tbl_KatilimciFirmaCalisanlar.kId).
                    Select(x => x.tbl_KatilimciFirma.kId).FirstOrDefault();



                //Bu kullanıcıya ait firmayı çek
                var tbl_katilimciFirma = veritabani.tbl_KatilimciFirma.
                     Where(x => x.kId == firmaVarmi).FirstOrDefault();
                if (tbl_katilimciFirma == null)
                {
                    tbl_katilimciFirma = new tbl_KatilimciFirma();
                    tbl_katilimciFirma.kFirmaAdi = " ";
                    tbl_katilimciFirma.kWebSiteUrl = " ";
                    tbl_katilimciFirma.kTelefon = " ";
                    tbl_katilimciFirma.kFax = " ";
                    tbl_katilimciFirma.kMail = " ";
                    tbl_katilimciFirma.kOnay = kisiId.ToString();
                    tbl_katilimciFirma.kResimUrl = " ";
                    tbl_katilimciFirma.SehirId = 1;
                    tbl_katilimciFirma.ilceId = 1;
                    tbl_katilimciFirma.SemtMahId = 1;

                    veritabani.tbl_KatilimciFirma.Add(tbl_katilimciFirma);
                    veritabani.SaveChanges();

                    var katilimciId = veritabani.tbl_KatilimciFirma.
                        Where(x => x.kOnay == kisiId.ToString()).
                        Select(x => x.kId).FirstOrDefault();

                    tbl_KatilimciFirmaCalisanlar calisanlar = new tbl_KatilimciFirmaCalisanlar();
                    calisanlar.kId = katilimciId;
                    calisanlar.kisiId = kisiId;
                    veritabani.tbl_KatilimciFirmaCalisanlar.Add(calisanlar);
                    veritabani.SaveChanges();

                }


                //Şehileri çek viewbag ile yolla
                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    Selected = v.SehirId == tbl_katilimciFirma.SehirId,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();

                ViewBag.Sehirler = sehirler;

                //ilçerleri çek viewbag ile yolla
                var ilceler = veritabani.Ilceler.Where(x => x.ilceId == tbl_katilimciFirma.ilceId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.ilceId == tbl_katilimciFirma.ilceId,
                        Text = v.IlceAdi,
                        Value = v.ilceId.ToString()
                    }).ToList();

                ViewBag.Ilceler = ilceler;

                //mahalleleri çek viewbag ile yolla
                var mahalle = veritabani.SemtMah.Where(x => x.SemtMahId == tbl_katilimciFirma.SemtMahId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.SemtMahId == tbl_katilimciFirma.SemtMahId,
                        Text = v.MahalleAdi,
                        Value = v.SemtMahId.ToString()
                    }).ToList();

                ViewBag.Mahalleler = mahalle;

                //Bu firmada çalısanları çek ve viewbag ile yolla
                var tbl_kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciTuru.Equals("katilimci")).
                     Select(v => new SelectListItem
                     {
                         Selected = false,
                         Text = v.kisiIsim + "  " + v.kisiSoyisim,
                         Value = v.kisiId.ToString()
                     }).ToList();

                var katilimciCalisanlar = veritabani.tbl_KatilimciFirmaCalisanlar.
                    Where(x => x.kId == firmaVarmi).Select(v => v.kisiId).ToList();

                //organizator firmada calisanların selected ının true yap
                foreach (int id in katilimciCalisanlar)
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



                //firmanın faaliyet analarını çek ve yolla
                var tbl_FaaliyetAlani = veritabani.tbl_FaatliyetAlani.
                     Select(v => new SelectListItem
                     {
                         Selected = false,
                         Text = v.faaliyetAlaniAdi,
                         Value = v.faaliyetAlaniId.ToString()
                     }).ToList();

                //Organiztor firmanın faaliyet alanları çekilir
                var tbl_katilimciFaaliyetAlani = veritabani.tbl_KatilimciFaaliyetAlani.
                    Where(v => v.kId == firmaVarmi).Select(v => v.faaliyetAlaniId).ToList();

                foreach (int id in tbl_katilimciFaaliyetAlani)
                {
                    foreach (SelectListItem faaliyetId in tbl_FaaliyetAlani)
                    {
                        if (id.ToString() == faaliyetId.Value)
                        {
                            faaliyetId.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.FaaliyetAlanlari = tbl_FaaliyetAlani;


                //Firmanın faaliyet türlerini çek ve yolla
                var tbl_FaaliyetTuru = veritabani.tbl_FaaliyetTuru
                    .Select(v => new SelectListItem
                    {
                        Selected = false,
                        Text = v.faaliyetTuruAdi,
                        Value = v.faaliyetTuruId.ToString()
                    }).ToList();

                var katilimciFaaliyetTuru = veritabani.tbl_KatilimciFaaliyetTuru.
                    Where(x => x.kId == firmaVarmi).Select(x => x.faaliyetTuruId).ToList();

                foreach (int id in katilimciFaaliyetTuru)
                {
                    foreach (SelectListItem faaliyetTurId in tbl_FaaliyetTuru)
                    {
                        if (id.ToString() == faaliyetTurId.Value)
                        {
                            faaliyetTurId.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.FaaliyetTurleri = tbl_FaaliyetTuru;

                var tbl_Amac = veritabani.tbl_Amac.Select(v => new SelectListItem
                {
                    Selected = false,
                    Text = v.amacAdi,
                    Value = v.amacId.ToString()
                }).ToList();

                var katilimciAmac = veritabani.tbl_KatilimciKatilimAmac.
                    Where(x => x.kId == firmaVarmi).Select(x => x.kKatilimAmacId).ToList();

                foreach(int id in katilimciAmac)
                {
                    foreach(SelectListItem amacid in tbl_Amac)
                    {
                        if (id.ToString() == amacid.Value)
                        {
                            amacid.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.katilimAmac = tbl_Amac;

                ViewBag.resimUrl = "~/Content/Resimler/FirmaResim/" + tbl_katilimciFirma.kResimUrl;

                return View(tbl_katilimciFirma);

            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FirmaBilgi(tbl_KatilimciFirma katilimciFirma,
            HttpPostedFileBase resimYukle, FormCollection deneme)
        {

            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

            //Firma varmı diye kontrol et firma id alınır
            var firmaVarmi = veritabani.tbl_KatilimciFirma.
                Join(veritabani.tbl_KatilimciFirmaCalisanlar,
                ku => ku.kId,
                ki => ki.kId,
                (ku, ki) => new { tbl_KatilimciFirma = ku, tbl_KatilimciFirmaCalisanlar = ki }).
                Where(x => x.tbl_KatilimciFirmaCalisanlar.kisiId == kisiId
                && x.tbl_KatilimciFirma.kId == x.tbl_KatilimciFirmaCalisanlar.kId).
                Select(x => x.tbl_KatilimciFirma.kId).FirstOrDefault();

            //Resim Kayıt edilir
            var resimYolu = veritabani.tbl_KatilimciFirma.Where(v => v.kId == katilimciFirma.kId).
                Select(v => v.kResimUrl).FirstOrDefault();
            string benzersiz = null;

            if (resimYukle != null)
            {//güncelleme ekrananında resim yüklenmiş ise
                if (System.IO.File.Exists(Server.MapPath("~/Content/Resimler/FirmaResim/" + resimYolu)) || resimYolu == " ")
                {//sistem klasörüünde resim var ise
                    if (resimYolu != " ")
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




            tbl_KatilimciFirma firma = veritabani.tbl_KatilimciFirma.
                Where(x => x.kId == firmaVarmi).FirstOrDefault();

            if (firma != null)
            {
                //Firma güncelleme kısmı
                firma.kFirmaAdi = katilimciFirma.kFirmaAdi;
                firma.kWebSiteUrl = katilimciFirma.kWebSiteUrl;
                firma.kTelefon = katilimciFirma.kTelefon;
                firma.kFax = katilimciFirma.kFax;
                firma.kMail = katilimciFirma.kMail;
                firma.kResimUrl = benzersiz;
                firma.SehirId = katilimciFirma.SehirId;
                firma.ilceId = katilimciFirma.ilceId;
                firma.SemtMahId = katilimciFirma.SemtMahId;
                firma.kCalisanSayisi = katilimciFirma.kCalisanSayisi;
                firma.kOnay = "Beklemede";
            }
            else
            {
                katilimciFirma.kResimUrl = benzersiz;
                katilimciFirma.kOnay = "Beklemede";
                veritabani.tbl_KatilimciFirma.Add(katilimciFirma);

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


            int i = 0;
            if (deneme["calisan"] != null)
            {
                //gelen seçili elemana göre veri silme ve ekleme kısmı 
                //çalışan için
                string[] cparcala = deneme["calisan"].Split(',');//kişi idleri çek
                int[] kisiIdler = new int[cparcala.Count()];
                i = 0;
                foreach (string id in cparcala)
                {
                    kisiIdler[i] = Convert.ToInt32(id);
                    i++;
                }

                var katilimciCalisan = veritabani.tbl_KatilimciFirmaCalisanlar.
                    Where(x => x.kId == firmaVarmi && !kisiIdler.Contains(x.kisiId)).ToList();
                foreach (tbl_KatilimciFirmaCalisanlar fa in katilimciCalisan)
                {
                    veritabani.tbl_KatilimciFirmaCalisanlar.Remove(fa);
                }//seçili olmayan yani silinmek istenen çalışan veritabanından silinir

                var yeniCalisan = veritabani.tbl_KatilimciFirmaCalisanlar.
                    Where(x => x.kId == firmaVarmi && kisiIdler.Contains(x.kisiId)).
                    Select(x => x.kisiId).ToList();

                foreach (int id in kisiIdler)
                {
                    if (!yeniCalisan.Contains(id))
                    {
                        tbl_KatilimciFirmaCalisanlar calisanekle = new tbl_KatilimciFirmaCalisanlar();
                        calisanekle.kId = firmaVarmi;
                        calisanekle.kisiId = id;
                        veritabani.tbl_KatilimciFirmaCalisanlar.Add(calisanekle);
                    }
                }
            }
            else
            {
                var katilimciCalisan = veritabani.tbl_KatilimciFirmaCalisanlar.
                    Where(x => x.kId == firmaVarmi).ToList();
                foreach (tbl_KatilimciFirmaCalisanlar fa in katilimciCalisan)
                {
                    veritabani.tbl_KatilimciFirmaCalisanlar.Remove(fa);
                }
            }
            
            if (deneme["faaliyetAlani"] != null)
            {
                /*#########   Faaliyet Alanı kısmı ##############*/
                string[] cparcala1 = deneme["faaliyetAlani"].Split(',');//faaliyet alanı idleri çek
                int[] alanidler = new int[cparcala1.Count()];
                i = 0;
                foreach (string id in cparcala1)
                {
                    alanidler[i] = Convert.ToInt32(id);
                    i++;
                }

                var katilimciAlanlar = veritabani.tbl_KatilimciFaaliyetAlani.
                    Where(x => x.kId == firmaVarmi && !alanidler.Contains(x.faaliyetAlaniId)).ToList();

                foreach (tbl_KatilimciFaaliyetAlani fa in katilimciAlanlar)
                {
                    veritabani.tbl_KatilimciFaaliyetAlani.Remove(fa);
                }

                var yeniAlan = veritabani.tbl_KatilimciFaaliyetAlani.
                    Where(x => x.kId == firmaVarmi
                    && alanidler.Contains(x.faaliyetAlaniId)).
                    Select(x => x.faaliyetAlaniId).ToList();

                foreach (int id in alanidler)
                {
                    if (!yeniAlan.Contains(id))
                    {
                        tbl_KatilimciFaaliyetAlani yeniAlanEkle = new tbl_KatilimciFaaliyetAlani();
                        yeniAlanEkle.kId = firmaVarmi;
                        yeniAlanEkle.faaliyetAlaniId = id;
                        veritabani.tbl_KatilimciFaaliyetAlani.Add(yeniAlanEkle);
                    }
                }
            }
            else
            {
                var faaliyetAlani = veritabani.tbl_KatilimciFaaliyetAlani.
                    Where(x => x.kId == firmaVarmi).ToList();
                foreach (tbl_KatilimciFaaliyetAlani fa in faaliyetAlani)
                {
                    veritabani.tbl_KatilimciFaaliyetAlani.Remove(fa);
                }
            }

            if (deneme["faaliyetTuru"] !=null)
            {
                /*#########   Faaliyet Türü kısmı ##############*/
                string[] cparcala2 = deneme["faaliyetTuru"].Split(',');//faaliyet türü idleri çek
                int[] turidler = new int[cparcala2.Count()];
                i = 0;
                foreach (string id in cparcala2)
                {
                    turidler[i] = Convert.ToInt32(id);
                    i++;
                }

                var katilimciFaTur = veritabani.tbl_KatilimciFaaliyetTuru.
                    Where(x => x.kId == firmaVarmi
                    && !turidler.Contains(x.faaliyetTuruId)).ToList();

                foreach (tbl_KatilimciFaaliyetTuru fa in katilimciFaTur)
                {
                    veritabani.tbl_KatilimciFaaliyetTuru.Remove(fa);
                }

                var yeniTur = veritabani.tbl_KatilimciFaaliyetTuru.
                    Where(x => x.kId == firmaVarmi
                    && turidler.Contains(x.faaliyetTuruId)).
                    Select(x => x.faaliyetTuruId).ToList();

                foreach (int id in turidler)
                {
                    if (!yeniTur.Contains(id))
                    {
                        tbl_KatilimciFaaliyetTuru yeniTurEkle = new tbl_KatilimciFaaliyetTuru();
                        yeniTurEkle.kId = firmaVarmi;
                        yeniTurEkle.faaliyetTuruId = id;
                        veritabani.tbl_KatilimciFaaliyetTuru.Add(yeniTurEkle);
                    }
                }
            }
            else
            {
                var faaliyetTuru = veritabani.tbl_KatilimciFaaliyetTuru.
                    Where(x => x.kId == firmaVarmi).ToList();
                foreach (tbl_KatilimciFaaliyetTuru fa in faaliyetTuru)
                {
                    veritabani.tbl_KatilimciFaaliyetTuru.Remove(fa);
                }
            }

            if (deneme["katilimAmac"] != null)
            {
                string[] cparcala3 = deneme["katilimAmac"].Split(',');//faaliyet türü idleri çek
                int[] amacidler = new int[cparcala3.Count()];
                i = 0;
                foreach (string id in cparcala3)
                {
                    amacidler[i] = Convert.ToInt32(id);
                    i++;
                }

                var katilimciAmac = veritabani.tbl_KatilimciKatilimAmac.
                    Where(x => x.kId == firmaVarmi
                    && !amacidler.Contains(x.amacId)).ToList();

                foreach (tbl_KatilimciKatilimAmac fa in katilimciAmac)
                {
                    veritabani.tbl_KatilimciKatilimAmac.Remove(fa);
                }

                var yeniAmac = veritabani.tbl_KatilimciKatilimAmac.
                    Where(x => x.kId == firmaVarmi
                    && amacidler.Contains(x.amacId)).
                    Select(x => x.amacId).ToList();

                foreach (int id in amacidler)
                {
                    if (!yeniAmac.Contains(id))
                    {
                        tbl_KatilimciKatilimAmac yeniAmacEkle = new tbl_KatilimciKatilimAmac();
                        yeniAmacEkle.kId = firmaVarmi;
                        yeniAmacEkle.amacId= id;
                        veritabani.tbl_KatilimciKatilimAmac.Add(yeniAmacEkle);
                    }
                }

            }
            else
            {
                var katilimAmac = veritabani.tbl_KatilimciKatilimAmac.
                    Where(x => x.kId == firmaVarmi).ToList();
                foreach (tbl_KatilimciKatilimAmac fa in katilimAmac)
                {
                    veritabani.tbl_KatilimciKatilimAmac.Remove(fa);
                }
            }


            veritabani.SaveChanges();

            return RedirectToAction("FirmaBilgi", "Katilimci");
        }


        public ActionResult FirmaAnket()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FirmaAnket(EgitimTablo tb)
        {

            TestTablo test = new TestTablo();
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

            //Firma varmı diye kontrol et firma id alınır
            var firmaVarmi = veritabani.tbl_KatilimciFirma.
                Join(veritabani.tbl_KatilimciFirmaCalisanlar,
                ku => ku.kId,
                ki => ki.kId,
                (ku, ki) => new { tbl_KatilimciFirma = ku, tbl_KatilimciFirmaCalisanlar = ki }).
                Where(x => x.tbl_KatilimciFirmaCalisanlar.kisiId == kisiId
                && x.tbl_KatilimciFirma.kId == x.tbl_KatilimciFirmaCalisanlar.kId).
                Select(x => x.tbl_KatilimciFirma.kId).FirstOrDefault();

            var firmaadi = veritabani.tbl_KatilimciFirma.Where(x => x.kId == firmaVarmi).
                Select(c => c.kFirmaAdi).FirstOrDefault();

            test.firmaid = firmaVarmi;
            test.firmaadi = firmaadi;
            test.C1 = tb.C1;
            test.C2 = tb.C2;
            test.C3 = tb.C3;
            test.C4 = tb.C4;
            test.C5 = tb.C5;
            test.C6 = tb.C6;
            test.C7 = tb.C7;
            test.C8 = tb.C8;
            test.C9 = tb.C9;
            test.C10 = tb.C10;
            test.C11 = tb.C11;
            test.C12 = tb.C12;
            test.C13 = tb.C13;
            test.C14 = tb.C14;
            test.C15 = tb.C15;

            veritabani.TestTablo.Add(test);
            veritabani.SaveChanges();

            return RedirectToAction("FirmaAnket", "Katilimci");
        }

    }
}