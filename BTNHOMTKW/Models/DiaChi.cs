namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DiaChi")]
    public partial class DiaChi
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(150)]
        public string KhachHangEmail { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(250)]
        public string DiaChiChiTiet { get; set; }

        [Required]
        [StringLength(150)]
        public string HoTenNhan { get; set; }

        [Required]
        [StringLength(15)]
        public string SDTNhan { get; set; }

        public bool MacDinh { get; set; }

        public virtual KhachHang KhachHang { get; set; }
    }
}
