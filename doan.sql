/* ===== TẠO CSDL ===== */
USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DoAn_Shop')
BEGIN
    ALTER DATABASE DoAn_Shop SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DoAn_Shop;
END
GO

CREATE DATABASE DoAn_Shop;
GO
USE DoAn_Shop;
GO

/* ===== 1) Tài khoản & Khách hàng ===== */
CREATE TABLE dbo.TaiKhoan (
    Email           VARCHAR(150) PRIMARY KEY,
    MatKhauHash     VARCHAR(150) NOT NULL,
    [Role]          NVARCHAR(20) NOT NULL CONSTRAINT DF_TaiKhoan_Role DEFAULT (N'User'),
    TrangThai       TINYINT NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT (1),
    NgayTao         DATETIME2 NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE dbo.KhachHang (
    TaiKhoanEmail   VARCHAR(150) PRIMARY KEY
        REFERENCES dbo.TaiKhoan(Email),
    HoTen           NVARCHAR(150) NOT NULL,
    SDT             VARCHAR(15) NULL UNIQUE,
    NgaySinh        DATE NULL
);

CREATE TABLE dbo.DiaChi (
    KhachHangEmail   VARCHAR(150) NOT NULL
        REFERENCES dbo.KhachHang(TaiKhoanEmail) ON DELETE CASCADE,
    DiaChiChiTiet    NVARCHAR(250) NOT NULL,
    HoTenNhan        NVARCHAR(150) NOT NULL,
    SDTNhan          VARCHAR(15) NOT NULL,
    MacDinh          BIT NOT NULL CONSTRAINT DF_DiaChi_MacDinh DEFAULT (0),
    CONSTRAINT PK_DiaChi PRIMARY KEY (KhachHangEmail, DiaChiChiTiet)
);
CREATE UNIQUE INDEX UX_DiaChi_MacDinh
ON dbo.DiaChi(KhachHangEmail)
WHERE MacDinh = 1;
GO

/* ===== 2) Danh mục / Thuộc tính ===== */
CREATE TABLE dbo.DanhMuc (
    TenDM     NVARCHAR(150) PRIMARY KEY,
    Slug      NVARCHAR(200) NULL,
    ParentDM  NVARCHAR(150) NULL
        REFERENCES dbo.DanhMuc(TenDM)
);

CREATE TABLE dbo.[Size] (
    TenSize  NVARCHAR(10) PRIMARY KEY
);

CREATE TABLE dbo.Mau (
    TenMau   NVARCHAR(30) PRIMARY KEY,
    MaHex    CHAR(7) NULL UNIQUE
);

GO
/* ===== 3) Sản phẩm & Ảnh ===== */
CREATE TABLE dbo.SanPham (
    MaSP             NVARCHAR(30) PRIMARY KEY,
    TenSP            NVARCHAR(200) NOT NULL,
    MoTa             NVARCHAR(MAX) NULL,
    DanhMucTen       NVARCHAR(150) NULL
        REFERENCES dbo.DanhMuc(TenDM),
    HinhAnhChinh     NVARCHAR(200) NULL,
    -- Đã sửa DEFAULT để tạo SKU dạng 'SP-' + 8 ký tự ngẫu nhiên ngay từ đầu
    SKU              NVARCHAR(40) NOT NULL UNIQUE
        CONSTRAINT DF_SanPham_SKU DEFAULT ('SP-' + LEFT(REPLACE(CONVERT(NVARCHAR(40), NEWID()), '-', ''), 8)),
    GiaBan           DECIMAL(12,2) NOT NULL 
        CONSTRAINT DF_SanPham_GiaBan DEFAULT (0)
        CONSTRAINT CK_SanPham_GiaBan CHECK (GiaBan >= 0),
    SoLuongTon       INT NOT NULL 
        CONSTRAINT DF_SanPham_SLT DEFAULT (0)
        CONSTRAINT CK_SanPham_SoLuongTon CHECK (SoLuongTon >= 0),
    Active           BIT NOT NULL CONSTRAINT DF_SanPham_Active DEFAULT (1),
    NgayTao          DATETIME2 NOT NULL CONSTRAINT DF_SanPham_NgayTao DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE dbo.AnhSanPham (
    MaSP     NVARCHAR(30) NOT NULL
        REFERENCES dbo.SanPham(MaSP) ON DELETE CASCADE,
    FileAnh  NVARCHAR(200) NOT NULL,
    ThuTu    SMALLINT NOT NULL CONSTRAINT DF_AnhSanPham_ThuTu DEFAULT (0),
    CONSTRAINT PK_AnhSanPham PRIMARY KEY (MaSP, FileAnh)
);

GO
/* ===== 4) Đánh giá ===== */
CREATE TABLE dbo.DanhGia (
    MaSP            NVARCHAR(30) NOT NULL
        REFERENCES dbo.SanPham(MaSP) ON DELETE CASCADE,
    KhachHangEmail VARCHAR(150) NOT NULL
        REFERENCES dbo.KhachHang(TaiKhoanEmail) ON DELETE CASCADE,
    SoSao           TINYINT NOT NULL
        CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5),
    BinhLuan        NVARCHAR(500) NULL,
    NgayDG          DATETIME2 NOT NULL CONSTRAINT DF_DanhGia_Ngay DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_DanhGia PRIMARY KEY (MaSP, KhachHangEmail)
);

GO
/* ===== 5) Giỏ hàng ===== */
CREATE TABLE dbo.GioHang (
    SessionKey      NVARCHAR(60) PRIMARY KEY,
    KhachHangEmail  VARCHAR(150) NULL
        REFERENCES dbo.KhachHang(TaiKhoanEmail) ON DELETE SET NULL,
    NgayTao         DATETIME2 NOT NULL CONSTRAINT DF_GioHang_NgayTao DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE dbo.GioHangItem (
    SessionKey   NVARCHAR(60) NOT NULL
        REFERENCES dbo.GioHang(SessionKey) ON DELETE CASCADE,
    MaSP         NVARCHAR(30) NOT NULL
        REFERENCES dbo.SanPham(MaSP),
    TenSize      NVARCHAR(10) NOT NULL
        REFERENCES dbo.[Size](TenSize),
    TenMau       NVARCHAR(30) NOT NULL
        REFERENCES dbo.Mau(TenMau),
    SoLuong      SMALLINT NOT NULL CONSTRAINT CK_GioHangItem_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT PK_GioHangItem PRIMARY KEY (SessionKey, MaSP, TenSize, TenMau)
);

GO
/* ===== 6) Đơn hàng & Chi tiết ===== */
CREATE TABLE dbo.DonHang (
    MaDonHang              NVARCHAR(20) PRIMARY KEY,
    KhachHangEmail         VARCHAR(150) NOT NULL
        REFERENCES dbo.KhachHang(TaiKhoanEmail),
    HoTenNguoiNhan         NVARCHAR(150) NOT NULL,
    SDTNguoiNhan           VARCHAR(15) NOT NULL,
    DiaChiGiaoHang         NVARCHAR(250) NOT NULL,
    PTThanhToan            NVARCHAR(50) NOT NULL,
    PhiVanChuyen           DECIMAL(12,2) NOT NULL CONSTRAINT DF_DonHang_PVC DEFAULT (0)
        CONSTRAINT CK_DonHang_PVC CHECK (PhiVanChuyen >= 0),
    TongTien               DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_DonHang_Tong CHECK (TongTien >= 0),
    ThanhTien              DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_DonHang_Thanh CHECK (ThanhTien >= 0),
    TrangThai              NVARCHAR(30) NOT NULL CONSTRAINT DF_DonHang_TrangThai DEFAULT (N'Đang xử lý')
        CONSTRAINT CK_DonHang_TrangThai CHECK (TrangThai IN (N'Đang xử lý', N'Đã xác nhận', N'Đang giao', N'Hoàn thành', N'Đã hủy')),
    TrangThaiThanhToan NVARCHAR(20) NOT NULL CONSTRAINT DF_DonHang_TTTT DEFAULT (N'Chưa thanh toán'),
    NgayDat                DATETIME2 NOT NULL CONSTRAINT DF_DonHang_NgayDat DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE dbo.ChiTietDonHang (
    MaDonHang  NVARCHAR(20) NOT NULL
        REFERENCES dbo.DonHang(MaDonHang) ON DELETE CASCADE,
    MaSP       NVARCHAR(30) NOT NULL
        REFERENCES dbo.SanPham(MaSP),
    TenSize    NVARCHAR(10) NOT NULL
        REFERENCES dbo.[Size](TenSize),
    TenMau     NVARCHAR(30) NOT NULL
        REFERENCES dbo.Mau(TenMau),
    SoLuong    SMALLINT NOT NULL CONSTRAINT CK_CT_SoLuong CHECK (SoLuong > 0),
    DonGia     DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_CT_DonGia CHECK (DonGia >= 0),
    CONSTRAINT PK_ChiTietDonHang PRIMARY KEY (MaDonHang, MaSP, TenSize, TenMau)
);

GO
/* ===== 7) Tiện ích ===== */
CREATE TABLE dbo.DatLaiMatKhau (
    Email               VARCHAR(150) PRIMARY KEY
        REFERENCES dbo.TaiKhoan(Email) ON DELETE CASCADE,
    Token               VARCHAR(150) NOT NULL UNIQUE,
    ThoiGianHetHan      DATETIME2 NOT NULL
);

CREATE TABLE dbo.LichSuDonHang (
    MaDonHang  NVARCHAR(20) NOT NULL
        REFERENCES dbo.DonHang(MaDonHang) ON DELETE CASCADE,
    TrangThai  NVARCHAR(30) NOT NULL,
    ThoiGian   DATETIME2 NOT NULL CONSTRAINT DF_LSDH_ThoiGian DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_LichSuDonHang PRIMARY KEY (MaDonHang, ThoiGian)
);
GO
USE DoAn_Shop;
GO

INSERT INTO dbo.[Size] (TenSize) VALUES 
-- Size chữ (Áo)
(N'S'), (N'M'), (N'L'), (N'XL'), (N'XXL'),
-- Size số (Quần - Bổ sung thêm để khớp với đơn hàng mẫu)
(N'28'), (N'29'), (N'30'), (N'31'), (N'32'), (N'33'), (N'34'),
-- Size giày (Bổ sung thêm 35, 36, 37)
(N'35'), (N'36'), (N'37'), (N'38'), (N'39'), (N'40'), (N'41'), (N'42'), (N'43'), (N'44');

-- 2. Màu sắc
INSERT INTO dbo.Mau (TenMau, MaHex) VALUES
(N'Trắng', '#FFFFFF'), 
(N'Đen', '#000000'), 
(N'Xám', '#808080'), 
(N'Xanh Navy', '#000080'), 
(N'Đỏ', '#FF0000'), 
(N'Kem', '#F5F5DC');

-- 3. Danh mục (2 Cấp)
INSERT INTO dbo.DanhMuc (TenDM, Slug, ParentDM) VALUES 
-- Cấp 1
(N'Thời Trang Nam', 'thoi-trang-nam', NULL),
(N'Thời Trang Nữ', 'thoi-trang-nu', NULL),
(N'Giày Dép', 'giay-dep', NULL),
-- Cấp 2
(N'Áo Nam', 'ao-nam', N'Thời Trang Nam'),
(N'Quần Nam', 'quan-nam', N'Thời Trang Nam'),
(N'Giày Thể Thao', 'giay-the-thao', N'Giày Dép'),
(N'Giày Cao Gót', 'giay-cao-got', N'Giày Dép'),
(N'Boots', 'boots', N'Giày Dép');
GO

/* ========================================================== */
/* PHẦN 2: TÀI KHOẢN & KHÁCH HÀNG                            */
/* ========================================================== */

-- 1. Tài khoản
INSERT INTO dbo.TaiKhoan (Email, MatKhauHash, [Role], TrangThai) VALUES
('admin@shop.com', 'e10adc3949ba59abbe56e057f20f883e', N'Admin', 1), -- Pass: 123456 (MD5)
('khach@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', N'User', 1),
('tu.nguyen@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', N'User', 1),
('lan.tran@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', N'User', 1);

-- 2. KhachHang
INSERT INTO dbo.KhachHang (TaiKhoanEmail, HoTen, SDT, NgaySinh) VALUES
('khach@gmail.com', N'Nguyễn Văn Khách', '0909123456', NULL),
('tu.nguyen@gmail.com', N'Nguyễn Thanh Tú', '0911223344', '2000-05-15'),
('lan.tran@gmail.com', N'Trần Thị Lan', '0988776655', '1998-11-20');

-- 3. Địa chỉ
INSERT INTO dbo.DiaChi (KhachHangEmail, DiaChiChiTiet, HoTenNhan, SDTNhan, MacDinh) VALUES
('khach@gmail.com', N'123 Nguyễn Trãi, Q5, TP.HCM', N'Nguyễn Văn Khách', '0909123456', 1),
('tu.nguyen@gmail.com', N'45 Lê Lợi, Q1, TP.HCM', N'Nguyễn Thanh Tú', '0911223344', 1),
('tu.nguyen@gmail.com', N'Tòa nhà Bitexco, Q1 (Công ty)', N'Nguyễn Thanh Tú', '0911223344', 0),
('lan.tran@gmail.com', N'10 Cầu Giấy, Hà Nội', N'Trần Thị Lan', '0988776655', 1);
GO

/* ========================================================== */
/* PHẦN 3: SẢN PHẨM (48 SP)                                  */
/* ========================================================== */
-- Lưu ý: Không nhập cột SKU, để mặc định tự sinh 'SP-' + random
INSERT INTO dbo.SanPham (MaSP, TenSP, GiaBan, MoTa, HinhAnhChinh, DanhMucTen, SoLuongTon, Active) VALUES
-- --- TRANG 1: ÁO NAM ---
('AO001', N'Áo NY Thêu Ngực', 150000, N'Form rộng rãi, thoải mái vận động.', '/Content/images/products/product-1-1.png', N'Áo Nam', 100, 1),
('AO002', N'Áo LA Navy Thêu Ngực', 250000, N'Thiết kế đơn giản, dễ phối đồ.', '/Content/images/products/product-1-2.png', N'Áo Nam', 100, 1),
('AO003', N'Áo NY BaseBall Đen', 320000, N'Cá tính, hiện đại.', '/Content/images/products/product-1-3.png', N'Áo Nam', 50, 1),
('AO004', N'Áo World Wide Đen', 220000, N'Cotton co giãn nhẹ.', '/Content/images/products/product-1-4.png', N'Áo Nam', 80, 1),
('AO005', N'Áo MLB B01 Đen', 450000, N'Họa tiết độc đáo.', '/Content/images/products/product-1-5.png', N'Áo Nam', 60, 1),
('AO006', N'Áo MLB NY Trắng Thêu', 380000, N'Trẻ trung, dễ phối jean.', '/Content/images/products/product-1-6.png', N'Áo Nam', 90, 1),
('AO007', N'Áo MLB Boston Mono Kem', 260000, N'Ôm dáng nhẹ nhàng.', '/Content/images/products/product-1-7.png', N'Áo Nam', 70, 1),
('AO008', N'Áo MLB LA Xanh Dương', 170000, N'Mềm mại, thoáng khí.', '/Content/images/products/product-1-8.png', N'Áo Nam', 120, 1),
('AO009', N'Áo MLB NY Xanh Logo', 350000, N'Điểm nhấn thương hiệu.', '/Content/images/products/product-1-9.png', N'Áo Nam', 45, 1),
('AO010', N'Áo MLB LA Trắng Xanh', 400000, N'Màu sắc tinh tế.', '/Content/images/products/product-1-10.png', N'Áo Nam', 55, 1),
('AO011', N'Áo NY MLB', 280000, N'Phù hợp giới trẻ.', '/Content/images/products/product-1-11.png', N'Áo Nam', 100, 1),
('AO012', N'Áo Adidas 3 Sọc Đen', 210000, N'Dáng regular fit.', '/Content/images/products/product-1-12.png', N'Áo Nam', 40, 1),
('AO013', N'Áo Adidas Sọc Trắng', 180000, N'Cổ điển, cá tính.', '/Content/images/products/product-1-13.png', N'Áo Nam', 110, 1),
('AO014', N'Áo Boston Thun Dệt', 300000, N'Phong cách streetwear.', '/Content/images/products/product-1-14.png', N'Áo Nam', 30, 1),
('AO015', N'Áo Convernat Xanh', 270000, N'Local brand tối giản.', '/Content/images/products/product-1-15.png', N'Áo Nam', 65, 1),
('AO016', N'Áo COLDSTONE REDEEMER', 460000, N'Dễ thương, vải mát.', '/Content/images/products/product-1-16.png', N'Áo Nam', 25, 1),

-- --- TRANG 2: QUẦN NAM ---
('QN001', N'Quần Cargo', 400000, N'Ống đứng, form chuẩn.', '/Content/images/products/product-2-1.png', N'Quần Nam', 50, 1),
('QN002', N'Quần Dài Túi Hộp Đen', 180000, N'Co giãn nhẹ, thoải mái.', '/Content/images/products/product-2-2.png', N'Quần Nam', 80, 1),
('QN003', N'Quần Jean Spao Cargo', 350000, N'Tôn dáng chân dài.', '/Content/images/products/product-2-3.png', N'Quần Nam', 60, 1),
('QN004', N'Quần Men Trousers', 270000, N'Vải dệt kim thoáng nhẹ.', '/Content/images/products/product-2-4.png', N'Quần Nam', 45, 1),
('QN005', N'Quần Slim Fit Chinos', 300000, N'Vải lạnh, không nhăn.', '/Content/images/products/product-2-5.png', N'Quần Nam', 70, 1),
('QN006', N'Quần ARMANI EXCHANGE', 420000, N'Phong cách thành thị.', '/Content/images/products/product-2-6.png', N'Quần Nam', 30, 1),
('QN007', N'Quần Danito Black', 220000, N'Lưng cao, tôn dáng.', '/Content/images/products/product-2-7.png', N'Quần Nam', 90, 1),
('QN008', N'Quần MasCuline Beggi', 280000, N'Basic, thanh lịch.', '/Content/images/products/product-2-8.png', N'Quần Nam', 100, 1),
('QN009', N'Quần Kaki Chino', 360000, N'Ống suông, trẻ trung.', '/Content/images/products/product-2-9.png', N'Quần Nam', 40, 1),
('QN010', N'Quần Tommy Hilfiger', 410000, N'Co giãn 4 chiều.', '/Content/images/products/product-2-10.png', N'Quần Nam', 20, 1),
('QN011', N'Quần Stretch-Fabric', 190000, N'Màu be thanh lịch.', '/Content/images/products/product-2-11.png', N'Quần Nam', 65, 1),
('QN012', N'Quần Slim Fit Serge', 260000, N'Ôm nhẹ, công sở.', '/Content/images/products/product-2-12.png', N'Quần Nam', 55, 1),
('QN013', N'Quần Ultra Stretch', 370000, N'Wash nhẹ tự nhiên.', '/Content/images/products/product-2-13.png', N'Quần Nam', 35, 1),
('QN014', N'Quần Dài Piccola', 430000, N'Ống đứng truyền thống.', '/Content/images/products/product-2-14.png', N'Quần Nam', 25, 1),
('QN015', N'Quần Slim Fit Twill', 200000, N'Không gò bó.', '/Content/images/products/product-2-15.png', N'Quần Nam', 85, 1),
('QN016', N'Quần Levi Cargo', 290000, N'Jogger dễ vận động.', '/Content/images/products/product-2-16.png', N'Quần Nam', 60, 1),

-- --- TRANG 3: GIÀY DÉP ---
('GD001', N'Giày Superstar', 750000, N'Họa tiết ren trang trí.', '/Content/images/products/product-3-1.png', N'Giày Thể Thao', 40, 1),
('GD002', N'Giày Court Refit', 1200000, N'Vật liệu tái chế.', '/Content/images/products/product-3-2.png', N'Giày Thể Thao', 30, 1),
('GD003', N'Giày Samba OG', 850000, N'Da mềm, mũi chữ T.', '/Content/images/products/product-3-3.png', N'Giày Thể Thao', 50, 1),
('GD004', N'Giày Barreda', 1100000, N'Da lộn cổ điển.', '/Content/images/products/product-3-4.png', N'Giày Thể Thao', 35, 1),
('GD005', N'Giày Nike Air Force 1', 350000, N'Thiết kế thập niên 80.', '/Content/images/products/product-3-5.png', N'Giày Thể Thao', 80, 1),
('GD006', N'Giày NikeCourt Lite', 700000, N'Bền bỉ, thể thao.', '/Content/images/products/product-3-6.png', N'Giày Thể Thao', 45, 1),
('GD007', N'Giày Jordan Stadium', 1300000, N'Thoải mái tối đa.', '/Content/images/products/product-3-7.png', N'Giày Thể Thao', 20, 1),
('GD008', N'Giày Air Jordan 1 Low', 900000, N'Vẻ ngoài cổ điển.', '/Content/images/products/product-3-8.png', N'Giày Thể Thao', 25, 1),
('GD009', N'Giày Nike Full Force', 1050000, N'Hấp dẫn trường phái cũ.', '/Content/images/products/product-3-9.png', N'Giày Thể Thao', 30, 1),
('GD010', N'Giày SIGOURNEY SIG-33', 400000, N'Tăng chiều cao 6cm.', '/Content/images/products/product-3-10.png', N'Giày Thể Thao', 60, 1),
('GD011', N'Giày Loafer SIGOURNEY', 850000, N'Phong cách khác biệt.', '/Content/images/products/product-3-11.png', N'Giày Thể Thao', 40, 1),
('GD012', N'Giày Cao Gót EHE006', 1250000, N'Quai thanh lịch.', '/Content/images/products/product-3-12.png', N'Giày Cao Gót', 30, 1),
('GD013', N'Giày Cao Gót EHE044', 900000, N'Phù hợp công sở.', '/Content/images/products/product-3-13.png', N'Giày Cao Gót', 35, 1),
('GD014', N'Giày Cao Gót EHE017', 1100000, N'Mary Jane cổ điển.', '/Content/images/products/product-3-14.png', N'Giày Cao Gót', 25, 1),
('GD015', N'Giày Cao Gót EFL010', 370000, N'Loafers chunky.', '/Content/images/products/product-3-15.png', N'Giày Cao Gót', 50, 1),
('GD016', N'Giày Boots E44005', 1100000, N'Knee-high boots.', '/Content/images/products/product-3-16.png', N'Boots', 15, 1);
GO

/* ========================================================== */
/* PHẦN 4: ẢNH SẢN PHẨM (Logic tự động sinh)                 */
/* ========================================================== */
-- Logic: Lấy tên file ảnh chính (bỏ extension) làm tên folder chứa ảnh chi tiết
WITH FolderData AS (
    SELECT 
        MaSP,
        REPLACE(REPLACE(HinhAnhChinh, '/Content/images/products/', ''), '.png', '') AS FolderName
    FROM dbo.SanPham
)
INSERT INTO dbo.AnhSanPham (MaSP, FileAnh, ThuTu)
-- Ảnh 1
SELECT MaSP, '/Content/images/product-details/' + FolderName + '/' + FolderName + '.png', 1 FROM FolderData
UNION ALL
-- Ảnh 2
SELECT MaSP, '/Content/images/product-details/' + FolderName + '/' + FolderName + '_2.png', 2 FROM FolderData
UNION ALL
-- Ảnh 3
SELECT MaSP, '/Content/images/product-details/' + FolderName + '/' + FolderName + '_3.png', 3 FROM FolderData
UNION ALL
-- Ảnh 4
SELECT MaSP, '/Content/images/product-details/' + FolderName + '/' + FolderName + '_4.png', 4 FROM FolderData;
GO

/* ========================================================== */
/* PHẦN 5: ĐƠN HÀNG & GIAO DỊCH                              */
/* ========================================================== */

-- 1. Đơn hàng
INSERT INTO dbo.DonHang (MaDonHang, KhachHangEmail, HoTenNguoiNhan, SDTNguoiNhan, DiaChiGiaoHang, PTThanhToan, TongTien, ThanhTien, TrangThai, TrangThaiThanhToan, NgayDat)
VALUES 
-- Đơn 1: Hoàn thành
('DH001', 'khach@gmail.com', N'Nguyễn Văn Khách', '0909123456', N'123 Nguyễn Trãi, Q5, TP.HCM', N'COD', 550000, 550000, N'Hoàn thành', N'Đã thanh toán', DATEADD(day, -10, GETDATE())),
-- Đơn 2: Đang xử lý
('DH002', 'tu.nguyen@gmail.com', N'Nguyễn Thanh Tú', '0911223344', N'45 Lê Lợi, Q1, TP.HCM', N'Chuyển khoản', 750000, 750000, N'Đang xử lý', N'Chưa thanh toán', GETDATE()),
-- Đơn 3: Đã hủy
('DH003', 'lan.tran@gmail.com', N'Trần Thị Lan', '0988776655', N'10 Cầu Giấy, Hà Nội', N'COD', 1250000, 1250000, N'Đã hủy', N'Chưa thanh toán', DATEADD(day, -5, GETDATE()));

-- 2. Chi tiết đơn hàng
INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSize, TenMau, SoLuong, DonGia) VALUES
-- DH001: Áo L + Quần 30
('DH001', 'AO001', N'L', N'Trắng', 1, 150000),
('DH001', 'QN001', N'30', N'Đen', 1, 400000),
-- DH002: Giày 41
('DH002', 'GD001', N'41', N'Trắng', 1, 750000),
-- DH003: Giày 37
('DH003', 'GD012', N'37', N'Đen', 1, 1250000);

-- 3. Lịch sử đơn hàng (Tạo tự động theo trạng thái đơn)
INSERT INTO dbo.LichSuDonHang (MaDonHang, TrangThai, ThoiGian) VALUES
('DH001', N'Đang xử lý', DATEADD(day, -10, GETDATE())),
('DH001', N'Hoàn thành', DATEADD(day, -8, GETDATE())),
('DH002', N'Đang xử lý', GETDATE()),
('DH003', N'Đang xử lý', DATEADD(day, -5, GETDATE())),
('DH003', N'Đã hủy', DATEADD(day, -4, GETDATE()));

-- 4. Đánh giá
INSERT INTO dbo.DanhGia (MaSP, KhachHangEmail, SoSao, BinhLuan, NgayDG) VALUES
('AO001', 'khach@gmail.com', 5, N'Áo đẹp, vải mát, giao hàng nhanh!', DATEADD(day, -9, GETDATE())),
('QN001', 'khach@gmail.com', 4, N'Quần form chuẩn nhưng hơi dài so với mình.', DATEADD(day, -9, GETDATE())),
('GD001', 'tu.nguyen@gmail.com', 5, N'Giày chính hãng, đi rất êm chân.', GETDATE());
GO

PRINT N'*** Đã nhập dữ liệu (Full Data) thành công! ***';
USE DoAn_Shop;
GO

-- 1. Xóa dữ liệu đơn hàng cũ để nhập lại cho chuẩn
DELETE FROM ChiTietDonHang;
DELETE FROM LichSuDonHang;
DELETE FROM DonHang;

-- 2. Nhập lại đơn hàng (Rải đều các tháng)
-- Giả sử năm hiện tại là 2025

-- --- Tháng 1: Doanh thu cao (Tết) ---
INSERT INTO dbo.DonHang (MaDonHang, KhachHangEmail, HoTenNguoiNhan, SDTNguoiNhan, DiaChiGiaoHang, PTThanhToan, TongTien, ThanhTien, TrangThai, TrangThaiThanhToan, NgayDat)
VALUES ('DH_T1_01', 'khach@gmail.com', N'Nguyễn Văn Khách', '0909123456', N'HCM', N'COD', 1500000, 1500000, N'Hoàn thành', N'Đã thanh toán', '2025-01-15');

-- SỬA LỖI: Thêm cột TenSize ('L') và TenMau (N'Trắng')
INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSize, TenMau, SoLuong, DonGia) 
VALUES ('DH_T1_01', 'AO001', N'L', N'Trắng', 10, 150000);


-- --- Tháng 3: Doanh thu trung bình ---
INSERT INTO dbo.DonHang (MaDonHang, KhachHangEmail, HoTenNguoiNhan, SDTNguoiNhan, DiaChiGiaoHang, PTThanhToan, TongTien, ThanhTien, TrangThai, TrangThaiThanhToan, NgayDat)
VALUES ('DH_T3_01', 'tu.nguyen@gmail.com', N'Nguyễn Thanh Tú', '0911223344', N'HN', N'Banking', 800000, 800000, N'Hoàn thành', N'Đã thanh toán', '2025-03-20');

INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSize, TenMau, SoLuong, DonGia) 
VALUES ('DH_T3_01', 'QN001', N'30', N'Đen', 2, 400000);


-- --- Tháng 6: Doanh thu thấp (Mùa hè) ---
INSERT INTO dbo.DonHang (MaDonHang, KhachHangEmail, HoTenNguoiNhan, SDTNguoiNhan, DiaChiGiaoHang, PTThanhToan, TongTien, ThanhTien, TrangThai, TrangThaiThanhToan, NgayDat)
VALUES ('DH_T6_01', 'lan.tran@gmail.com', N'Trần Thị Lan', '0988776655', N'Đà Nẵng', N'COD', 350000, 350000, N'Hoàn thành', N'Đã thanh toán', '2025-06-10');

INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSize, TenMau, SoLuong, DonGia) 
VALUES ('DH_T6_01', 'GD005', N'40', N'Trắng', 1, 350000);


-- --- Tháng 9: Doanh thu tăng lại (Back to school) ---
INSERT INTO dbo.DonHang (MaDonHang, KhachHangEmail, HoTenNguoiNhan, SDTNguoiNhan, DiaChiGiaoHang, PTThanhToan, TongTien, ThanhTien, TrangThai, TrangThaiThanhToan, NgayDat)
VALUES ('DH_T9_01', 'khach@gmail.com', N'Nguyễn Văn Khách', '0909123456', N'HCM', N'Banking', 2000000, 2000000, N'Hoàn thành', N'Đã thanh toán', '2025-09-05');

INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSize, TenMau, SoLuong, DonGia) 
VALUES 
('DH_T9_01', 'AO005', N'XL', N'Đen', 4, 450000); -- Đã bỏ bớt 1 món để tránh lỗi nếu thiếu SP QN005


-- --- Tháng 11 (Hiện tại): Vừa có đơn hoàn thành, vừa có đơn mới ---
INSERT INTO dbo.DonHang (MaDonHang, KhachHangEmail, HoTenNguoiNhan, SDTNguoiNhan, DiaChiGiaoHang, PTThanhToan, TongTien, ThanhTien, TrangThai, TrangThaiThanhToan, NgayDat)
VALUES 
('DH_T11_01', 'tu.nguyen@gmail.com', N'Tú Nguyễn', '0909000111', N'HCM', N'COD', 550000, 550000, N'Hoàn thành', N'Đã thanh toán', GETDATE()), 
('DH_T11_02', 'lan.tran@gmail.com', N'Lan Trần', '0909000222', N'HN', N'COD', 1200000, 1200000, N'Đang xử lý', N'Chưa thanh toán', GETDATE());

INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSize, TenMau, SoLuong, DonGia) 
VALUES 
('DH_T11_01', 'GD001', N'41', N'Trắng', 1, 550000),
('DH_T11_02', 'GD002', N'39', N'Đen', 1, 1200000);


PRINT N'*** Đã cập nhật dữ liệu biểu đồ thành công (Đã sửa lỗi Size)! ***';


USE DoAn_Shop;
GO

-- 1. Xóa dữ liệu cũ
DELETE FROM dbo.AnhSanPham;
GO

-- 2. Chèn dữ liệu với logic động (Dynamic Prefix)
WITH LogicData AS (
    SELECT 
        MaSP,
        -- A. Lấy tên gốc (VD: product-2-10)
        REPLACE(REPLACE(HinhAnhChinh, '/Content/images/products/', ''), '.png', '') AS BaseName,
        
        -- B. Tạo tên Folder (VD: product-details-2-10)
        REPLACE(
            REPLACE(
                REPLACE(HinhAnhChinh, '/Content/images/products/', ''), 
                '.png', ''
            ), 
            'product-', 'product-details-'
        ) AS FolderName
    FROM dbo.SanPham
),
FinalData AS (
    SELECT 
        MaSP,
        FolderName,
        -- C. Xác định tiền tố tên file dựa trên nhóm sản phẩm
        -- Nếu tên gốc bắt đầu bằng 'product-1-' -> File là product-1-
        -- Nếu tên gốc bắt đầu bằng 'product-2-' -> File là product-2- (QUẦN)
        -- Nếu tên gốc bắt đầu bằng 'product-3-' -> File là product-3- (GIÀY)
        CASE 
            WHEN BaseName LIKE 'product-1-%' THEN 'product-1-'
            WHEN BaseName LIKE 'product-2-%' THEN 'product-2-'
            WHEN BaseName LIKE 'product-3-%' THEN 'product-3-'
            ELSE 'product-1-' -- Mặc định
        END AS FilePrefix
    FROM LogicData
)

INSERT INTO dbo.AnhSanPham (MaSP, FileAnh, ThuTu)

-- Ảnh 1 (Đuôi 1.png)
SELECT MaSP, 
       '/Content/images/product-details/' + FolderName + '/' + FilePrefix + '1.png', 
       1 
FROM FinalData

UNION ALL

-- Ảnh 2 (Đuôi 2.png)
SELECT MaSP, 
       '/Content/images/product-details/' + FolderName + '/' + FilePrefix + '2.png', 
       2 
FROM FinalData

UNION ALL

-- Ảnh 3 (Đuôi 3.png)
SELECT MaSP, 
       '/Content/images/product-details/' + FolderName + '/' + FilePrefix + '3.png', 
       3 
FROM FinalData

UNION ALL

-- Ảnh 4 (Đuôi 4.png)
SELECT MaSP, 
       '/Content/images/product-details/' + FolderName + '/' + FilePrefix + '4.png', 
       4 
FROM FinalData;

GO

PRINT N'*** Đã cập nhật ảnh chi tiết cho cả Áo (1), Quần (2) và Giày (3) thành công! ***';