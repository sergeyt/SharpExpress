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

		private static ExpressApplication Get<TResult>(this ExpressApplication app, string url,
			Func<TResult> func, Action<RequestContext, TResult> send)
		{
			return app.Get(url, req => send(req, func()));
		}

		private static ExpressApplication Get<T1, TResult>(this ExpressApplication app, string url,
			Func<T1, TResult> func, Action<RequestContext, TResult> send)
		{
			var p = ParseRouteParams(url);
			return app.Get(url, req =>
			{
				var arg1 = req.Param<T1>(p[0]);
				var result = func(arg1);
				send(req, result);
			});
		}

		private static ExpressApplication Get<T1, T2, TResult>(this ExpressApplication app, string url,
			Func<T1, T2, TResult> func, Action<RequestContext, TResult> send)
		{
			var p = ParseRouteParams(url);
			return app.Get(url, req =>
			{
				var arg1 = req.Param<T1>(p[0]);
				var arg2 = req.Param<T2>(p[1]);
				var result = func(arg1, arg2);
				send(req, result);
			});
		}

		private static ExpressApplication Get<T1, T2, T3, TResult>(this ExpressApplication app, string url,
			Func<T1, T2, T3, TResult> func, Action<RequestContext, TResult> send)
		{
			var p = ParseRouteParams(url);
			return app.Get(url, req =>
			{
				var arg1 = req.Param<T1>(p[0]);
				var arg2 = req.Param<T2>(p[1]);
				var arg3 = req.Param<T3>(p[2]);
				var result = func(arg1, arg2, arg3);
				send(req, result);
			});
		}

		public static ExpressApplication Json<TResult>(this ExpressApplication app, string url, Func<TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Json(res));
		}

		public static ExpressApplication Json<T1, TResult>(this ExpressApplication app, string url, Func<T1, TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Json(res));
		}

		public static ExpressApplication Json<T1, T2, TResult>(this ExpressApplication app, string url, Func<T1, T2, TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Json(res));
		}

		public static ExpressApplication Json<T1, T2, T3, TResult>(this ExpressApplication app, string url, Func<T1, T2, T3, TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Json(res));
		}

		public static ExpressApplication Xml<TResult>(this ExpressApplication app, string url, Func<TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Xml(res));
		}

		public static ExpressApplication Xml<T1, TResult>(this ExpressApplication app, string url, Func<T1, TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Xml(res));
		}

		public static ExpressApplication Xml<T1, T2, TResult>(this ExpressApplication app, string url, Func<T1, T2, TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Xml(res));
		}

		public static ExpressApplication Xml<T1, T2, T3, TResult>(this ExpressApplication app, string url, Func<T1, T2, T3, TResult> func)
		{
			return app.Get(url, func, (req, res) => req.Xml(res));
		}
	}
}
