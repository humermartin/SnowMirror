using System.Web.Mvc;
using MirrorWeb.ViewModels.Monitoring;

namespace MirrorWeb.Controllers
{
    public class MonitoringController : Controller
    {
        // GET: Monitoring
        public ActionResult Main()
        {
            MonitoringViewModel model = new MonitoringViewModel();
            

            return View(model);
        }

    }
}