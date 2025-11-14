using frontend.App_Start;
using System;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace frontend
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ViewEngines.Engines.Clear();
            //custom engine
            ViewEngines.Engines.Add(new CustomViewEngine());
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                try
                {
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        // This is where we read the roles from the ticket
                        var roles = authTicket.UserData.Split(new char[] { ',' });
                        var genericIdentity = new GenericIdentity(authTicket.Name, "Forms");
                        var genericPrincipal = new GenericPrincipal(genericIdentity, roles);

                        System.Web.HttpContext.Current.User = genericPrincipal;
                    }
                }
                catch (Exception)
                {
                    // Decrypt failed
                }
            }
        }
    }
}
