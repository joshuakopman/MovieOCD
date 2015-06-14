using System.Net;
using System.Web.Mvc;
using MovieOCD.DTO;

namespace MovieOCD.Controllers
{
    public class OpenGraphController : Controller
    {
        //
        // GET: /OpenGraph/

        public ActionResult Index(string movieName)
        {
            var path = "";
            var summary = "";
           if (!string.IsNullOrEmpty(Request.QueryString["movieImagePath"]))
           {
               path = Request.QueryString["movieImagePath"];
           }
           if (!string.IsNullOrEmpty(Request.QueryString["summary"]))
           {
               summary = Request.QueryString["summary"];
           }
          // var client = new WebClient();
         //  string tinyUrl = client.DownloadString("http://tinyurl.com/api-create.php?url=" + Request.Url);
           var openGraphDTO = new OpenGraphDTO { MovieName = movieName, MovieImagePath = path, PlotSummary = summary };
           
            return View("opengraph", openGraphDTO);
        }

    }
}
