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
    
    public partial class tbl_FuarKapsamAlani
    {
        public int fKapsamAlaniId { get; set; }
        public int fuarId { get; set; }
        public int faaliyetAlaniId { get; set; }
        public string diger { get; set; }
    
        public virtual tbl_FaatliyetAlani tbl_FaatliyetAlani { get; set; }
        public virtual tbl_Fuar tbl_Fuar { get; set; }
    }
}
