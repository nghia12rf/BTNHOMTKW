using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BTNHOMTKW.Models;

public class AdminDanhMucsController : BaseAdminController
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // GET: AdminDanhMucs
    public ActionResult Index()
    {
        // Sắp xếp để danh mục cha lên trước
        return View(db.DanhMucs.OrderBy(d => d.ParentDM).ThenBy(d => d.TenDM).ToList());
    }

    // GET: AdminDanhMucs/Create
    public ActionResult Create()
    {
        // Gửi danh sách danh mục (để chọn cha)
        ViewBag.ParentDM = new SelectList(db.DanhMucs, "TenDM", "TenDM");
        return View();
    }

    // POST: AdminDanhMucs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "TenDM,Slug,ParentDM")] DanhMuc danhMuc)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra trùng lặp TenDM (PK)
            if (db.DanhMucs.Any(d => d.TenDM == danhMuc.TenDM))
            {
                ModelState.AddModelError("TenDM", "Tên danh mục này đã tồn tại.");
            }
            else
            {
                db.DanhMucs.Add(danhMuc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        ViewBag.ParentDM = new SelectList(db.DanhMucs, "TenDM", "TenDM", danhMuc.ParentDM);
        return View(danhMuc);
    }

    // GET: AdminDanhMucs/Edit/Ao-Nam
    public ActionResult Edit(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        DanhMuc danhMuc = db.DanhMucs.Find(id);
        if (danhMuc == null)
        {
            return HttpNotFound();
        }
        ViewBag.ParentDM = new SelectList(db.DanhMucs, "TenDM", "TenDM", danhMuc.ParentDM);
        return View(danhMuc);
    }

    // POST: AdminDanhMucs/Edit/Ao-Nam
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "TenDM,Slug,ParentDM")] DanhMuc danhMuc)
    {
        if (ModelState.IsValid)
        {
            db.Entry(danhMuc).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        ViewBag.ParentDM = new SelectList(db.DanhMucs, "TenDM", "TenDM", danhMuc.ParentDM);
        return View(danhMuc);
    }

    // GET: AdminDanhMucs/Delete/Ao-Nam
    public ActionResult Delete(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        DanhMuc danhMuc = db.DanhMucs.Find(id);
        if (danhMuc == null)
        {
            return HttpNotFound();
        }
        return View(danhMuc);
    }

    // POST: AdminDanhMucs/Delete/Ao-Nam
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(string id)
    {
        // TODO: Cần kiểm tra xem có danh mục con,
        // hoặc sản phẩm nào đang dùng danh mục này không trước khi xóa

        DanhMuc danhMuc = db.DanhMucs.Find(id);
        db.DanhMucs.Remove(danhMuc);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}