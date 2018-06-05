using System.Web;
using System.Web.Optimization;

namespace Fuar_Organizasyon
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Geliştirme yapmak ve öğrenmek için Modernizr'ın geliştirme sürümünü kullanın. Daha sonra,
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Content/multi/js/bootstrap-multiselect.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/Panel/css").Include(
                    "~/Content/Panel/bootstrap.css",
                    "~/Content/Panel/metisMenu.css",
                    "~/Content/Panel/sb-admin-2.css",
                    "~/Content/Panel/font-awesome/css/font-awesome.min.css",
                    "~/Content/multi/css/bootstrap-multiselect.css"
                ));
            bundles.Add(new StyleBundle("~/Content/Panel/jquery").Include(
                    "~/Content/Panel/jquery/metisMenu.js",
                    "~/Content/Panel/jquery/metisMenu.min.js",
                    "~/Content/Panel/jquery/sb-admin-2.js"
                ));


        }
    }
}
