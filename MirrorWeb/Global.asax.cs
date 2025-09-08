using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MirrorWeb
{
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //WebConfig.Register();

            JsonSerializerSettings serializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;

            var contractResolver = (DefaultContractResolver)serializerSettings.ContractResolver;
            contractResolver.IgnoreSerializableAttribute = true;
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.UseXmlSerializer = true;
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        protected void Application_PreSendRequestHeaders()
        {
            //remove IIS Version from header. can not be done in web.config
            Response.Headers.Remove("Server");

            //this can be done in web.config
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();

            if (!Debugger.IsAttached) Server.ClearError();

            if (exc is HttpUnhandledException && exc.InnerException != null)
            {
                Log.Error("Unhandled Exception", exc.InnerException);
            }
            else
            {
                Log.Error("Unhandled Exception", exc);
            }

            
        }
    }
}
