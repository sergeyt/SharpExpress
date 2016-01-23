using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace SharpExpress
{
	/// <summary>
	/// HTTP router inspired by express.js.
	/// </summary>
	public partial class ExpressApplication : IHttpHandler
	{
		private readonly IList<ExpressApplication> _handlers;
		private static readonly RouteCollection EmptyRoutes = new RouteCollection();

		public ExpressApplication()
			: this(Enumerable.Empty<ExpressApplication>())
		{
		}

		public ExpressApplication(IEnumerable<ExpressApplication> handlers)
		{
			_handlers = (handlers ?? Enumerable.Empty<ExpressApplication>()).ToList().AsReadOnly();
		}

		#region Routes API

		private readonly IDictionary<string, RouteCollection> _routes =
			new Dictionary<string, RouteCollection>(StringComparer.OrdinalIgnoreCase);

		private ExpressApplication Register(string verb, string route, Action<RequestContext> handler)
		{
			RouteCollection routes;
			if (!_routes.TryGetValue(verb, out routes))
			{
				routes = new RouteCollection();
				_routes.Add(verb, routes);
			}
			routes.Add(new Route(route, new RouteHandler(handler)));
			return this;
		}

		/// <summary>
		/// Registers GET handler on given route.
		/// </summary>
		public ExpressApplication Get(string route, Action<RequestContext> handler)
		{
			return Register("GET", route, handler);
		}

		/// <summary>
		/// Registers POST handler on given route.
		/// </summary>
		public ExpressApplication Post(string route, Action<RequestContext> handler)
		{
			return Register("POST", route, handler);
		}

		/// <summary>
		/// Registers HEAD handler on given route.
		/// </summary>
		public ExpressApplication Head(string route, Action<RequestContext> handler)
		{
			return Register("HEAD", route, handler);
		}

		/// <summary>
		/// Registers PUT handler on given route.
		/// </summary>
		public ExpressApplication Put(string route, Action<RequestContext> handler)
		{
			return Register("PUT", route, handler);
		}

		/// <summary>
		/// Registers UPDATE handler on given route.
		/// </summary>
		public ExpressApplication Update(string route, Action<RequestContext> handler)
		{
			return Register("UPDATE", route, handler);
		}

		/// <summary>
		/// Registers DELETE handler on given route.
		/// </summary>
		public ExpressApplication Delete(string route, Action<RequestContext> handler)
		{
			return Register("DELETE", route, handler);
		}

		/// <summary>
		/// Registers OPTIONS handler on given route.
		/// </summary>
		public ExpressApplication Options(string route, Action<RequestContext> handler)
		{
			return Register("OPTIONS", route, handler);
		}

		/// <summary>
		/// Registers handler on given route for any http method.
		/// </summary>
		public ExpressApplication All(string route, Action<RequestContext> handler)
		{
			string[] verbs = {"GET", "POST", "HEAD", "PUT", "UPDATE", "DELETE", "OPTIONS"};
			Array.ForEach(verbs, verb => Register(verb, route, handler));
			return this;
		}

		#endregion

		#region IHttpHandler Impl

		public bool LogEnabled { get; set; }

		public bool Process(HttpContextBase context)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (context.Request == null)
				return false; // unit test?

			var method = context.Request.HttpMethod;
			var path = context.Request.Url.PathAndQuery;
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				return ProcessImpl(context);
			}
			finally
			{
				if (LogEnabled)
				{
					stopwatch.Stop();
					var status = context.Response.StatusCode;
					Console.WriteLine("{0} {1} - {2} in {3}ms", method, path, status, stopwatch.Elapsed.TotalMilliseconds);
				}
			}
		}

		private bool ProcessImpl(HttpContextBase context)
		{
			if (string.IsNullOrEmpty(context.Request.HttpMethod))
				return false;

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

			return _handlers.Any(x => x.Process(context));
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

		#endregion

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
