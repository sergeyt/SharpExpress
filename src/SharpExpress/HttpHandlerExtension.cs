using System;
using System.Web;
using System.Web.Routing;

namespace SharpExpress
{
	/// <summary>
	/// Allows injecting any <see cref="IHttpHandler"/> into express router.
	/// </summary>
	public static class HttpHandlerExtension
	{
		public static ExpressApplication HttpHandler<T>(this ExpressApplication app, string url, string verb)
		{
			if (string.IsNullOrEmpty(verb))
				verb = "*";

			Func<string, bool> hasVerb = v =>
			{
				if (string.Equals(verb, "*")) return true;
				return verb.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0;
			};

			IHttpHandler handler = null;

			Action<RequestContext> action = req =>
			{
				if (handler == null)
				{
					handler = (IHttpHandler) Activator.CreateInstance<T>();
				}

				var context = req.HttpContext.Unwrap();
				handler.ProcessRequest(context);
			};

			if (hasVerb("GET"))
			{
				app.Get(url, action);
			}

			if (hasVerb("POST"))
			{
				app.Post(url, action);
			}

			return app;
		}

		internal static HttpContext Unwrap(this HttpContextBase context)
		{
			var impl = context as HttpListenerContextImpl;
			if (impl != null)
			{
				return impl.HttpContext;
			}

			return new HttpContext(new HttpWorkerRequestImpl(
				context,
				// TODO fix
				new HttpServerSettings
				{
					VirtualDir = "/",
					PhisycalDir = Environment.CurrentDirectory
				}));
		}
	}
}
