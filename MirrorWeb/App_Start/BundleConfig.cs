using System.Web.Optimization;

namespace MirrorWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new Bundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui.min.js",
                "~/Scripts/select2.min.js",
                "~/Scripts/alertify.min.js"));

            bundles.Add(new Bundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new Bundle("~/bundles/kendo").Include(
                "~/Scripts/kendo/jszip.min.js",
                "~/Scripts/kendo/2019.1.220/kendo.all.min.js",
                "~/Scripts/kendo/2019.1.220/kendo.web.min.js",
                "~/Scripts/kendo/2019.1.220/cultures/kendo.culture.de-DE.min.js",
                "~/Scripts/kendo/2019.1.220/cultures/kendo.culture.en-EN.min.js,"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new Bundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/dashboard").Include(
                "~/Scripts/app/dashboard.js"));

            bundles.Add(new Bundle("~/bundles/main").Include(
                "~/Scripts/app/main.js"));

            bundles.Add(new Bundle("~/bundles/monitoring").Include(
                "~/Scripts/app/monitoring.js"));

            bundles.Add(new Bundle("~/bundles/syncAdminSettings").Include(
                "~/Scripts/syncAdminSettings.js"));

            bundles.Add(new Bundle("~/bundles/synchronization").Include(
                "~/Scripts/synchronization.js"));

            bundles.Add(new Bundle("~/bundles/syncsettings").Include(
                "~/Scripts/syncsettings.js"));

            bundles.Add(new Bundle("~/bundles/syncscheduler").Include(
                "~/Scripts/syncscheduler.js"));

            bundles.Add(new Bundle("~/bundles/syncQueue").Include(
                "~/Scripts/syncQueue.js"));

            bundles.Add(new Bundle("~/bundles/manageSynchronizations").Include(
                "~/Scripts/app/manageSynchronizations.js"));

            bundles.Add(new Bundle("~/bundles/databaseSettings").Include(
                "~/Scripts/app/databaseSettings.js"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-multiselect.js",
                "~/Scripts/bootstrap-switch.min.js",
                "~/Scripts/Chart.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/jquery-ui.css",
                "~/Content/alertifyjs/alertify.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-multiselect.css",
                "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.min.css",
                "~/Content/site.css",
                "~/Content/css/select2.css",
                "~/Content/font-awesome.css",
                "~/Content/kendo/2019.1.220/kendo.common.min.css",
                "~/Content/kendo/2019.1.220/kendo.default.min.css",
                "~/Content/kendo/2019.1.220/kendo.common-bootstrap.min.css",
                "~/Content/kendo/2019.1.220/kendo.rtl.min.css",
                "~/Content/kendo/2019.1.220/kendo.bootstrap.min.css",
                "~/Content/kendo/2019.1.220/kendo.silver.min.css"));

        }
    }
}