using System.Web.Mvc;
using System.Web.Routing;
using MovieOCD.App_Start;
using MovieOCD.DataAccess.Context;
using System.Data.Entity;
using MovieOCD.Migrations;
using System.Web.Http;

namespace MovieOCD
{
    //if year is provided, it means it came from a suggested title duplicate, filter the ratings on title AND Year
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MovieOCDDB, Configuration>()); //automatic migrations 
        }
    }
}