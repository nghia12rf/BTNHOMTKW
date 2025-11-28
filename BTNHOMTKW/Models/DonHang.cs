namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DonHang")]
    public partial class DonHang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DonHang()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            LichSuDonHangs = new HashSet<LichSuDonHang>();
        }

        [Key]
        [StringLength(20)]
        public string MaDonHang { get; set; }

        [Required]
        [StringLength(150)]
        public string KhachHangEmail { get; set; }

        [Required]
        [StringLength(150)]
        public string HoTenNguoiNhan { get; set; }

        [Required]
        [StringLength(15)]
        public string SDTNguoiNhan { get; set; }

        [Required]
        [StringLength(250)]
        public string DiaChiGiaoHang { get; set; }

        [Required]
        [StringLength(50)]
        public string PTThanhToan { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TongTien { get; set; }

        public decimal ThanhTien { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }

        [Required]
        [StringLength(20)]
        public string TrangThaiThanhToan { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NgayDat { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }

        public virtual KhachHang KhachHang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LichSuDonHang> LichSuDonHangs { get; set; }
    }
}
