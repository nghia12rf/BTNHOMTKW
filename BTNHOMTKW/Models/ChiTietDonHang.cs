namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChiTietDonHang")]
    public partial class ChiTietDonHang
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string MaDonHang { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string MaSP { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string TenSize { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(30)]
        public string TenMau { get; set; }

        public short SoLuong { get; set; }

        public decimal DonGia { get; set; }

        public virtual DonHang DonHang { get; set; }

        public virtual Mau Mau { get; set; }

        public virtual Size Size { get; set; }

        public virtual SanPham SanPham { get; set; }
    }
}
