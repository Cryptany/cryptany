using System.Web.Mvc;
using Recaptcha;

namespace Uniplat.Controllers
{
    public class MainController : Controller
    {
        //
        // GET: /Main/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Vacancies()
        {
            return View();
        }

        public ActionResult Contacts()
        {
            return View();
        }

        public ActionResult News()
        {
            return View();
        }
        
        public ActionResult Partners()
        {
            return View();
        }

        public ActionResult Projects()
        {
            return View();
        }

        [HttpPost]
        [RecaptchaControlMvc.CaptchaValidator]
        public ActionResult Captcha(bool captchaValid = true)
        {
            if (captchaValid)
                ViewData["captcha"] = "ok))";
            else
                ViewData["captcha"] = "Error!!!";
            return View("Index");
        }

    }
}
