namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GioHang")]
    public partial class GioHang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GioHang()
        {
            GioHangItems = new HashSet<GioHangItem>();
        }

        [Key]
        [StringLength(60)]
        public string SessionKey { get; set; }

        [StringLength(150)]
        public string KhachHangEmail { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NgayTao { get; set; }

        public virtual KhachHang KhachHang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GioHangItem> GioHangItems { get; set; }
    }
}
