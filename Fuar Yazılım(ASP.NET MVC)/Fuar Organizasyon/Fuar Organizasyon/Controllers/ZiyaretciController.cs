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
using System.Net.Mail;
using System.Net;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using System.Net.Mime;

namespace Fuar_Organizasyon.Controllers
{
    public class ZiyaretciController : Controller
    {

        public double[] ebias = new double[16];//bias dizileri
        public double[] ybias = new double[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        int[] firmalistesi = new int[5];

        FuarDatabaseEntities veritabani;
        YonetimController yonetim;
        int kullaniciId;

        public ZiyaretciController()
        {
            veritabani = new FuarDatabaseEntities();
            yonetim = new YonetimController();
        }



        // GET: Ziyaretci
        public ActionResult Index()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            //giriş yapılan kullanıcı ziyaretçi mi diye bakılır
            if (yonetim.kullaniciKontrol("ziyaretci", kullaniciId))
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
            if (yonetim.kullaniciKontrol("ziyaretci", kullaniciId))
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
            return RedirectToAction("kullaniciSifre", "Ziyaretci");
        }

        public ActionResult kisiselBilgi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("ziyaretci", kullaniciId))
            {
                var kisiler = veritabani.tbl_Kisiler.
                    Where(x => x.kullaniciId == kullaniciId).
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

            var resimYolu = veritabani.tbl_Kisiler.
                Where(v => v.kisiId == kisiler.kisiId).
                Select(v => v.kisiResimUrl).FirstOrDefault();

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

            tbl_Kisiler guncelKisi = veritabani.tbl_Kisiler.
                Where(v => v.kisiId == kisiler.kisiId).FirstOrDefault();

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

            return RedirectToAction("kisiselBilgi", "Ziyaretci");

        }

        //Katılımcı firma bilgi güncelleme
        public ActionResult FirmaBilgi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();


            if (yonetim.kullaniciKontrol("ziyaretci", kullaniciId))
            {


                //Firma varmı diye kontrol et firma id alınır
                var firmaVarmi = veritabani.tbl_ZiyaretciFirma.
                    Join(veritabani.tbl_ZiyaretciFirmaCalisanlar,
                    ku => ku.zId,
                    ki => ki.zId,
                    (ku, ki) => new { tbl_ZiyaretciFirma = ku, tbl_ZiyaretciFirmaCalisanlar = ki }).
                    Where(x => x.tbl_ZiyaretciFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_ZiyaretciFirma.zId == x.tbl_ZiyaretciFirmaCalisanlar.zId).
                    Select(x => x.tbl_ZiyaretciFirma.zId).FirstOrDefault();



                //Bu kullanıcıya ait firmayı çek
                var tbl_ziyaretciFirma = veritabani.tbl_ZiyaretciFirma.
                     Where(x => x.zId == firmaVarmi).FirstOrDefault();
                if (tbl_ziyaretciFirma == null)
                {
                    tbl_ziyaretciFirma = new tbl_ZiyaretciFirma();
                    tbl_ziyaretciFirma.zFirmaAdi = " ";
                    tbl_ziyaretciFirma.zWebSiteUrl = " ";
                    tbl_ziyaretciFirma.zTelefon = " ";
                    tbl_ziyaretciFirma.zFax = " ";
                    tbl_ziyaretciFirma.zMail = " ";
                    tbl_ziyaretciFirma.zOnay = kisiId.ToString();
                    tbl_ziyaretciFirma.zResimUrl = " ";
                    tbl_ziyaretciFirma.SehirId = 1;
                    tbl_ziyaretciFirma.ilceId = 1;
                    tbl_ziyaretciFirma.SemtMahId = 1;

                    veritabani.tbl_ZiyaretciFirma.Add(tbl_ziyaretciFirma);
                    veritabani.SaveChanges();

                    var katilimciId = veritabani.tbl_ZiyaretciFirma.
                        Where(x => x.zOnay == kisiId.ToString()).
                        Select(x => x.zId).FirstOrDefault();

                    tbl_ZiyaretciFirmaCalisanlar calisanlar = new tbl_ZiyaretciFirmaCalisanlar();
                    calisanlar.zId = katilimciId;
                    calisanlar.kisiId = kisiId;
                    veritabani.tbl_ZiyaretciFirmaCalisanlar.Add(calisanlar);
                    veritabani.SaveChanges();

                }


                //Şehileri çek viewbag ile yolla
                var sehirler = veritabani.Sehirler.Select(v => new SelectListItem
                {
                    Selected = v.SehirId == tbl_ziyaretciFirma.SehirId,
                    Text = v.SehirAdi,
                    Value = v.SehirId.ToString()
                }).ToList();

                ViewBag.Sehirler = sehirler;

                //ilçerleri çek viewbag ile yolla
                var ilceler = veritabani.Ilceler.Where(x => x.ilceId == tbl_ziyaretciFirma.ilceId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.ilceId == tbl_ziyaretciFirma.ilceId,
                        Text = v.IlceAdi,
                        Value = v.ilceId.ToString()
                    }).ToList();

                ViewBag.Ilceler = ilceler;

                //mahalleleri çek viewbag ile yolla
                var mahalle = veritabani.SemtMah.Where(x => x.SemtMahId == tbl_ziyaretciFirma.SemtMahId).
                    Select(v => new SelectListItem
                    {
                        Selected = v.SemtMahId == tbl_ziyaretciFirma.SemtMahId,
                        Text = v.MahalleAdi,
                        Value = v.SemtMahId.ToString()
                    }).ToList();

                ViewBag.Mahalleler = mahalle;

                //Bu firmada çalısanları çek ve viewbag ile yolla
                var tbl_kisiler = veritabani.tbl_Kisiler.Where(x => x.kullaniciTuru.Equals("ziyaretci")).
                     Select(v => new SelectListItem
                     {
                         Selected = false,
                         Text = v.kisiIsim + "  " + v.kisiSoyisim,
                         Value = v.kisiId.ToString()
                     }).ToList();

                var ziyaretciCalisanlar = veritabani.tbl_ZiyaretciFirmaCalisanlar.
                    Where(x => x.zId == firmaVarmi).Select(v => v.kisiId).ToList();

                //organizator firmada calisanların selected ının true yap
                foreach (int id in ziyaretciCalisanlar)
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
                var tbl_ziyaretciFaaliyetAlani = veritabani.tbl_ZiyaretciFaaliyetAlani.
                    Where(v => v.zId == firmaVarmi).Select(v => v.faaliyetAlaniId).ToList();

                foreach (int id in tbl_ziyaretciFaaliyetAlani)
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

                var ziyaretciFaaliyetTuru = veritabani.tbl_ZiyaretciFaaliyetTuru.
                    Where(x => x.zId == firmaVarmi).Select(x => x.faaliyetTuruId).ToList();

                foreach (int id in ziyaretciFaaliyetTuru)
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

                var ziyaretAmac = veritabani.tbl_ZiyaretciKatilimAmaci.
                    Where(x => x.zId == firmaVarmi).Select(x => x.amacId).ToList();

                foreach (int id in ziyaretAmac)
                {
                    foreach (SelectListItem amacid in tbl_Amac)
                    {
                        if (id.ToString() == amacid.Value)
                        {
                            amacid.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.Amac = tbl_Amac;

                var tbl_urunGrup = veritabani.tbl_UrunGruplari.Select(v => new SelectListItem
                {
                    Selected = false,
                    Text = v.urunGrupAdi,
                    Value = v.urunId.ToString()
                }).ToList();

                var ziyaretIlgiUrun = veritabani.tbl_ZiyaretciIlgilendigiUrun.
                    Where(x => x.zId == firmaVarmi).Select(x => x.urunId).ToList();

                foreach (int id in ziyaretIlgiUrun)
                {
                    foreach (SelectListItem urunid in tbl_urunGrup)
                    {
                        if (id.ToString() == urunid.Value)
                        {
                            urunid.Selected = true;
                            break;
                        }
                    }
                }

                ViewBag.ilgiUrun = tbl_urunGrup;

                ViewBag.resimUrl = "~/Content/Resimler/FirmaResim/" + tbl_ziyaretciFirma.zResimUrl;

                return View(tbl_ziyaretciFirma);

            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FirmaBilgi(tbl_ZiyaretciFirma ziyaretciFirma,
            HttpPostedFileBase resimYukle, FormCollection deneme)
        {

            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            var kisiId = veritabani.tbl_Kisiler.
                Where(x => x.kullaniciId == kullaniciId).
                Select(x => x.kisiId).FirstOrDefault();

            //Firma varmı diye kontrol et firma id alınır
            var firmaVarmi = veritabani.tbl_ZiyaretciFirma.
                Join(veritabani.tbl_ZiyaretciFirmaCalisanlar,
                ku => ku.zId,
                ki => ki.zId,
                (ku, ki) => new { tbl_ZiyaretciFirma = ku, tbl_ZiyaretciFirmaCalisanlar = ki }).
                Where(x => x.tbl_ZiyaretciFirmaCalisanlar.kisiId == kisiId
                && x.tbl_ZiyaretciFirma.zId == x.tbl_ZiyaretciFirmaCalisanlar.zId).
                Select(x => x.tbl_ZiyaretciFirma.zId).FirstOrDefault();

            //Resim Kayıt edilir
            var resimYolu = veritabani.tbl_ZiyaretciFirma.Where(v => v.zId == ziyaretciFirma.zId).
                Select(v => v.zResimUrl).FirstOrDefault();
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




            tbl_ZiyaretciFirma firma = veritabani.tbl_ZiyaretciFirma.
                Where(x => x.zId == firmaVarmi).FirstOrDefault();

            if (firma != null)
            {
                //Firma güncelleme kısmı
                firma.zFirmaAdi = ziyaretciFirma.zFirmaAdi;
                firma.zWebSiteUrl = ziyaretciFirma.zWebSiteUrl;
                firma.zTelefon = ziyaretciFirma.zTelefon;
                firma.zFax = ziyaretciFirma.zFax;
                firma.zMail = ziyaretciFirma.zMail;
                firma.zResimUrl = benzersiz;
                firma.SehirId = ziyaretciFirma.SehirId;
                firma.ilceId = ziyaretciFirma.ilceId;
                firma.SemtMahId = ziyaretciFirma.SemtMahId;
                firma.zCalisanSayisi = ziyaretciFirma.zCalisanSayisi;
                firma.zOnay = "Beklemede";
            }
            else
            {
                ziyaretciFirma.zResimUrl = benzersiz;
                ziyaretciFirma.zOnay = "Beklemede";
                veritabani.tbl_ZiyaretciFirma.Add(ziyaretciFirma);

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

                var ziyaretciCalisan = veritabani.tbl_ZiyaretciFirmaCalisanlar.
                    Where(x => x.zId == firmaVarmi && !kisiIdler.Contains(x.kisiId)).ToList();
                foreach (tbl_ZiyaretciFirmaCalisanlar fa in ziyaretciCalisan)
                {
                    veritabani.tbl_ZiyaretciFirmaCalisanlar.Remove(fa);
                }//seçili olmayan yani silinmek istenen çalışan veritabanından silinir

                var yeniCalisan = veritabani.tbl_ZiyaretciFirmaCalisanlar.
                    Where(x => x.zId == firmaVarmi && kisiIdler.Contains(x.kisiId)).
                    Select(x => x.kisiId).ToList();

                foreach (int id in kisiIdler)
                {
                    if (!yeniCalisan.Contains(id))
                    {
                        tbl_ZiyaretciFirmaCalisanlar calisanekle = new tbl_ZiyaretciFirmaCalisanlar();
                        calisanekle.zId = firmaVarmi;
                        calisanekle.kisiId = id;
                        veritabani.tbl_ZiyaretciFirmaCalisanlar.Add(calisanekle);
                    }
                }
            }
            else
            {
                var ziyaretciCalisan = veritabani.tbl_ZiyaretciFirmaCalisanlar.
                    Where(x => x.zId == firmaVarmi).ToList();
                foreach (tbl_ZiyaretciFirmaCalisanlar fa in ziyaretciCalisan)
                {
                    veritabani.tbl_ZiyaretciFirmaCalisanlar.Remove(fa);
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

                var ziyaretciAlanlar = veritabani.tbl_ZiyaretciFaaliyetAlani.
                    Where(x => x.zId == firmaVarmi && !alanidler.Contains(x.faaliyetAlaniId)).ToList();

                foreach (tbl_ZiyaretciFaaliyetAlani fa in ziyaretciAlanlar)
                {
                    veritabani.tbl_ZiyaretciFaaliyetAlani.Remove(fa);
                }

                var yeniAlan = veritabani.tbl_ZiyaretciFaaliyetAlani.
                    Where(x => x.zId == firmaVarmi
                    && alanidler.Contains(x.faaliyetAlaniId)).
                    Select(x => x.faaliyetAlaniId).ToList();

                foreach (int id in alanidler)
                {
                    if (!yeniAlan.Contains(id))
                    {
                        tbl_ZiyaretciFaaliyetAlani yeniAlanEkle = new tbl_ZiyaretciFaaliyetAlani();
                        yeniAlanEkle.zId = firmaVarmi;
                        yeniAlanEkle.faaliyetAlaniId = id;
                        veritabani.tbl_ZiyaretciFaaliyetAlani.Add(yeniAlanEkle);
                    }
                }
            }
            else
            {
                var faaliyetAlani = veritabani.tbl_ZiyaretciFaaliyetAlani.
                    Where(x => x.zId == firmaVarmi).ToList();
                foreach (tbl_ZiyaretciFaaliyetAlani fa in faaliyetAlani)
                {
                    veritabani.tbl_ZiyaretciFaaliyetAlani.Remove(fa);
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

                var ziyaretciFaTur = veritabani.tbl_ZiyaretciFaaliyetTuru.
                    Where(x => x.zId == firmaVarmi
                    && !turidler.Contains(x.faaliyetTuruId)).ToList();

                foreach (tbl_ZiyaretciFaaliyetTuru fa in ziyaretciFaTur)
                {
                    veritabani.tbl_ZiyaretciFaaliyetTuru.Remove(fa);
                }

                var yeniTur = veritabani.tbl_ZiyaretciFaaliyetTuru.
                    Where(x => x.zId == firmaVarmi
                    && turidler.Contains(x.faaliyetTuruId)).
                    Select(x => x.faaliyetTuruId).ToList();

                foreach (int id in turidler)
                {
                    if (!yeniTur.Contains(id))
                    {
                        tbl_ZiyaretciFaaliyetTuru yeniTurEkle = new tbl_ZiyaretciFaaliyetTuru();
                        yeniTurEkle.zId = firmaVarmi;
                        yeniTurEkle.faaliyetTuruId = id;
                        veritabani.tbl_ZiyaretciFaaliyetTuru.Add(yeniTurEkle);
                    }
                }
            }
            else
            {
                var faaliyetTuru = veritabani.tbl_ZiyaretciFaaliyetTuru.
                    Where(x => x.zId == firmaVarmi).ToList();
                foreach (tbl_ZiyaretciFaaliyetTuru fa in faaliyetTuru)
                {
                    veritabani.tbl_ZiyaretciFaaliyetTuru.Remove(fa);
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

                var ziyaretAmac = veritabani.tbl_ZiyaretciKatilimAmaci.
                    Where(x => x.zId == firmaVarmi
                    && !amacidler.Contains(x.amacId)).ToList();

                foreach (tbl_ZiyaretciKatilimAmaci fa in ziyaretAmac)
                {
                    veritabani.tbl_ZiyaretciKatilimAmaci.Remove(fa);
                }

                var yeniAmac = veritabani.tbl_ZiyaretciKatilimAmaci.
                    Where(x => x.zId == firmaVarmi
                    && amacidler.Contains(x.amacId)).
                    Select(x => x.amacId).ToList();

                foreach (int id in amacidler)
                {
                    if (!yeniAmac.Contains(id))
                    {
                        tbl_ZiyaretciKatilimAmaci yeniAmacEkle = new tbl_ZiyaretciKatilimAmaci();
                        yeniAmacEkle.zId = firmaVarmi;
                        yeniAmacEkle.amacId = id;
                        veritabani.tbl_ZiyaretciKatilimAmaci.Add(yeniAmacEkle);
                    }
                }

            }
            else
            {
                var ziyaretAmac = veritabani.tbl_ZiyaretciKatilimAmaci.
                    Where(x => x.zId == firmaVarmi).ToList();
                foreach (tbl_ZiyaretciKatilimAmaci fa in ziyaretAmac)
                {
                    veritabani.tbl_ZiyaretciKatilimAmaci.Remove(fa);
                }
            }


            if (deneme["urunGrup"] != null)
            {
                string[] cparcala4 = deneme["urunGrup"].Split(',');//faaliyet türü idleri çek
                int[] urunIdler = new int[cparcala4.Count()];
                i = 0;
                foreach (string id in cparcala4)
                {
                    urunIdler[i] = Convert.ToInt32(id);
                    i++;
                }

                var ziyaretciIlgiUrun = veritabani.tbl_ZiyaretciIlgilendigiUrun.
                    Where(x => x.zId == firmaVarmi
                    && !urunIdler.Contains(x.urunId)).ToList();

                foreach (tbl_ZiyaretciIlgilendigiUrun fa in ziyaretciIlgiUrun)
                {
                    veritabani.tbl_ZiyaretciIlgilendigiUrun.Remove(fa);
                }

                var yeniIlgiUrun = veritabani.tbl_ZiyaretciIlgilendigiUrun.
                    Where(x => x.zId == firmaVarmi
                    && urunIdler.Contains(x.urunId)).
                    Select(x => x.urunId).ToList();

                foreach (int id in urunIdler)
                {
                    if (!yeniIlgiUrun.Contains(id))
                    {
                        tbl_ZiyaretciIlgilendigiUrun yeniIlgiUrunEkle = new tbl_ZiyaretciIlgilendigiUrun();
                        yeniIlgiUrunEkle.zId = firmaVarmi;
                        yeniIlgiUrunEkle.urunId = id;
                        veritabani.tbl_ZiyaretciIlgilendigiUrun.Add(yeniIlgiUrunEkle);
                    }
                }

            }
            else
            {
                var ilgiUrun = veritabani.tbl_ZiyaretciIlgilendigiUrun.
                    Where(x => x.zId == firmaVarmi).ToList();
                foreach (tbl_ZiyaretciIlgilendigiUrun fa in ilgiUrun)
                {
                    veritabani.tbl_ZiyaretciIlgilendigiUrun.Remove(fa);
                }
            }

            veritabani.SaveChanges();

            return RedirectToAction("FirmaBilgi", "Ziyaretci");
        }

        public ActionResult FuarListesi()
        {
            kullaniciId = Convert.ToInt32(Session["kullanici"]);

            //giriş yapılan kullanıcı ziyaretçi mi diye bakılır
            if (yonetim.kullaniciKontrol("ziyaretci", kullaniciId))
            {
                var kisiId = veritabani.tbl_Kisiler.Where(x => x.kullaniciId == kullaniciId).
                   Select(x => x.kisiId).FirstOrDefault();

                //Firma varmı diye kontrol et firma id alınır
                var firmaId = veritabani.tbl_ZiyaretciFirma.
                    Join(veritabani.tbl_ZiyaretciFirmaCalisanlar,
                    ku => ku.zId,
                    ki => ki.zId,
                    (ku, ki) => new { tbl_ZiyaretciFirma = ku, tbl_ZiyaretciFirmaCalisanlar = ki }).
                    Where(x => x.tbl_ZiyaretciFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_ZiyaretciFirma.zId == x.tbl_ZiyaretciFirmaCalisanlar.zId).
                    Select(x => x.tbl_ZiyaretciFirma.zId).FirstOrDefault();
                if (firmaId != 0)
                {
                    var firmaKatilidigiFuarlar = veritabani.tbl_ZiyaretciKatildigiFuarlar.
                    Where(x => x.zId == firmaId).Select(x => x.fuarId).ToList();

                    ViewBag.fKatildigi = firmaKatilidigiFuarlar;

                    var fuarlar = veritabani.tbl_Fuar.
                        Where(x => !firmaKatilidigiFuarlar.Contains(x.fuarId)).ToList();

                    return View(fuarlar);
                }
                else
                {
                    return RedirectToAction("index", "Ziyaretci");
                }
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        [HttpGet]
        public ActionResult KatildiginizFuarlar(int id)
        {
            int fuarId = id;
            kullaniciId = Convert.ToInt32(Session["kullanici"]);
            if (yonetim.kullaniciKontrol("ziyaretci", kullaniciId))
            {
                var kisiId = veritabani.tbl_Kisiler.Where(x => x.kullaniciId == kullaniciId).
                   Select(x => x.kisiId).FirstOrDefault();

                var fuarAdi = veritabani.tbl_Fuar.Where(x => x.fuarId == fuarId).
                    Select(x => x.fuarAdi).FirstOrDefault();

                //Firma varmı diye kontrol et firma id alınır
                var firmaId = veritabani.tbl_ZiyaretciFirma.
                    Join(veritabani.tbl_ZiyaretciFirmaCalisanlar,
                    ku => ku.zId,
                    ki => ki.zId,
                    (ku, ki) => new { tbl_ZiyaretciFirma = ku, tbl_ZiyaretciFirmaCalisanlar = ki }).
                    Where(x => x.tbl_ZiyaretciFirmaCalisanlar.kisiId == kisiId
                    && x.tbl_ZiyaretciFirma.zId == x.tbl_ZiyaretciFirmaCalisanlar.zId).
                    Select(x => x.tbl_ZiyaretciFirma.zId).FirstOrDefault();
                if (firmaId != 0)
                {



                    tbl_ZiyaretciKatildigiFuarlar ziyaretciFirmaKatil = new tbl_ZiyaretciKatildigiFuarlar();

                    ziyaretciFirmaKatil.zId = firmaId;
                    ziyaretciFirmaKatil.fuarId = fuarId;
                    veritabani.tbl_ZiyaretciKatildigiFuarlar.Add(ziyaretciFirmaKatil);
                    veritabani.SaveChanges();

                    var calisanlar = veritabani.tbl_ZiyaretciFirmaCalisanlar.
                        Join(veritabani.tbl_Kisiler,
                        ku => ku.kisiId,
                        ki => ki.kisiId,
                        (ku, ki) => new { tbl_ZiyaretciFirmaCalisanlar = ku, tbl_Kisiler = ki }).
                        Where(x => x.tbl_ZiyaretciFirmaCalisanlar.zId == firmaId).
                        Select(x => x.tbl_ZiyaretciFirmaCalisanlar).ToList();

                    foreach (tbl_ZiyaretciFirmaCalisanlar fa in calisanlar)
                    {
                        //qr code oluşturma kısmı
                        MessagingToolkit.QRCode.Codec.QRCodeEncoder encoder = new MessagingToolkit.QRCode.Codec.QRCodeEncoder();
                        encoder.QRCodeScale = 8;
                        string bilgi = "KisiId=" + fa.tbl_Kisiler.kisiId + ",isim=" + fa.tbl_Kisiler.kisiIsim + ",soyisim=" + fa.tbl_Kisiler.kisiSoyisim;
                        Bitmap bmp = encoder.Encode(bilgi);

                        //resim memorystreame jpeg olarak kayıt edilir
                        var resim = new MemoryStream();
                        bmp.Save(resim, ImageFormat.Jpeg);
                        resim.Position = 0;


                        var body = new StringBuilder();
                        body.AppendLine("Merhabalar " + fa.tbl_Kisiler.kisiIsim + " " + fa.tbl_Kisiler.kisiSoyisim + ", <br />");
                        body.AppendLine(fuarAdi + " isimli fuara katılımız gerçekleştirilmiştir qr code resminiz ekte mevcuttur. <br />");
                        body.AppendLine("İyi günler dileriz. <br />");
                        body.AppendLine("<br /><br />Fuar Yazılım Bilgilendirme Mesajı");
                        MailSender(fa.tbl_Kisiler.kisiMail, body.ToString(), resim);

                    }

                    //ziyaretçi firmanın katıldığı fuarlar database den çekilir
                    var fuar = veritabani.tbl_Fuar.
                        Join(veritabani.tbl_ZiyaretciKatildigiFuarlar,
                        ku => ku.fuarId,
                        ki => ki.fuarId,
                        (ku, ki) => new { tbl_Fuar = ku, tbl_ZiyaretciKatildigiFuarlar = ki }).
                        Where(x => x.tbl_Fuar.fuarId == x.tbl_ZiyaretciKatildigiFuarlar.fuarId &&
                        x.tbl_ZiyaretciKatildigiFuarlar.zId == firmaId).
                        Select(x => x.tbl_Fuar).ToList();

                    return View(fuar);
                }
                else
                {//eğer kullanıcı bir firmaya dahil değilse ana sayfaya yönlendirir.
                    return RedirectToAction("index", "Ziyaretci");
                }


            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        //smtp mail yollama
        public static void MailSender(string mail, string body, MemoryStream resim)
        {
            /* SmtpClient mailClient = new SmtpClient("smtp.gmail.com", 587);
             NetworkCredential cred = new NetworkCredential("sistemfuar@gmail.com", "31106188");
             mailClient.Credentials = cred;
             MailMessage contact = new MailMessage();

             ContentType contentType = new ContentType();
             contentType.MediaType = MediaTypeNames.Image.Jpeg;
             contentType.Name = "qrcode.Jpeg";

             contact.From = new MailAddress("sistemfuar@gmail.com");
             contact.Subject = "Fuar yazılım | E-Davetiye";
             contact.IsBodyHtml = true;
             contact.Body = body;
             contact.Attachments.Add(new Attachment(resim, "qrcode.jpeg"));
             mailClient.EnableSsl = true;
             mailClient.UseDefaultCredentials = false;
             contact.To.Add(mail);
             mailClient.Send(contact);*/

            MailMessage mail1 = new MailMessage();
            mail1.IsBodyHtml = true;
            mail1.To.Add(mail); //mail gönderilen adres 
            mail1.From = new MailAddress("sistemfuar@gmail.com"); //maili gönderen adres 
            mail1.Subject = "Fuar yazılım | E - Davetiye";
            mail1.Body = body;
            mail1.Attachments.Add(new Attachment(resim, "qrcode.jpeg"));
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com"; //mail serverının host bilgisi 
            smtp.Port = 587; //mail serverının portu 
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("sistemfuar@gmail.com", "31106188"); //mail serverının kullanıcı bilgileri 
            smtp.Send(mail1);
        }

        public ActionResult Oneri()
        {
            var list = new List<CheckModel>
            {
                 new CheckModel{Id = 1, Name = "Facebook", Checked = false},
                 new CheckModel{Id = 2, Name = "Youtube", Checked = false},
                 new CheckModel{Id = 3, Name = "Aselsan", Checked = false},
                 new CheckModel{Id = 4, Name = "Trakya Tarım", Checked = false},
                 new CheckModel{Id = 5, Name = "Erüst Tarım", Checked = false},
                 new CheckModel{Id = 6, Name = "Ford", Checked = false},
                 new CheckModel{Id = 7, Name = "Honda", Checked = false},
                 new CheckModel{Id = 8, Name = "Nintendo", Checked = false},
                 new CheckModel{Id = 9, Name = "Burger King", Checked = false},
                 new CheckModel{Id = 10, Name = "Banvit", Checked = false},
                 new CheckModel{Id = 11, Name = "Tat Gıda", Checked = false},
                 new CheckModel{Id = 12, Name = "Tüpraş", Checked = false},
                 new CheckModel{Id = 13, Name = "Türk Hava Yolları", Checked = false},
                 new CheckModel{Id = 14, Name = "Türkiye Elektrik İletim A.Ş", Checked = false},
                 new CheckModel{Id = 15, Name = "Univeler", Checked = false},
                 new CheckModel{Id = 16, Name = "Tusaş", Checked = false},
                 new CheckModel{Id = 17, Name = "İndeks", Checked = false},
                 new CheckModel{Id = 18, Name = "Aselsan", Checked = false},
                 new CheckModel{Id = 19, Name = "Çalık Enerji", Checked = false},
                 new CheckModel{Id = 20, Name = "Pegasus", Checked = false},
                 new CheckModel{Id = 21, Name = "Aksa Enerji", Checked = false},
                 new CheckModel{Id = 22, Name = "Teknosa", Checked = false},
                 new CheckModel{Id = 23, Name = "Gübretaş", Checked = false},
                 new CheckModel{Id = 24, Name = "Konya Şeker", Checked = false},
                 new CheckModel{Id = 25, Name = "Namet", Checked = false},
                 new CheckModel{Id = 26, Name = "Evkur", Checked = false},
                 new CheckModel{Id = 27, Name = "İhlas Holding", Checked = false},
                 new CheckModel{Id = 28, Name = "Netaş", Checked = false},
                 new CheckModel{Id = 29, Name = "Hidromek", Checked = false},
                 new CheckModel{Id = 30, Name = "Nuh Çimento", Checked = false},
                 new CheckModel{Id = 31, Name = "Koç Sistem", Checked = false},
                 new CheckModel{Id = 32, Name = "Dyo", Checked = false},
                 new CheckModel{Id = 33, Name = "Tekzen", Checked = false},
                 new CheckModel{Id = 34, Name = "Kent", Checked = false},
                 new CheckModel{Id = 35, Name = "Jotun", Checked = false},
                 new CheckModel{Id = 36, Name = "Havaş", Checked = false},
                 new CheckModel{Id = 37, Name = "Havelsan", Checked = false},
                 new CheckModel{Id = 38, Name = "Helvacızade", Checked = false},

            };

            return View(list);
        }

        [HttpPost]
        public ActionResult Oneri(List<CheckModel> list)
        {
            var egitim = veritabani.EgitimTablo.ToList();
            foreach (var eg in egitim)
            {//sıfırla
                eg.C16 = null;
            }
            veritabani.SaveChanges();


            int[] item = new int[38];
            int q = 0;
            foreach(var item1 in list)
            {
                if (item1.Checked)
                {
                    item[q] = item1.Id;
                    q++;
                }
            }

            var egitimdegistir = veritabani.EgitimTablo.Where(x => item.Contains(x.firmaid)).ToList();
            foreach (var item2 in egitimdegistir)
            {
                item2.C16 = 1;
            }//seçtiği firmların c16 ıncısını 1 yap
            veritabani.SaveChanges();


            var egitimtablo = veritabani.EgitimTablo.ToList();
            for(var i = 0; i < egitimtablo.Count; i++)
            {
                egitimtablo[i].C1 *= 0.01;
                egitimtablo[i].C2 *= 0.01;
                egitimtablo[i].C3 *= 0.01;
                egitimtablo[i].C4 *= 0.01;
                egitimtablo[i].C5 *= 0.01;
                egitimtablo[i].C6 *= 0.01;
                egitimtablo[i].C7 *= 0.01;
                egitimtablo[i].C8 *= 0.01;
                egitimtablo[i].C9 *= 0.01;
                egitimtablo[i].C10 *= 0.01;
                egitimtablo[i].C11 *= 0.01;
                egitimtablo[i].C12 *= 0.01;
                egitimtablo[i].C13 *= 0.01;
                egitimtablo[i].C14 *= 0.01;
                egitimtablo[i].C15 *= 0.01;

            }
            //--------------------------------------------------------------
            //bias
            double reg = 0;
            for (var j = 0; j < 100; j++)
            {
                reg = 0;
                foreach (var item3 in egitimtablo)
                {
                    reg = reg + bias(item3) - Convert.ToDouble(item3.C16);
                }

                ebias[0] = ybias[0] - (0.0001 * (1 / egitimtablo.Count - 1)) * reg;
                reg = 0;

                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C1);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C2);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C3);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C4);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C5);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C6);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C7);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C8);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C9);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C10);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C11);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C12);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C13);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C14);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;
                foreach (var item4 in egitimtablo)
                    reg = reg + (bias(item4) - Convert.ToDouble(item4.C16)) * Convert.ToDouble(item4.C15);
                ebias[q] = ybias[q] - (0.0001 * (1 / (egitimtablo.Count - 1)) * reg);
                reg = 0;

                for (var z = 0; z < 16; z++)
                {//son olarak güncel bias değerleri ybias a atılır
                    ybias[z] = ebias[z];
                }
            }

            var test = veritabani.TestTablo.ToList();
            foreach (var item5 in test)
            {
                item5.C16 = bias1(item5);
            }


            TestTablo temp;
            for (var f = 0; f < test.Count; f++)
            {//dizi büyük biasdan küçüğe doğru sıralanır
                for (var g = 0; g < test.Count; g++)
                {
                    if (test[g].C16 < test[f].C16)
                    {
                        temp = test[f];
                        test[f] = test[g];
                        test[g] = temp;
                    }
                }
            }
            //-----------------------------------

            

            firmalistesi[0] = Convert.ToInt32(test[0].firmaid);
            firmalistesi[1] = Convert.ToInt32(test[1].firmaid);
            firmalistesi[2] = Convert.ToInt32(test[2].firmaid);
            firmalistesi[3] = Convert.ToInt32(test[3].firmaid);
            firmalistesi[4] = Convert.ToInt32(test[4].firmaid);
            List<tbl_KatilimciFirma> firmalar = veritabani.tbl_KatilimciFirma.Where(x => firmalistesi.Contains(x.kId)).ToList();
            // Onerilen(firmalar);

            //            return RedirectToAction("Onerilen", "Ziyaretci",new { firmal = firmalar });

            TempData["firmal"] = firmalar;

            return RedirectToAction("Onerilen", "Ziyaretci");
        }
        public double bias(EgitimTablo item)
        {
            double sonuc;
            double e = ybias[0];
            e = e + (Convert.ToDouble(item.C1) * ybias[0]);
            e = e + (Convert.ToDouble(item.C2) * ybias[1]);
            e = e + (Convert.ToDouble(item.C3) * ybias[2]);
            e = e + (Convert.ToDouble(item.C4) * ybias[3]);
            e = e + (Convert.ToDouble(item.C5) * ybias[4]);
            e = e + (Convert.ToDouble(item.C6) * ybias[5]);
            e = e + (Convert.ToDouble(item.C7) * ybias[6]);
            e = e + (Convert.ToDouble(item.C8) * ybias[7]);
            e = e + (Convert.ToDouble(item.C9) * ybias[8]);
            e = e + (Convert.ToDouble(item.C10) * ybias[9]);
            e = e + (Convert.ToDouble(item.C11) * ybias[10]);
            e = e + (Convert.ToDouble(item.C12) * ybias[11]);
            e = e + (Convert.ToDouble(item.C13) * ybias[12]);
            e = e + (Convert.ToDouble(item.C14) * ybias[13]);
            e = e + (Convert.ToDouble(item.C15) * ybias[14]);
            e = -1 * e;
            double b1 = Math.Exp(e);
            b1++;
            sonuc = 1 / b1;
            return sonuc;
        }
        public double bias1(TestTablo item)
        {
            double sonuc;
            double e = ybias[0];
            e = e + (Convert.ToDouble(item.C1) * ybias[0]);
            e = e + (Convert.ToDouble(item.C2) * ybias[1]);
            e = e + (Convert.ToDouble(item.C3) * ybias[2]);
            e = e + (Convert.ToDouble(item.C4) * ybias[3]);
            e = e + (Convert.ToDouble(item.C5) * ybias[4]);
            e = e + (Convert.ToDouble(item.C6) * ybias[5]);
            e = e + (Convert.ToDouble(item.C7) * ybias[6]);
            e = e + (Convert.ToDouble(item.C8) * ybias[7]);
            e = e + (Convert.ToDouble(item.C9) * ybias[8]);
            e = e + (Convert.ToDouble(item.C10) * ybias[9]);
            e = e + (Convert.ToDouble(item.C11) * ybias[10]);
            e = e + (Convert.ToDouble(item.C12) * ybias[11]);
            e = e + (Convert.ToDouble(item.C13) * ybias[12]);
            e = e + (Convert.ToDouble(item.C14) * ybias[13]);
            e = e + (Convert.ToDouble(item.C15) * ybias[14]);
            e = -1 * e;
            double b1 = Math.Exp(e);
            b1++;
            sonuc = 1 / b1;
            return sonuc;
        }
        public ActionResult Onerilen()
        {
            //var firmalar = veritabani.tbl_KatilimciFirma.Where(x => firmalistesi.Contains(x.kId)).ToList();
            List<tbl_KatilimciFirma> firmal = (List<tbl_KatilimciFirma>)TempData["firmal"];

            var egitim = veritabani.EgitimTablo.ToList();
            foreach (var eg in egitim)
            {//sıfırla
                eg.C16 = null;
            }
            veritabani.SaveChanges();
            return View(firmal);
        }
    }
}