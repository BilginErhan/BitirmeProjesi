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
    
    public partial class tbl_FaaliyetTuru
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_FaaliyetTuru()
        {
            this.tbl_FuarKapsamTuru = new HashSet<tbl_FuarKapsamTuru>();
            this.tbl_KatilimciFaaliyetTuru = new HashSet<tbl_KatilimciFaaliyetTuru>();
            this.tbl_OrganizatorFaaliyetTuru = new HashSet<tbl_OrganizatorFaaliyetTuru>();
            this.tbl_ZiyaretciFaaliyetAlani = new HashSet<tbl_ZiyaretciFaaliyetAlani>();
        }
    
        public int faaliyetTuruId { get; set; }
        public string faaliyetTuruAdi { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_FuarKapsamTuru> tbl_FuarKapsamTuru { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_KatilimciFaaliyetTuru> tbl_KatilimciFaaliyetTuru { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_OrganizatorFaaliyetTuru> tbl_OrganizatorFaaliyetTuru { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_ZiyaretciFaaliyetAlani> tbl_ZiyaretciFaaliyetAlani { get; set; }
    }
}
