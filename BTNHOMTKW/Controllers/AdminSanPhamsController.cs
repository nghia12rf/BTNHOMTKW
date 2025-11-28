using BTNHOMTKW.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

public class AdminSanPhamsController : BaseAdminController
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

   
    // GET: AdminSanPhams
    public ActionResult Index(string searchString, string searchCategory, string searchStatus)
    {
        var products = db.SanPhams.Include(s => s.DanhMuc).AsQueryable();

        // 1. Lọc theo Tên/SKU
        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(s => s.TenSP.Contains(searchString) || s.SKU.Contains(searchString));
        }

        // 2. Lọc theo Danh mục (SỬA LẠI ĐOẠN NÀY)
        if (!string.IsNullOrEmpty(searchCategory))
        {
            // === LOGIC MỚI: LẤY CẢ CHA LẪN CON ===

            // Bước A: Lấy danh sách các danh mục con trực tiếp
            var subCategories = db.DanhMucs
                                  .Where(d => d.ParentDM == searchCategory)
                                  .Select(d => d.TenDM)
                                  .ToList();

            // Bước B: Thêm chính danh mục cha (đang chọn) vào danh sách
            subCategories.Add(searchCategory);

            // Bước C: Lọc sản phẩm có tên danh mục nằm trong danh sách trên
            // Dùng .Contains() tương đương với câu lệnh SQL: WHERE DanhMucTen IN ('Áo Nam', 'Áo Sơ Mi', ...)
            products = products.Where(s => subCategories.Contains(s.DanhMucTen));
        }

        // 3. Lọc theo Trạng thái
        if (!string.IsNullOrEmpty(searchStatus))
        {
            if (searchStatus == "active")
                products = products.Where(s => s.Active == true);
            else if (searchStatus == "inactive")
                products = products.Where(s => s.Active == false);
        }
        else
        {
            // Mặc định chỉ hiện active
            products = products.Where(s => s.Active == true);
        }

        products = products.OrderByDescending(s => s.NgayTao);

        // --- ViewBags ---
        ViewBag.CategoryList = new SelectList(db.DanhMucs, "TenDM", "TenDM", searchCategory);

        var statusList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Đang bán", Value = "active" },
        new SelectListItem { Text = "Đã ẩn / Xóa", Value = "inactive" },
        new SelectListItem { Text = "Tất cả", Value = "all" }
    };
        ViewBag.StatusList = new SelectList(statusList, "Value", "Text", searchStatus ?? "active");
        ViewBag.CurrentFilter = searchString;

        return View(products.ToList());
    }

    // GET: AdminSanPhams/Details/5
    public ActionResult Details(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        SanPham sanPham = db.SanPhams.Find(id);
        if (sanPham == null)
        {
            return HttpNotFound();
        }
        return View(sanPham);
    }

    // GET: AdminSanPhams/Create
    public ActionResult Create()
    {
        ViewBag.DanhMucTen = new SelectList(db.DanhMucs, "TenDM", "TenDM");
        return View();
    }

    // POST: AdminSanPhams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "MaSP,TenSP,MoTa,DanhMucTen,GiaBan,SoLuongTon,Active")] SanPham sanPham, HttpPostedFileBase HinhAnhChinhFile)
    {
        // Bỏ qua kiểm tra SKU và NgayTao vì sẽ tự sinh
        ModelState.Remove("SKU");
        ModelState.Remove("NgayTao");

        try
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh chính
                if (HinhAnhChinhFile != null && HinhAnhChinhFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileNameWithoutExtension(HinhAnhChinhFile.FileName);
                    var extension = Path.GetExtension(HinhAnhChinhFile.FileName);

                    // Tạo tên file duy nhất
                    var uniqueFileName = $"{fileName}-{Guid.NewGuid()}{extension}";
                    var savePath = Path.Combine(Server.MapPath("~/Content/images/products/"), uniqueFileName);

                    HinhAnhChinhFile.SaveAs(savePath);
                    sanPham.HinhAnhChinh = "/Content/images/products/" + uniqueFileName;
                }
                else
                {
                    sanPham.HinhAnhChinh = "/Content/images/default.jpg"; // Ảnh mặc định
                }

                // 1. Đếm số lượng sản phẩm đang có trong kho
                int count = db.SanPhams.Count();

                // 2. Cộng thêm 1
                int nextId = count + 1;

                // 3. Tạo chuỗi (D3 nghĩa là đảm bảo luôn có 3 số: 1 -> 001)
                sanPham.SKU = "SP" + nextId.ToString("D3");

                // Kết quả sẽ là: SP001, SP002, SP003...

                // ... (Code gán ngày tạo giữ nguyên) ...
                sanPham.NgayTao = DateTime.Now;

                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
        {
            if (ex.InnerException?.InnerException?.Message.Contains("PRIMARY KEY constraint") == true)
            {
                ModelState.AddModelError("MaSP", "Mã sản phẩm này đã tồn tại. Vui lòng nhập mã khác.");
            }
            else
            {
                ModelState.AddModelError("", "Đã có lỗi CSDL xảy ra: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Đã có lỗi không xác định: " + ex.Message);
        }

        // Trả lại DropDownList khi model không hợp lệ
        ViewBag.DanhMucTen = new SelectList(db.DanhMucs, "TenDM", "TenDM", sanPham.DanhMucTen);
        return View(sanPham);
    }


    // GET: AdminSanPhams/Edit/5
    public ActionResult Edit(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        SanPham sanPham = db.SanPhams.Find(id);
        if (sanPham == null)
        {
            return HttpNotFound();
        }
        ViewBag.DanhMucTen = new SelectList(db.DanhMucs, "TenDM", "TenDM", sanPham.DanhMucTen);
        return View(sanPham);
    }

    // POST: AdminSanPhams/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "MaSP,TenSP,MoTa,DanhMucTen,GiaBan,SoLuongTon,Active,SKU,NgayTao,HinhAnhChinh")] SanPham sanPham, HttpPostedFileBase HinhAnhChinhFile)
    {
        

        if (ModelState.IsValid)
        {
            // Xử lý upload ảnh mới
            if (HinhAnhChinhFile != null && HinhAnhChinhFile.ContentLength > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(HinhAnhChinhFile.FileName);
                var extension = Path.GetExtension(HinhAnhChinhFile.FileName);
                fileName = $"{fileName}-{Guid.NewGuid()}{extension}";

                var path = Path.Combine(Server.MapPath("~/Content/images/products/"), fileName);
                HinhAnhChinhFile.SaveAs(path);

                sanPham.HinhAnhChinh = "/Content/images/products/" + fileName; // Gán ảnh mới
            }
            

            db.Entry(sanPham).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        ViewBag.DanhMucTen = new SelectList(db.DanhMucs, "TenDM", "TenDM", sanPham.DanhMucTen);
        return View(sanPham);
    }

    // GET: AdminSanPhams/Delete/5
    public ActionResult Delete(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        SanPham sanPham = db.SanPhams.Find(id);
        if (sanPham == null)
        {
            return HttpNotFound();
        }
        return View(sanPham);
    }

    // POST: AdminSanPhams/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(string id)
    {
        
        SanPham sanPham = db.SanPhams.Find(id);
        if (sanPham == null)
        {
            return HttpNotFound();
        }

        // Thay vì xóa, chúng "ẩn" sản phẩm đi
        sanPham.Active = false;
        db.Entry(sanPham).State = EntityState.Modified;

        
        db.SaveChanges();
        return RedirectToAction("Index");
    }
    // GET: AdminSanPhams/ThungRac
    public ActionResult ThungRac()
    {
        // Lấy các sản phẩm có Active = false (đã bị xóa mềm)
        var sanPhamsDaXoa = db.SanPhams
                              .Include(s => s.DanhMuc)
                              .Where(s => s.Active == false) // Điều kiện quan trọng
                              .OrderByDescending(s => s.NgayTao);

        return View(sanPhamsDaXoa.ToList());
    }

    // GET: AdminSanPhams/KhoiPhuc/5
    public ActionResult KhoiPhuc(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        SanPham sanPham = db.SanPhams.Find(id);
        if (sanPham == null)
        {
            return HttpNotFound();
        }

        // Khôi phục lại sản phẩm
        sanPham.Active = true;

        // Lưu thay đổi
        db.Entry(sanPham).State = EntityState.Modified;
        db.SaveChanges();

        // Quay lại trang Thùng rác để xem còn cái nào cần khôi phục không
        return RedirectToAction("ThungRac");
    }

    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }
}