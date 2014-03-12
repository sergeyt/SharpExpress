using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace SharpExpress
{
	// TODO support default values for arguments

	internal static class RouteDataExtensions
	{
		public static T Get<T>(this RouteData data, string name)
		{
			var val = data.Values[name];
			return (T) Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
		}
	}

	// LINQ API
	partial class ExpressApplication
	{
		private static string[] ParseRouteParams(string url)
		{
			var regex = new Regex(@"\{(?<p>[^\{]+)\}");
			return (from Match m in regex.Matches(url) select m.Groups["p"].Value).ToArray();
		}

		private ExpressApplication Get<TResult>(string url,
			Expression<Func<TResult>> expression,
			Action<RequestContext, TResult> send)
		{
			Func<TResult> func = null;

			return Get(url, req =>
			{
				if (func == null)
				{
					func = expression.Compile();
				}

				send(req, func());
			});
		}

		private ExpressApplication Get<T1, TResult>(string url,
			Expression<Func<T1, TResult>> expression,
			Action<RequestContext, TResult> send)
		{
			string[] p = null;
			Func<T1, TResult> func = null;

			return Get(url, req =>
			{
				if (func == null)
				{
					func = expression.Compile();
					p = ParseRouteParams(url);
				}

				var arg1 = req.RouteData.Get<T1>(p[0]);
				var result = func(arg1);
				send(req, result);
			});
		}

		private ExpressApplication Get<T1, T2, TResult>(string url,
			Expression<Func<T1, T2, TResult>> expression,
			Action<RequestContext, TResult> send)
		{
			string[] p = null;
			Func<T1, T2, TResult> func = null;

			return Get(url, req =>
			{
				if (func == null)
				{
					func = expression.Compile();
					p = ParseRouteParams(url);
				}

				var arg1 = req.RouteData.Get<T1>(p[0]);
				var arg2 = req.RouteData.Get<T2>(p[1]);
				var result = func(arg1, arg2);
				send(req, result);
			});
		}

		private ExpressApplication Get<T1, T2, T3, TResult>(string url,
			Expression<Func<T1, T2, T3, TResult>> expression,
			Action<RequestContext, TResult> send)
		{
			string[] p = null;
			Func<T1, T2, T3, TResult> func = null;

			return Get(url, req =>
			{
				if (func == null)
				{
					func = expression.Compile();
					p = ParseRouteParams(url);
				}

				var arg1 = req.RouteData.Get<T1>(p[0]);
				var arg2 = req.RouteData.Get<T2>(p[1]);
				var arg3 = req.RouteData.Get<T3>(p[2]);
				var result = func(arg1, arg2, arg3);
				send(req, result);
			});
		}

		public ExpressApplication Json<TResult>(string url, Expression<Func<TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Json(res));
		}

		public ExpressApplication Json<T1, TResult>(string url, Expression<Func<T1, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Json(res));
		}

		public ExpressApplication Json<T1, T2, TResult>(string url, Expression<Func<T1, T2, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Json(res));
		}

		public ExpressApplication Json<T1, T2, T3, TResult>(string url, Expression<Func<T1, T2, T3, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Json(res));
		}

		public ExpressApplication Xml<TResult>(string url, Expression<Func<TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Xml(res));
		}

		public ExpressApplication Xml<T1, TResult>(string url, Expression<Func<T1, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Xml(res));
		}

		public ExpressApplication Xml<T1, T2, TResult>(string url, Expression<Func<T1, T2, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Xml(res));
		}

		public ExpressApplication Xml<T1, T2, T3, TResult>(string url, Expression<Func<T1, T2, T3, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Xml(res));
		}
	}
}
