using System;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Fleck;
using WebLeap.Models;

namespace WebLeap
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : HttpApplication
	{
		private static Lazy<WebSocketServer> _webSocketServerLazy;
		public static WebSocketServer WebSocketServer { get { return _webSocketServerLazy.Value; } }

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_BeginRequest()
		{
			Interlocked.CompareExchange(ref _webSocketServerLazy,
			                            new Lazy<WebSocketServer>(() => LeapWebSocketServer.Start(Request.Url.Host)),
			                            null);
		}

		protected void Application_End()
		{
			if(WebSocketServer != null)
				WebSocketServer.Dispose();
		}
	}
}