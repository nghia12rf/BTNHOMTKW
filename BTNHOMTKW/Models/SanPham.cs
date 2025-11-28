namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SanPham")]
    public partial class SanPham
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SanPham()
        {
            AnhSanPhams = new HashSet<AnhSanPham>();
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            DanhGias = new HashSet<DanhGia>();
            GioHangItems = new HashSet<GioHangItem>();
        }

        [Key]
        [StringLength(30)]
        public string MaSP { get; set; }

        [Required]
        [StringLength(200)]
        public string TenSP { get; set; }

        public string MoTa { get; set; }

        [StringLength(150)]
        public string DanhMucTen { get; set; }

        [StringLength(200)]
        public string HinhAnhChinh { get; set; }

        [Required]
        [StringLength(40)]
        public string SKU { get; set; }

        public decimal GiaBan { get; set; }

        public int SoLuongTon { get; set; }

        public bool Active { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NgayTao { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AnhSanPham> AnhSanPhams { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DanhGia> DanhGias { get; set; }

        public virtual DanhMuc DanhMuc { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GioHangItem> GioHangItems { get; set; }
    }
}
