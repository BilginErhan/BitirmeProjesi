using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fuar_Organizasyon.Models;
using Fuar_Organizasyon.Controllers;
using System.IO;
using System.Web.Security;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Fuar_Organizasyon.Controllers
{
    public class OrganizatorController : Controller
    {
        FuarDatabaseEntities veritabani;
        YonetimController yonetim;
        int kullaniciId;

        public OrganizatorController()
        {
            yonetim = new YonetimController();
            veritabani = new FuarDatabaseEntities();

        }

        // GET: Organizator
        public ActionResult Index()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
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

        public ActionResult kisiselBilgi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
            {
                var kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciId == kullaniciId).
                    FirstOrDefault();

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

            return RedirectToAction("kisiselBilgi", "Organizator");

        }

        public ActionResult kullaniciSifre()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
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
                    //return RedirectToAction("kullaniciSifre", "organizator");
                }
            }
            else
            {

            }
            return RedirectToAction("kullaniciSifre", "Organizator");
        }

        //Organizator firma bilgi güncelleme
        public ActionResult FirmaBilgi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();


            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
            {


                //Firma varmı diye kontrol et firma id alınır
                var firmaVarmi = veritabani.tbl_OrganizatorFirma.
                    Join(veritabani.tbl_OrganizatorFirmaCalisanlar,
                    ku => ku.oId,
                    ki => ki.oId,
                    (ku, ki) => new { tbl_OrganizatorFirma = ku, tbl_OrganizatorFirmaCalisanlar = ki }).
                    Where(x => x.tbl_OrganizatorFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_OrganizatorFirma.oId == x.tbl_OrganizatorFirmaCalisanlar.oId).
                    Select(x => x.tbl_OrganizatorFirma.oId).FirstOrDefault();



                //Bu kullanıcıya ait firmayı çek
                var tbl_OrganizatorFirma = veritabani.tbl_OrganizatorFirma.
                     Where(x => x.oId == firmaVarmi).FirstOrDefault();
                if (tbl_OrganizatorFirma == null)
                {
                    tbl_OrganizatorFirma = new tbl_OrganizatorFirma();
                    tbl_OrganizatorFirma.oFirmaAdi = " ";
                    tbl_OrganizatorFirma.oWebSite = " ";
                    tbl_OrganizatorFirma.oTelefon = " ";
                    tbl_OrganizatorFirma.oFax = " ";
                    tbl_OrganizatorFirma.oMail = " ";
                    tbl_OrganizatorFirma.oOnay = kisiId.ToString();
                    tbl_OrganizatorFirma.oResimUrl = " ";
                    tbl_OrganizatorFirma.SehirId = 1;
                    tbl_OrganizatorFirma.ilceId = 1;
                    tbl_OrganizatorFirma.SemtMahId = 1;

                    veritabani.tbl_OrganizatorFirma.Add(tbl_OrganizatorFirma);
                    veritabani.SaveChanges();

                    var organizatorId = veritabani.tbl_OrganizatorFirma.
                        Where(x => x.oOnay == kisiId.ToString()).
                        Select(x => x.oId).FirstOrDefault();

                    tbl_OrganizatorFirmaCalisanlar calisanlar = new tbl_OrganizatorFirmaCalisanlar();
                    calisanlar.oId = organizatorId;
                    calisanlar.kisiId = kisiId;
                    veritabani.tbl_OrganizatorFirmaCalisanlar.Add(calisanlar);
                    veritabani.SaveChanges();

                }


                //Şehileri çek viewbag ile yolla
                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    Selected = v.SehirId == tbl_OrganizatorFirma.SehirId,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();

                ViewBag.Sehirler = sehirler;

                //ilçerleri çek viewbag ile yolla
                var ilceler = veritabani.Ilceler.Where(x => x.ilceId == tbl_OrganizatorFirma.ilceId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.ilceId == tbl_OrganizatorFirma.ilceId,
                        Text = v.IlceAdi,
                        Value = v.ilceId.ToString()
                    }).ToList();

                ViewBag.Ilceler = ilceler;

                //mahalleleri çek viewbag ile yolla
                var mahalle = veritabani.SemtMah.Where(x => x.SemtMahId == tbl_OrganizatorFirma.SemtMahId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.SemtMahId == tbl_OrganizatorFirma.SemtMahId,
                        Text = v.MahalleAdi,
                        Value = v.SemtMahId.ToString()
                    }).ToList();

                ViewBag.Mahalleler = mahalle;

                //Bu firmada çalısanları çek ve viewbag ile yolla
                var tbl_kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciTuru.Equals("organizator")).
                     Select(v => new SelectListItem
                     {
                         Selected = false,
                         Text = v.kisiIsim + "  " + v.kisiSoyisim,
                         Value = v.kisiId.ToString()
                     }).ToList();

                var organizatorCalisanlar = veritabani.tbl_OrganizatorFirmaCalisanlar.
                    Where(x => x.oId == firmaVarmi).Select(v => v.kisiId).ToList();

                //organizator firmada calisanların selected ının true yap
                foreach (int id in organizatorCalisanlar)
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
                var tbl_organizatorFaaliyetAlani = veritabani.tbl_OrganizatorFaaliyetAlani.
                    Where(v => v.oId == firmaVarmi).Select(v => v.faaliyetAlaniId).ToList();

                foreach (int id in tbl_organizatorFaaliyetAlani)
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

                var organizatorFaaliyetTuru = veritabani.tbl_OrganizatorFaaliyetTuru.
                    Where(x => x.oId == firmaVarmi).Select(x => x.faaliyetTuruId).ToList();

                foreach (int id in organizatorFaaliyetTuru)
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

                ViewBag.resimUrl = "~/Content/Resimler/FirmaResim/" + tbl_OrganizatorFirma.oResimUrl;

                return View(tbl_OrganizatorFirma);

            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FirmaBilgi(tbl_OrganizatorFirma organizatorFirma,
            HttpPostedFileBase resimYukle, FormCollection deneme)
        {

            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

            //Firma varmı diye kontrol et firma id alınır
            var firmaVarmi = veritabani.tbl_OrganizatorFirma.
                Join(veritabani.tbl_OrganizatorFirmaCalisanlar,
                ku => ku.oId,
                ki => ki.oId,
                (ku, ki) => new { tbl_OrganizatorFirma = ku, tbl_OrganizatorFirmaCalisanlar = ki }).
                Where(x => x.tbl_OrganizatorFirmaCalisanlar.kisiId == kisiId
                && x.tbl_OrganizatorFirma.oId == x.tbl_OrganizatorFirmaCalisanlar.oId).
                Select(x => x.tbl_OrganizatorFirma.oId).FirstOrDefault();

            //Resim Kayıt edilir
            var resimYolu = veritabani.tbl_OrganizatorFirma.Where(v => v.oId == organizatorFirma.oId).Select(v => v.oResimUrl).FirstOrDefault();
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




            tbl_OrganizatorFirma firma = veritabani.tbl_OrganizatorFirma.
                Where(x => x.oId == firmaVarmi).FirstOrDefault();

            if (firma != null)
            {
                //Firma güncelleme kısmı
                firma.oFirmaAdi = organizatorFirma.oFirmaAdi;
                firma.oWebSite = organizatorFirma.oWebSite;
                firma.oTelefon = organizatorFirma.oTelefon;
                firma.oFax = organizatorFirma.oFax;
                firma.oMail = organizatorFirma.oMail;
                firma.oResimUrl = benzersiz;
                firma.SehirId = organizatorFirma.SehirId;
                firma.ilceId = organizatorFirma.ilceId;
                firma.SemtMahId = organizatorFirma.SemtMahId;
                firma.oCalisanSayisi = organizatorFirma.oCalisanSayisi;
                firma.oOnay = "Beklemede";
            }
            else
            {
                organizatorFirma.oResimUrl = benzersiz;
                organizatorFirma.oOnay = "Beklemede";
                veritabani.tbl_OrganizatorFirma.Add(organizatorFirma);

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
                /*#########   Çalışan kısmı ##############*/
                string[] cparcala = deneme["calisan"].Split(',');//kişi idleri çek
                int[] kisiIdler = new int[cparcala.Count()];
                i = 0;
                foreach (string id in cparcala)
                {
                    kisiIdler[i] = Convert.ToInt32(id);
                    i++;
                }

                var orgcalisan = veritabani.tbl_OrganizatorFirmaCalisanlar.
                    Where(x => x.oId == firmaVarmi && !kisiIdler.Contains(x.kisiId)).ToList();
                foreach (tbl_OrganizatorFirmaCalisanlar fa in orgcalisan)
                {
                    veritabani.tbl_OrganizatorFirmaCalisanlar.Remove(fa);
                }//seçili olmayan yani silinmek istenen çalışan veritabanından silinir

                var yeniCalisan = veritabani.tbl_OrganizatorFirmaCalisanlar.
                    Where(x => x.oId == firmaVarmi && kisiIdler.Contains(x.kisiId)).
                    Select(x => x.kisiId).ToList();

                foreach (int id in kisiIdler)
                {
                    if (!yeniCalisan.Contains(id))
                    {
                        tbl_OrganizatorFirmaCalisanlar calisanekle = new tbl_OrganizatorFirmaCalisanlar();
                        calisanekle.oId = firmaVarmi;
                        calisanekle.kisiId = id;
                        veritabani.tbl_OrganizatorFirmaCalisanlar.Add(calisanekle);
                    }
                }


            }
            else
            {
                var orgcalisanlar = veritabani.tbl_OrganizatorFirmaCalisanlar.
                    Where(x => x.oId == firmaVarmi).ToList();
                foreach (tbl_OrganizatorFirmaCalisanlar fa in orgcalisanlar)
                {
                    veritabani.tbl_OrganizatorFirmaCalisanlar.Remove(fa);
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

                var orgalanlar = veritabani.tbl_OrganizatorFaaliyetAlani.
                    Where(x => x.oId == firmaVarmi &&
                    !alanidler.Contains(x.faaliyetAlaniId)).ToList();

                foreach (tbl_OrganizatorFaaliyetAlani fa in orgalanlar)
                {
                    veritabani.tbl_OrganizatorFaaliyetAlani.Remove(fa);
                }

                var yeniAlan = veritabani.tbl_OrganizatorFaaliyetAlani.
                    Where(x => x.oId == firmaVarmi
                    && alanidler.Contains(x.faaliyetAlaniId)).
                    Select(x => x.faaliyetAlaniId).ToList();

                foreach (int id in alanidler)
                {
                    if (!yeniAlan.Contains(id))
                    {
                        tbl_OrganizatorFaaliyetAlani yeniAlanEkle = new tbl_OrganizatorFaaliyetAlani();
                        yeniAlanEkle.oId = firmaVarmi;
                        yeniAlanEkle.faaliyetAlaniId = id;
                        veritabani.tbl_OrganizatorFaaliyetAlani.Add(yeniAlanEkle);
                    }
                }



            }
            else
            {
                var faaliyetAlani = veritabani.tbl_OrganizatorFaaliyetAlani.
                    Where(x => x.oId == firmaVarmi).ToList();
                foreach (tbl_OrganizatorFaaliyetAlani fa in faaliyetAlani)
                {
                    veritabani.tbl_OrganizatorFaaliyetAlani.Remove(fa);
                }
            }

            if (deneme["faaliyetTuru"] != null)
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

                var orgturler = veritabani.tbl_OrganizatorFaaliyetTuru.
                    Where(x => x.oId == firmaVarmi
                    && !turidler.Contains(x.faaliyetTuruId)).ToList();

                foreach (tbl_OrganizatorFaaliyetTuru fa in orgturler)
                {
                    veritabani.tbl_OrganizatorFaaliyetTuru.Remove(fa);
                }

                var yeniTur = veritabani.tbl_OrganizatorFaaliyetTuru.
                    Where(x => x.oId == firmaVarmi
                    && turidler.Contains(x.faaliyetTuruId)).
                    Select(x => x.faaliyetTuruId).ToList();

                foreach (int id in turidler)
                {
                    if (!yeniTur.Contains(id))
                    {
                        tbl_OrganizatorFaaliyetTuru yeniTurEkle = new tbl_OrganizatorFaaliyetTuru();
                        yeniTurEkle.oId = firmaVarmi;
                        yeniTurEkle.faaliyetTuruId = id;
                        veritabani.tbl_OrganizatorFaaliyetTuru.Add(yeniTurEkle);
                    }
                }
            }
            else
            {
                var faaliyetTuru = veritabani.tbl_OrganizatorFaaliyetTuru.
                    Where(x => x.oId == firmaVarmi).ToList();
                foreach (tbl_OrganizatorFaaliyetTuru fa in faaliyetTuru)
                {
                    veritabani.tbl_OrganizatorFaaliyetTuru.Remove(fa);
                }
            }


            veritabani.SaveChanges();

            return RedirectToAction("FirmaBilgi", "Organizator");
        }

        public ActionResult FuarOlustur()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
            {
                //Şehileri çek viewbag ile yolla
                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    Selected = false,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();
                ViewBag.Sehirler = sehirler;
                return View();
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FuarOlustur(tbl_Fuar fuar)
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

            //Firma id si çekilir varmı diye kontrol et firma id alınır
            var firmaVarmi = veritabani.tbl_OrganizatorFirma.
                Join(veritabani.tbl_OrganizatorFirmaCalisanlar,
                ku => ku.oId,
                ki => ki.oId,
                (ku, ki) => new { tbl_OrganizatorFirma = ku, tbl_OrganizatorFirmaCalisanlar = ki }).
                Where(x => x.tbl_OrganizatorFirmaCalisanlar.kisiId == kisiId
                && x.tbl_OrganizatorFirma.oId == x.tbl_OrganizatorFirmaCalisanlar.oId).
                Select(x => x.tbl_OrganizatorFirma.oId).FirstOrDefault();

            fuar.fuarOnay = "Beklemede";

            veritabani.tbl_Fuar.Add(fuar);
            veritabani.SaveChanges();

            tbl_OrganizatorKatildigiFuarlar orgOlusturduguFuarlar = new tbl_OrganizatorKatildigiFuarlar();
            orgOlusturduguFuarlar.oId = firmaVarmi;
            orgOlusturduguFuarlar.fuarId = fuar.fuarId;

            veritabani.tbl_OrganizatorKatildigiFuarlar.Add(orgOlusturduguFuarlar);
            veritabani.SaveChanges();
            return RedirectToAction("FuarOlustur", "Organizator");
        }

        [HttpPost]
        public ActionResult fuarKapmsamAlaniGetir(int fuarSec)
        {
            var fuarAlan = veritabani.tbl_FuarKapsamAlani.
                Where(x => x.fuarId == fuarSec).
                Select(x => new { x.faaliyetAlaniId }).ToList();

            return Json(fuarAlan);
        }

        [HttpPost]
        public ActionResult fuarKapmsamTurGetir(int fuarSec)
        {
            var fuarTur = veritabani.tbl_FuarKapsamTuru.
                Where(x => x.fuarId == fuarSec).
                Select(x => new { x.faaliyetTuruId }).ToList();

            return Json(fuarTur);
        }


        public ActionResult FuarKapsamEkle()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
            {

                var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

                //Firma varmı diye kontrol et firma id alınır
                var firmaVarmi = veritabani.tbl_OrganizatorFirma.
                    Join(veritabani.tbl_OrganizatorFirmaCalisanlar,
                    ku => ku.oId,
                    ki => ki.oId,
                    (ku, ki) => new { tbl_OrganizatorFirma = ku, tbl_OrganizatorFirmaCalisanlar = ki }).
                    Where(x => x.tbl_OrganizatorFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_OrganizatorFirma.oId == x.tbl_OrganizatorFirmaCalisanlar.oId).
                    Select(x => x.tbl_OrganizatorFirma.oId).FirstOrDefault();

                //organizatorün oluşturduğu fuarlar çekilir
                var fuarlar = veritabani.tbl_Fuar.
                    Join(veritabani.tbl_OrganizatorKatildigiFuarlar,
                    ku => ku.fuarId,
                    ki => ki.fuarId,
                    (ku, ki) => new { tbl_Fuar = ku, tbl_OrganizatorKatildigiFuarlar = ki }).
                    Where(x => x.tbl_OrganizatorKatildigiFuarlar.oId == firmaVarmi &&
                    x.tbl_OrganizatorKatildigiFuarlar.fuarId == x.tbl_Fuar.fuarId).
                    Select(x => new SelectListItem
                    {
                        Selected = false,
                        Text = x.tbl_Fuar.fuarAdi,
                        Value = x.tbl_Fuar.fuarId.ToString()
                    }).ToList();

                //viewbag ile yollanır
                ViewBag.fuarlar = fuarlar;

                var falan = veritabani.tbl_FaatliyetAlani.Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.faaliyetAlaniAdi,
                    Value = x.faaliyetAlaniId.ToString()
                }).ToList();

                ViewBag.falan = falan;

                var ftur = veritabani.tbl_FaaliyetTuru.Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.faaliyetTuruAdi,
                    Value = x.faaliyetTuruId.ToString()
                }).ToList();

                ViewBag.ftur = ftur;

                return View();
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FuarKapsamEkle(FormCollection gelenVeri)
        {
            if (gelenVeri["fuarSec"] != null)
            {
                int fuarId = Convert.ToInt32(gelenVeri["fuarSec"]);
                int i = 0;

                if (gelenVeri["faaliyetAlaniId"] != null)
                {
                    string[] cparcala = gelenVeri["faaliyetAlaniId"].Split(',');
                    int[] alanidler = new int[cparcala.Count()];
                    i = 0;
                    foreach (string id in cparcala)
                    {
                        alanidler[i] = Convert.ToInt32(id);
                        i++;
                    }

                    var fuarKapsamAlani = veritabani.tbl_FuarKapsamAlani.
                        Where(x => x.fuarId == fuarId &&
                        !alanidler.Contains(x.faaliyetAlaniId)).ToList();

                    foreach (tbl_FuarKapsamAlani fa in fuarKapsamAlani)
                    {
                        veritabani.tbl_FuarKapsamAlani.Remove(fa);
                    }

                    var yeniAlan = veritabani.tbl_FuarKapsamAlani.
                        Where(x => x.fuarId == fuarId &&
                        alanidler.Contains(x.faaliyetAlaniId)).
                        Select(x => x.faaliyetAlaniId).ToList();

                    foreach (int id in alanidler)
                    {
                        if (!yeniAlan.Contains(id))
                        {
                            tbl_FuarKapsamAlani yeniAlanEkle = new tbl_FuarKapsamAlani();
                            yeniAlanEkle.fuarId = fuarId;
                            yeniAlanEkle.faaliyetAlaniId = id;
                            veritabani.tbl_FuarKapsamAlani.Add(yeniAlanEkle);
                        }
                    }
                }
                else
                {
                    var kapsamAlani = veritabani.tbl_FuarKapsamAlani.
                        Where(x => x.fuarId == fuarId).ToList();
                    foreach (tbl_FuarKapsamAlani fa in kapsamAlani)
                    {
                        veritabani.tbl_FuarKapsamAlani.Remove(fa);
                    }
                }

                if (gelenVeri["faaliyetTuruId"] != null)
                {
                    string[] cparcala = gelenVeri["faaliyetTuruId"].Split(',');
                    int[] turidler = new int[cparcala.Count()];
                    i = 0;
                    foreach (string id in cparcala)
                    {
                        turidler[i] = Convert.ToInt32(id);
                        i++;
                    }

                    var fuarKapsamTur = veritabani.tbl_FuarKapsamTuru.
                        Where(x => x.fuarId == fuarId &&
                        !turidler.Contains(x.faaliyetTuruId)).ToList();

                    foreach (tbl_FuarKapsamTuru fa in fuarKapsamTur)
                    {
                        veritabani.tbl_FuarKapsamTuru.Remove(fa);
                    }

                    var yeniTur = veritabani.tbl_FuarKapsamTuru.
                        Where(x => x.fuarId == fuarId &&
                        turidler.Contains(x.faaliyetTuruId)).
                        Select(x => x.faaliyetTuruId).ToList();

                    foreach (int id in turidler)
                    {
                        if (!yeniTur.Contains(id))
                        {
                            tbl_FuarKapsamTuru yeniTurEkle = new tbl_FuarKapsamTuru();
                            yeniTurEkle.fuarId = fuarId;
                            yeniTurEkle.faaliyetTuruId = id;
                            veritabani.tbl_FuarKapsamTuru.Add(yeniTurEkle);
                        }
                    }
                }
                else
                {
                    var kapsamTuru = veritabani.tbl_FuarKapsamTuru.
                        Where(x => x.fuarId == fuarId).ToList();
                    foreach (tbl_FuarKapsamTuru fa in kapsamTuru)
                    {
                        veritabani.tbl_FuarKapsamTuru.Remove(fa);
                    }
                }

                veritabani.SaveChanges();
            }
            return RedirectToAction("FuarKapsamEkle", "Organizator");
        }

        [HttpPost]
        public ActionResult fuarZiyaretciProfiliGetir(int fuarSec)
        {
            var ziyaretciProfil = veritabani.tbl_FuarZiyaretciProfil.
                Where(x => x.fuarId == fuarSec).
                Select(x => new { x.zProfilId }).ToList();

            return Json(ziyaretciProfil);
        }

        [HttpPost]
        public ActionResult fuarKatilimciProfilGetir(int fuarSec)
        {
            var katilimciProfil = veritabani.tbl_FuarKatilimciProfil.
               Where(x => x.fuarId == fuarSec).
               Select(x => new { x.kProfilId }).ToList();

            return Json(katilimciProfil);
        }

        public ActionResult FuarProfiliEkle()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("organizator", kullaniciId))
            {
                var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

                //Firma varmı diye kontrol et firma id alınır
                var firmaVarmi = veritabani.tbl_OrganizatorFirma.
                    Join(veritabani.tbl_OrganizatorFirmaCalisanlar,
                    ku => ku.oId,
                    ki => ki.oId,
                    (ku, ki) => new { tbl_OrganizatorFirma = ku, tbl_OrganizatorFirmaCalisanlar = ki }).
                    Where(x => x.tbl_OrganizatorFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_OrganizatorFirma.oId == x.tbl_OrganizatorFirmaCalisanlar.oId).
                    Select(x => x.tbl_OrganizatorFirma.oId).FirstOrDefault();

                //organizatorün oluşturduğu fuarlar çekilir
                var fuarlar = veritabani.tbl_Fuar.
                    Join(veritabani.tbl_OrganizatorKatildigiFuarlar,
                    ku => ku.fuarId,
                    ki => ki.fuarId,
                    (ku, ki) => new { tbl_Fuar = ku, tbl_OrganizatorKatildigiFuarlar = ki }).
                    Where(x => x.tbl_OrganizatorKatildigiFuarlar.oId == firmaVarmi &&
                    x.tbl_OrganizatorKatildigiFuarlar.fuarId == x.tbl_Fuar.fuarId).
                    Select(x => new SelectListItem
                    {
                        Selected = false,
                        Text = x.tbl_Fuar.fuarAdi,
                        Value = x.tbl_Fuar.fuarId.ToString()
                    }).ToList();

                //viewbag ile yollanır
                ViewBag.fuarlar = fuarlar;


                var katilimciProfili = veritabani.tbl_KatilimciProfil.Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.kProfilAdi,
                    Value = x.kProfilId.ToString()
                }).ToList();

                ViewBag.kProfil = katilimciProfili;

                var ziyaretciProfil = veritabani.tbl_ZiyaretciProfil.Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.zProfilAdi,
                    Value = x.zProfilId.ToString()
                }).ToList();
                ViewBag.zProfil = ziyaretciProfil;

                return View();
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FuarProfiliEkle(FormCollection gelenVeri)
        {
            if (gelenVeri["fuarSec"] != null)
            {
                int fuarId = Convert.ToInt32(gelenVeri["fuarSec"]);
                int i = 0;

                if (gelenVeri["kProfilId"] != null)
                {
                    string[] cparcala = gelenVeri["kProfilId"].Split(',');
                    int[] kProfilIdler = new int[cparcala.Count()];
                    i = 0;
                    foreach (string id in cparcala)
                    {
                        kProfilIdler[i] = Convert.ToInt32(id);
                        i++;
                    }

                    var fuarkatilimciProfili = veritabani.tbl_FuarKatilimciProfil.
                        Where(x => x.fuarId == fuarId &&
                        !kProfilIdler.Contains(x.kProfilId)).ToList();

                    foreach (tbl_FuarKatilimciProfil fa in fuarkatilimciProfili)
                    {
                        veritabani.tbl_FuarKatilimciProfil.Remove(fa);
                    }

                    var yeniKprofil = veritabani.tbl_FuarKatilimciProfil.
                        Where(x => x.fuarId == fuarId &&
                        kProfilIdler.Contains(x.kProfilId)).
                        Select(x => x.kProfilId).ToList();

                    foreach (int id in kProfilIdler)
                    {
                        if (!yeniKprofil.Contains(id))
                        {
                            tbl_FuarKatilimciProfil yeniKprofilEkle= new tbl_FuarKatilimciProfil();
                            yeniKprofilEkle.fuarId = fuarId;
                            yeniKprofilEkle.kProfilId = id;
                            veritabani.tbl_FuarKatilimciProfil.Add(yeniKprofilEkle);
                        }
                    }
                }
                else
                {
                    var kprofil = veritabani.tbl_FuarKatilimciProfil.
                        Where(x => x.fuarId == fuarId).ToList();
                    foreach (tbl_FuarKatilimciProfil fa in kprofil)
                    {
                        veritabani.tbl_FuarKatilimciProfil.Remove(fa);
                    }
                }

                if (gelenVeri["zProfilId"] != null)
                {
                    string[] cparcala = gelenVeri["zProfilId"].Split(',');
                    int[] zProfilIdler = new int[cparcala.Count()];
                    i = 0;
                    foreach (string id in cparcala)
                    {
                        zProfilIdler[i] = Convert.ToInt32(id);
                        i++;
                    }

                    var fuarZiyaretciProfil = veritabani.tbl_FuarZiyaretciProfil.
                        Where(x => x.fuarId == fuarId &&
                        !zProfilIdler.Contains(x.zProfilId)).ToList();

                    foreach (tbl_FuarZiyaretciProfil fa in fuarZiyaretciProfil)
                    {
                        veritabani.tbl_FuarZiyaretciProfil.Remove(fa);
                    }

                    var yeniZiyaretciProfil = veritabani.tbl_FuarZiyaretciProfil.
                        Where(x => x.fuarId == fuarId &&
                        zProfilIdler.Contains(x.zProfilId)).
                        Select(x => x.zProfilId).ToList();

                    foreach (int id in zProfilIdler)
                    {
                        if (!yeniZiyaretciProfil.Contains(id))
                        {
                            tbl_FuarZiyaretciProfil yeniZiyaretciProfilEkle= new tbl_FuarZiyaretciProfil();
                            yeniZiyaretciProfilEkle.fuarId = fuarId;
                            yeniZiyaretciProfilEkle.zProfilId = id;
                            veritabani.tbl_FuarZiyaretciProfil.Add(yeniZiyaretciProfilEkle);
                        }
                    }
                }
                else
                {
                    var fuarZiyaretciProfil = veritabani.tbl_FuarZiyaretciProfil.
                        Where(x => x.fuarId == fuarId).ToList();
                    foreach (tbl_FuarZiyaretciProfil fa in fuarZiyaretciProfil)
                    {
                        veritabani.tbl_FuarZiyaretciProfil.Remove(fa);
                    }
                }

                veritabani.SaveChanges();
            }
            return RedirectToAction("FuarProfiliEkle", "Organizator");
        }
    }
}