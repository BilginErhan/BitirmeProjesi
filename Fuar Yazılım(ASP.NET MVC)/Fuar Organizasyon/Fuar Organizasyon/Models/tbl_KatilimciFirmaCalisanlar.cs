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
    
    public partial class tbl_KatilimciFirmaCalisanlar
    {
        public int kCalisanId { get; set; }
        public int kId { get; set; }
        public int kisiId { get; set; }
    
        public virtual tbl_KatilimciFirma tbl_KatilimciFirma { get; set; }
        public virtual tbl_Kisiler tbl_Kisiler { get; set; }
    }
}
