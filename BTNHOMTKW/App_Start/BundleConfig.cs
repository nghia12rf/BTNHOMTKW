using System.Web;
using System.Web.Optimization;

namespace BTNHOMTKW
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // 1. JAVASCRIPT
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js"));
            

            // 2. CSS BUNDLES

            // --- Giao diện Khách hàng (User) ---
            bundles.Add(new StyleBundle("~/Content/usercss").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css"));


            // --- Giao diện Quản trị (Admin) ---
            bundles.Add(new StyleBundle("~/Content/admincss").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Admin.css"));
                      
        }
    }
}