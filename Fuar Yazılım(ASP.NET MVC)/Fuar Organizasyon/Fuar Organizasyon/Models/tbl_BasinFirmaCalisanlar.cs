//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Fuar_Organizasyon.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_BasinFirmaCalisanlar
    {
        public int bCalisanlarId { get; set; }
        public int bId { get; set; }
        public int kisiId { get; set; }
    
        public virtual tbl_BasinFirma tbl_BasinFirma { get; set; }
        public virtual tbl_Kisiler tbl_Kisiler { get; set; }
    }
}
