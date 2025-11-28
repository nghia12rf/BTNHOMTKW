namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaiKhoan")]
    public partial class TaiKhoan
    {
        [Key]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(150)]
        public string MatKhauHash { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public byte TrangThai { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NgayTao { get; set; }

        public virtual DatLaiMatKhau DatLaiMatKhau { get; set; }

        public virtual KhachHang KhachHang { get; set; }
    }
}
