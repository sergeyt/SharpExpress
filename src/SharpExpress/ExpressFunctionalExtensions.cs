using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace SharpExpress
{
	// TODO support default values for arguments

	public static class ExpressFunctionalExtensions
	{
		private static string[] ParseRouteParams(string url)
		{
			var regex = new Regex(@"\{(?<p>[^\{]+)\}");
			return (from Match m in regex.Matches(url) select m.Groups["p"].Value).ToArray();
		}

		private static void Send(RequestContext req, object data)
		{
			var type = req.HttpContext.Request.ContentType ?? "";
			if (type.IndexOf("xml", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				req.Xml(data);
				return;
			}

			req.Json(data);
		}

		public static ExpressApplication Get<TResult>(this ExpressApplication app, string url, Func<TResult> func)
		{
			return app.Get(url, req => Send(req, func()));
		}

		public static ExpressApplication Get<T1, TResult>(this ExpressApplication app, string url, Func<T1, TResult> func)
		{
			var p = ParseRouteParams(url);
			return app.Get(url, req =>
			{
				var arg1 = req.Param<T1>(p[0]);
				var result = func(arg1);
				Send(req, result);
			});
		}

		public static ExpressApplication Get<T1, T2, TResult>(this ExpressApplication app, string url,
			Func<T1, T2, TResult> func)
		{
			var p = ParseRouteParams(url);
			return app.Get(url, req =>
			{
				var arg1 = req.Param<T1>(p[0]);
				var arg2 = req.Param<T2>(p[1]);
				var result = func(arg1, arg2);
				Send(req, result);
			});
		}

		public static ExpressApplication Get<T1, T2, T3, TResult>(this ExpressApplication app, string url,
			Func<T1, T2, T3, TResult> func)
		{
			var p = ParseRouteParams(url);
			return app.Get(url, req =>
			{
				var arg1 = req.Param<T1>(p[0]);
				var arg2 = req.Param<T2>(p[1]);
				var arg3 = req.Param<T3>(p[2]);
				var result = func(arg1, arg2, arg3);
				Send(req, result);
			});
		}

		public static ExpressApplication Post<T, TResult>(this ExpressApplication app, string url, Func<T, TResult> func)
		{
			return app.Post(url, req =>
			{
				var arg = req.ParseJson<T>();
				var result = func(arg);
				Send(req, result);
			});
		}
	}
}
