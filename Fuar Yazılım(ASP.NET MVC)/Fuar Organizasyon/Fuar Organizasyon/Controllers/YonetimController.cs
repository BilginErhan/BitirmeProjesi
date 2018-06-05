using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fuar_Organizasyon.Models;
using System.Web.SessionState;

namespace Fuar_Organizasyon.Controllers
{
    public class YonetimController : Controller
    {
        FuarDatabaseEntities veritabani;
        public YonetimController()
        {
            veritabani = new FuarDatabaseEntities();
        }

        public bool kullaniciKontrol(string turKullanici,int kullaniciId)
        {
            var kullaniciTur = veritabani.tbl_Kisiler.Where(v => v.kullaniciId == kullaniciId).Select(v => v.kullaniciTuru).FirstOrDefault();

            if (kullaniciTur != null)
            {
                if (kullaniciTur.Equals(turKullanici))
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}