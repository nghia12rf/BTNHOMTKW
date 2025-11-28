namespace BTNHOMTKW.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LichSuDonHang")]
    public partial class LichSuDonHang
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string MaDonHang { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "datetime2")]
        public DateTime ThoiGian { get; set; }

        public virtual DonHang DonHang { get; set; }
    }
}
