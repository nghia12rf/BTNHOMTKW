using BTNHOMTKW.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System;
using System.Data.Entity; 

namespace BTNHOMTKW.Controllers
{
    public class AccountController : Controller
    {
        private DoAn_ShopEntities db = new DoAn_ShopEntities();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                // 1. Mã hóa mật khẩu
                string passHash = Security.MD5Hash(password);

                // 2. Tìm tài khoản
                var user = db.TaiKhoans.FirstOrDefault(u => u.Email == email
                                                         && u.MatKhauHash == passHash
                                                         && u.TrangThai == 1);

                if (user != null)
                {
                    // 3. Lưu cookie đăng nhập
                    FormsAuthentication.SetAuthCookie(user.Email, false);

                    // 4. KIỂM TRA QUYỀN VÀ CHUYỂN HƯỚNG (Đã sửa)
                    if (user.Role == "Admin")
                    {
                        
                        return RedirectToAction("Index", "AdminDashboard");

                        
                    }
                    else
                    {
                        // User thường về trang chủ
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản bị khóa!";
                }
            }
            return View();
        }

        

        // 2. Đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string email, string password, string hoTen, string sdt, DateTime? ngaySinh)
        {
            if (db.TaiKhoans.Any(x => x.Email == email))
            {
                ViewBag.Error = "Email này đã được sử dụng!";
                return View();
            }

            var tk = new TaiKhoan
            {
                Email = email,
                MatKhauHash = Security.MD5Hash(password),
                Role = "User",
                TrangThai = 1,
                NgayTao = DateTime.Now
            };
            db.TaiKhoans.Add(tk);

            var kh = new KhachHang
            {
                TaiKhoanEmail = email,
                HoTen = hoTen,
                SDT = sdt,
                NgaySinh = ngaySinh
            };
            db.KhachHangs.Add(kh);

            db.SaveChanges();
            return RedirectToAction("Login");
        }

        // 3. Đăng xuất
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // 4. Trang cá nhân
        public ActionResult MyProfile()
        {
            string email = User.Identity.Name;
            var kh = db.KhachHangs
                       .Include("DiaChis") // Load danh sách địa chỉ
                       .Include("DonHangs") // Load đơn hàng (nếu cần hiển thị lịch sử)
                       .FirstOrDefault(k => k.TaiKhoanEmail == email);

            return View(kh);
        }

        // 2. CẬP NHẬT THÔNG TIN CÁ NHÂN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string hoTen, string sdt, DateTime? ngaySinh)
        {
            string email = User.Identity.Name;
            var kh = db.KhachHangs.Find(email);
            if (kh != null)
            {
                kh.HoTen = hoTen;
                kh.SDT = sdt;
                kh.NgaySinh = ngaySinh;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction("MyProfile");
        }

        // 3. THÊM ĐỊA CHỈ MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAddress(string diaChi, string nguoiNhan, string sdtNhan, bool macDinh = false)
        {
            string email = User.Identity.Name;

            // Kiểm tra trùng lặp (Vì DiaChiChiTiet là khóa chính)
            var existing = db.DiaChis.FirstOrDefault(d => d.KhachHangEmail == email && d.DiaChiChiTiet == diaChi);
            if (existing != null)
            {
                TempData["Error"] = "Địa chỉ này đã tồn tại trong sổ địa chỉ!";
                return RedirectToAction("MyProfile");
            }

            // Xử lý logic mặc định: Nếu user chọn mặc định, phải bỏ mặc định các cái cũ đi
            if (macDinh)
            {
                var listOld = db.DiaChis.Where(d => d.KhachHangEmail == email).ToList();
                foreach (var item in listOld) item.MacDinh = false;
            }
            // Nếu đây là địa chỉ đầu tiên, bắt buộc là mặc định
            else if (!db.DiaChis.Any(d => d.KhachHangEmail == email))
            {
                macDinh = true;
            }

            var newAddr = new DiaChi
            {
                KhachHangEmail = email,
                DiaChiChiTiet = diaChi,
                HoTenNhan = nguoiNhan,
                SDTNhan = sdtNhan,
                MacDinh = macDinh
            };

            db.DiaChis.Add(newAddr);
            db.SaveChanges();
            TempData["Success"] = "Thêm địa chỉ mới thành công!";

            return RedirectToAction("MyProfile");
        }

        // 4. ĐẶT LÀM ĐỊA CHỈ MẶC ĐỊNH
        public ActionResult SetDefaultAddress(string diaChiChiTiet)
        {
            string email = User.Identity.Name;

            // 1. Bỏ mặc định tất cả
            var allAddr = db.DiaChis.Where(d => d.KhachHangEmail == email).ToList();
            foreach (var item in allAddr) item.MacDinh = false;

            // 2. Set mặc định cái được chọn
            // Lưu ý: Vì DiaChiChiTiet là khóa chính nên dùng Find(email, diaChiChiTiet)
            var target = db.DiaChis.Find(email, diaChiChiTiet);
            if (target != null)
            {
                target.MacDinh = true;
                db.SaveChanges();
                TempData["Success"] = "Đã thay đổi địa chỉ mặc định.";
            }

            return RedirectToAction("MyProfile");
        }

        // 5. XÓA ĐỊA CHỈ
        public ActionResult DeleteAddress(string diaChiChiTiet)
        {
            string email = User.Identity.Name;
            var target = db.DiaChis.Find(email, diaChiChiTiet);

            if (target != null)
            {
                if (target.MacDinh)
                {
                    TempData["Error"] = "Không thể xóa địa chỉ mặc định. Hãy chọn cái khác làm mặc định trước!";
                }
                else
                {
                    db.DiaChis.Remove(target);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa địa chỉ.";
                }
            }
            return RedirectToAction("MyProfile");
        }

        // 5. Chi tiết đơn hàng
        [Authorize]
        public ActionResult OrderDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            string userEmail = User.Identity.Name;

            var donHang = db.DonHangs
                            .Include("ChiTietDonHangs")
                            .Include("ChiTietDonHangs.SanPham")
                            .FirstOrDefault(d => d.MaDonHang == id && d.KhachHangEmail == userEmail);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
        }
    }
}