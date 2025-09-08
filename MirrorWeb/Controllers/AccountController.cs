using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using MirrorWeb.AdAuthorizationFilter;
using MirrorWeb.ViewModels;

namespace MirrorWeb.Controllers
{
    public class AccountController : Controller
    {

        /// <summary>
        /// Gets the authentication manager
        /// </summary>
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        /// <summary>
        /// GET: /Account/Login 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Account/Login 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult<ActionResult>(View(model));
            }

            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            var authService = new AdAuthenticationService(authenticationManager);

            var authenticationResult = authService.SignIn(model.UserName, model.Password);

            if (authenticationResult.IsSuccess)
            {
                // we are in!
                return Task.FromResult(RedirectToLocal(returnUrl));
            }

            ModelState.AddModelError("", authenticationResult.ErrorMessage);
            return Task.FromResult<ActionResult>(View(model));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(Startup.MyAuthentication.ApplicationCookie);

            return RedirectToAction("Dashboard", "Manage");
        }

        /// <summary>
        /// redirects to local
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Dashboard", "Manage");
        }
    }
}