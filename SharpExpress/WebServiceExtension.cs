using System;
using System.Reflection;
using System.Web.Routing;
using System.Web.Services;

namespace SharpExpress
{
	// TODO fast method invoke using LINQ expressions or DynamicMethod

	/// <summary>
	/// Middleware to host web services.
	/// </summary>
	public static class WebServiceExtension
	{
		public static ExpressApplication WebService<T>(this ExpressApplication app, string urlPrefix)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (urlPrefix == null) throw new ArgumentNullException("urlPrefix");

			var type = typeof(T);
			if (type.IsAbstract || type.IsInterface)
				throw new InvalidOperationException("Need concrete type!");

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
					req =>
					{
						req.SetContext();
						var result = method.Invoke(serviceInstance, new object[0]);
						req.Json(result);
					});
			}
			else
			{
				app.Get(
					Combine(urlPrefix, method.Name),
					req =>
					{
						req.SetContext();
						var args = ParseQueryArgs(req, parameters);
						var result = method.Invoke(serviceInstance, args);
						req.Json(result);
					});

				app.Post(
					Combine(urlPrefix, method.Name),
					req =>
					{
						req.SetContext();
						var args = ParseArgs(req, parameters);
						var result = method.Invoke(serviceInstance, args);
						req.Json(result);
					});
			}
		}

		private static object[] ParseQueryArgs(RequestContext req, ParameterInfo[] parameters)
		{
			var query = req.HttpContext.Request.QueryString;
			var args = new object[parameters.Length];
			for (int i = 0; i < args.Length; i++)
			{
				var param = parameters[i];
				var val = query.Get(param.Name);
				if (val != null)
				{
					args[i] = val.ConvertTo(param.ParameterType);
				}
			}
			return args;
		}

		private static object[] ParseArgs(RequestContext req, ParameterInfo[] parameters)
		{
			var dictionary = req.ParseJson();
			var args = new object[parameters.Length];
			for (int i = 0; i < args.Length; i++)
			{
				var param = parameters[i];
				object val;
				if (dictionary.TryGetValue(param.Name, out val))
				{
					args[i] = val.ConvertTo(param.ParameterType);
				}
			}
			return args;
		}

		private static string Combine(string prefix, string suffix)
		{
			return prefix.EndsWith("/")
				? prefix + suffix
				: prefix + "/" + suffix;
		}
	}
}
