namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DatLaiMatKhau")]
    public partial class DatLaiMatKhau
    {
        [Key]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(150)]
        public string Token { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime ThoiGianHetHan { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}
