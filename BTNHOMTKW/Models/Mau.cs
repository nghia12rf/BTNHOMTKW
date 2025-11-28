namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Mau")]
    public partial class Mau
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Mau()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            GioHangItems = new HashSet<GioHangItem>();
        }

        [Key]
        [StringLength(30)]
        public string TenMau { get; set; }

        [StringLength(7)]
        public string MaHex { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GioHangItem> GioHangItems { get; set; }
    }
}
