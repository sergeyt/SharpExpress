using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace SharpExpress
{
	/// <summary>
	/// HTTP router inspired by express.js.
	/// </summary>
	public sealed class ExpressApplication : IHttpHandler
	{
		private static readonly RouteCollection EmptyRoutes = new RouteCollection();

		private readonly IDictionary<string, RouteCollection> _routes =
			new Dictionary<string, RouteCollection>(StringComparer.OrdinalIgnoreCase);

		private ExpressApplication Register(string verb, string routeUrl, Action<RequestContext> handler)
		{
			RouteCollection routes;
			if (!_routes.TryGetValue(verb, out routes))
			{
				routes = new RouteCollection();
				_routes.Add(verb, routes);
			}
			routes.Add(new Route(routeUrl, new RouteHandler(handler)));
			return this;
		}

		public ExpressApplication Get(string routeUrl, Action<RequestContext> handler)
		{
			return Register("GET", routeUrl, handler);
		}

		public ExpressApplication Post(string routeUrl, Action<RequestContext> handler)
		{
			return Register("POST", routeUrl, handler);
		}

		public bool Process(HttpContextBase context)
		{
			RouteCollection routes;
			if (!_routes.TryGetValue(context.Request.HttpMethod, out routes))
			{
				// TODO send method is not allowed
				routes = EmptyRoutes;
			}
				
			var data = routes.GetRouteData(context);
			if (data != null)
			{
				var handler = data.RouteHandler as RouteHandler;
				if (handler != null)
				{
					handler.Process(new RequestContext(context, data));
					return true;
				}
			}

			return false;
		}

		public void ProcessRequest(HttpContext context)
		{
			var ctx = new HttpContextWrapper(context);

			try
			{
				if (!Process(ctx))
				{
					ctx.Response.StatusCode = (int) HttpStatusCode.NotFound;
					ctx.Response.ContentType = "text/plain";
					ctx.Response.Write(string.Format("Cannot resolve resource '{0}'", ctx.Request.Url));
				}
			}
			catch (Exception e)
			{
				ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				ctx.Response.ContentType = "text/plain";
				ctx.Response.Write(string.Format("{0} request failed. Exception: {1}", ctx.Request.Url, e));
			}
		}

		public bool IsReusable
		{
			get { return true; }
		}

		#region class RouteHandler

		private class RouteHandler : IRouteHandler
		{
			private readonly Action<RequestContext> _handler;

			public RouteHandler(Action<RequestContext> handler)
			{
				_handler = handler;
			}

			public void Process(RequestContext requestContext)
			{
				_handler(requestContext);
			}

			public IHttpHandler GetHttpHandler(RequestContext requestContext)
			{
				return new HttpHandler(_handler, requestContext.RouteData);
			}
		}

		#endregion

		#region class HttpHandler

		private class HttpHandler : IHttpHandler
		{
			private readonly Action<RequestContext> _handler;
			private readonly RouteData _routeData;

			public HttpHandler(Action<RequestContext> handler, RouteData routeData)
			{
				_handler = handler;
				_routeData = routeData;
			}

			public void ProcessRequest(HttpContext context)
			{
				_handler(new RequestContext(new HttpContextWrapper(context), _routeData));
			}

			public bool IsReusable { get { return true; } }
		}

		#endregion
	}
}
