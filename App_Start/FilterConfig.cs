using System.Web.Mvc;

namespace MovieOCD.App_Start
{
    public class FilterConfig
    {
        internal static void RegisterGlobalFilters(GlobalFilterCollection globalFilterCollection)
        {
            globalFilterCollection.Add(new HandleErrorAttribute());
           
        }
    }

    /*public class GroupAuthorizationAttribute: AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            string securityGroup = WebConfigurationManager.AppSettings["Security.DomainGroup"];
            var user = WindowsIdentity.GetCurrent();

            if (user == null)
            {
                throw new HttpException(401,
                                        "Unauthorized: Access is denied due to server configuration favoring an alternate authentication method.");
            }

            var groupNames = from id in user.Groups
                             select id.Translate(typeof(NTAccount)).Value;

            var principal = new WindowsPrincipal(user);

            if (!string.IsNullOrEmpty(securityGroup) &&  !principal.IsInRole(securityGroup))//!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                throw new HttpException(401,
                                        "Unauthorized: Access is denied due to server configuration favoring an alternate authentication method.");
            }
        } 
    }*/
}