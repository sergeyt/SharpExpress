using System;
using System.Reflection;
using System.Web.Services;

namespace SharpExpress
{
	/// <summary>
	/// Middleware to host web services.
	/// </summary>
	public static class WebServiceHandler
	{
		public static ExpressApplication WebService<T>(this ExpressApplication app, string urlPrefix)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (urlPrefix == null) throw new ArgumentNullException("urlPrefix");

			var type = typeof(T);
			var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			var service = Activator.CreateInstance<T>();

			foreach (var method in methods)
			{
				var meta = method.GetAttribute<WebMethodAttribute>(true);
				if (meta == null) continue;

				RegisterMethod(app, urlPrefix, service, method, meta);
			}

			return app;
		}

		private static void RegisterMethod(this ExpressApplication app, string urlPrefix, object serviceInstance,
			MethodBase method, WebMethodAttribute meta)
		{
			// TODO support WebMethodAttribute options (caching, etc)

			var parameters = method.GetParameters();
			if (parameters.Length == 0)
			{
				app.Get(
					Combine(urlPrefix, method.Name),
					req => req.Json(method.Invoke(serviceInstance, new object[0]))
					);
			}
			else
			{
				app.Post(
					Combine(urlPrefix, method.Name),
					req =>
					{
						// parse json and invoke method
						throw new NotImplementedException();
					});
			}
		}

		private static string Combine(string urlPrefix, string suffix)
		{
			return urlPrefix.EndsWith("/")
				? urlPrefix + suffix
				: urlPrefix + "/" + suffix;
		}
	}
}
