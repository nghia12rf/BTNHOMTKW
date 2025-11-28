namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DanhGia")]
    public partial class DanhGia
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string MaSP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(150)]
        public string KhachHangEmail { get; set; }

        public byte SoSao { get; set; }

        [StringLength(500)]
        public string BinhLuan { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NgayDG { get; set; }

        public virtual KhachHang KhachHang { get; set; }

        public virtual SanPham SanPham { get; set; }
    }
}
