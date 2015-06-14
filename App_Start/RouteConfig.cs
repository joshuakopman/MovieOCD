using System.Web.Mvc;
using System.Web.Routing;

namespace MovieOCD.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("");
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("index.html");
            routes.IgnoreRoute("Scripts/{file}.js");
            routes.IgnoreRoute("Styles/{file}.css");
            routes.IgnoreRoute("Images/{file}.jpg");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
              "OpenGraph",                                              // Route name
              "opengraph/{movieName}",                           // URL with parameters
               new { controller = "opengraph", action = "Index", movieName = "" });

            routes.MapRoute(
              "Reviews",                                              // Route name
              "movie/reviews/{id}",                           // URL with parameters
               new { controller = "Movie", action = "Reviews", id = "" });

            routes.MapRoute(
              "Movies", // Route name
              "movie/search/{MovieName}/{Year}", // URL with parameters
              new { controller = "Movie", action = "Search", MovieName = "", Year = UrlParameter.Optional });


            routes.MapRoute(
              "Default",
              "{controller}/{action}/{id}",
              new { controller = "Home", action = "Index", id = UrlParameter.Optional });

        }
    }
}