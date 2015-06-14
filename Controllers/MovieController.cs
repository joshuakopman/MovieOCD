using System.Web.Mvc;
using MovieOCD.BusinessLogic;
using System.Configuration;
using MovieOCD.DTO;
using System;

namespace MovieOCD.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieManager _movieMgr;

        public MovieController()
        {
            _movieMgr = new MovieManager();
        }

        public JsonResult Search(string movieName, string year = "")
        {
            if (HttpContext.Request.Headers["APIKey"] == ConfigurationManager.AppSettings["PrivateKey"])
            {
                try
                {
                    return Json(_movieMgr.RetrieveMovieData(movieName, year), JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    DebugManager.LogException(e);
                    return Json(new ErrorMessageDTO { Message = "Internal Error: Check Error Log For Exception Details", Provider = "MovieOCD" });
                }
            }

            return Json(new ErrorMessageDTO { Message = "Invalid API Key", Provider = "MovieOCD" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Reviews(string id)
        {
            if (HttpContext.Request.Headers["APIKey"] == ConfigurationManager.AppSettings["PrivateKey"])
            {
                try
                {
                    return Json(_movieMgr.RetrieveReviewData(id), JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    DebugManager.LogException(e);
                    return Json(new ErrorMessageDTO { Message = "Internal Error: Check Error Log For Exception Details", Provider = "MovieOCD" });
                }
            }

            return Json(new ErrorMessageDTO { Message = "Invalid API Key", Provider = "MovieOCD" }, JsonRequestBehavior.AllowGet);
        }

        public void Clear()
        {
            CacheManager.Instance.Clear();
            DebugManager.LogWarning("Cache Was Cleared Successfully");
        }

        public ActionResult Facebook()
        {
            return View("Facebook");
        }
    }
}