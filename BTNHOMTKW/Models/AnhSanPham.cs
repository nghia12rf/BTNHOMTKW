namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AnhSanPham")]
    public partial class AnhSanPham
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string MaSP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(200)]
        public string FileAnh { get; set; }

        public short ThuTu { get; set; }

        public virtual SanPham SanPham { get; set; }
    }
}
