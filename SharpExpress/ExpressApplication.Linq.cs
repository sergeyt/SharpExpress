using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Routing;

namespace SharpExpress
{
	// TODO support funcs with 2-5 arguments

	// LINQ API
	partial class ExpressApplication
	{
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

		private ExpressApplication Get<TResult, T1>(string url,
			Expression<Func<T1, TResult>> expression,
			Action<RequestContext, TResult> send)
		{
			Func<T1, TResult> func = null;

			return Get(url, req =>
			{
				if (func == null)
				{
					func = expression.Compile();
				}

				var value = req.RouteData.Values.Values.First();
				var arg = (T1) Convert.ChangeType(value, typeof(T1), CultureInfo.InvariantCulture);
				var result = func(arg);
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

		public ExpressApplication Xml<TResult>(string url, Expression<Func<TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Xml(res));
		}

		public ExpressApplication Xml<T1, TResult>(string url, Expression<Func<T1, TResult>> expression)
		{
			return Get(url, expression, (req, res) => req.Xml(res));
		}
	}
}
