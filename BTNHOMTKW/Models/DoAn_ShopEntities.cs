using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace BTNHOMTKW.Models
{
    public partial class DoAn_ShopEntities : DbContext
    {
        public DoAn_ShopEntities()
            : base("name=DoAn_ShopEntities")
        {
        }

        public virtual DbSet<AnhSanPham> AnhSanPhams { get; set; }
        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual DbSet<DanhGia> DanhGias { get; set; }
        public virtual DbSet<DanhMuc> DanhMucs { get; set; }
        public virtual DbSet<DatLaiMatKhau> DatLaiMatKhaus { get; set; }
        public virtual DbSet<DiaChi> DiaChis { get; set; }
        public virtual DbSet<DonHang> DonHangs { get; set; }
        public virtual DbSet<GioHang> GioHangs { get; set; }
        public virtual DbSet<GioHangItem> GioHangItems { get; set; }
        public virtual DbSet<KhachHang> KhachHangs { get; set; }
        public virtual DbSet<LichSuDonHang> LichSuDonHangs { get; set; }
        public virtual DbSet<Mau> Maus { get; set; }
        public virtual DbSet<SanPham> SanPhams { get; set; }
        public virtual DbSet<Size> Sizes { get; set; }
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChiTietDonHang>()
                .Property(e => e.DonGia)
                .HasPrecision(12, 2);

            modelBuilder.Entity<DanhGia>()
                .Property(e => e.KhachHangEmail)
                .IsUnicode(false);

            modelBuilder.Entity<DanhMuc>()
                .HasMany(e => e.DanhMuc1)
                .WithOptional(e => e.DanhMuc2)
                .HasForeignKey(e => e.ParentDM);

            modelBuilder.Entity<DanhMuc>()
                .HasMany(e => e.SanPhams)
                .WithOptional(e => e.DanhMuc)
                .HasForeignKey(e => e.DanhMucTen);

            modelBuilder.Entity<DatLaiMatKhau>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<DatLaiMatKhau>()
                .Property(e => e.Token)
                .IsUnicode(false);

            modelBuilder.Entity<DiaChi>()
                .Property(e => e.KhachHangEmail)
                .IsUnicode(false);

            modelBuilder.Entity<DiaChi>()
                .Property(e => e.SDTNhan)
                .IsUnicode(false);

            modelBuilder.Entity<DonHang>()
                .Property(e => e.KhachHangEmail)
                .IsUnicode(false);

            modelBuilder.Entity<DonHang>()
                .Property(e => e.SDTNguoiNhan)
                .IsUnicode(false);

            modelBuilder.Entity<DonHang>()
                .Property(e => e.PhiVanChuyen)
                .HasPrecision(12, 2);

            modelBuilder.Entity<DonHang>()
                .Property(e => e.TongTien)
                .HasPrecision(12, 2);

            modelBuilder.Entity<DonHang>()
                .Property(e => e.ThanhTien)
                .HasPrecision(12, 2);

            modelBuilder.Entity<GioHang>()
                .Property(e => e.KhachHangEmail)
                .IsUnicode(false);

            modelBuilder.Entity<KhachHang>()
                .Property(e => e.TaiKhoanEmail)
                .IsUnicode(false);

            modelBuilder.Entity<KhachHang>()
                .Property(e => e.SDT)
                .IsUnicode(false);

            modelBuilder.Entity<KhachHang>()
                .HasMany(e => e.DanhGias)
                .WithRequired(e => e.KhachHang)
                .HasForeignKey(e => e.KhachHangEmail);

            modelBuilder.Entity<KhachHang>()
                .HasMany(e => e.DiaChis)
                .WithRequired(e => e.KhachHang)
                .HasForeignKey(e => e.KhachHangEmail);

            modelBuilder.Entity<KhachHang>()
                .HasMany(e => e.DonHangs)
                .WithRequired(e => e.KhachHang)
                .HasForeignKey(e => e.KhachHangEmail)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<KhachHang>()
                .HasMany(e => e.GioHangs)
                .WithOptional(e => e.KhachHang)
                .HasForeignKey(e => e.KhachHangEmail);

            modelBuilder.Entity<Mau>()
                .Property(e => e.MaHex)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Mau>()
                .HasMany(e => e.ChiTietDonHangs)
                .WithRequired(e => e.Mau)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Mau>()
                .HasMany(e => e.GioHangItems)
                .WithRequired(e => e.Mau)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SanPham>()
                .Property(e => e.GiaBan)
                .HasPrecision(12, 2);

            modelBuilder.Entity<SanPham>()
                .HasMany(e => e.ChiTietDonHangs)
                .WithRequired(e => e.SanPham)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SanPham>()
                .HasMany(e => e.GioHangItems)
                .WithRequired(e => e.SanPham)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Size>()
                .HasMany(e => e.ChiTietDonHangs)
                .WithRequired(e => e.Size)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Size>()
                .HasMany(e => e.GioHangItems)
                .WithRequired(e => e.Size)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaiKhoan>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<TaiKhoan>()
                .Property(e => e.MatKhauHash)
                .IsUnicode(false);

            modelBuilder.Entity<TaiKhoan>()
                .HasOptional(e => e.DatLaiMatKhau)
                .WithRequired(e => e.TaiKhoan)
                .WillCascadeOnDelete();

            modelBuilder.Entity<TaiKhoan>()
                .HasOptional(e => e.KhachHang)
                .WithRequired(e => e.TaiKhoan);
        }
    }
}
