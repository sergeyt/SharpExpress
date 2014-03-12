using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SharpExpress;

namespace ConsoleServer
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			var options = (from arg in args
				where arg.StartsWith("--")
				let pair = arg.Substring(2).Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries)
				where pair.Length == 2
				let key = pair[0].Trim()
				let val = pair[1].Trim()
				select new KeyValuePair<string, string>(key, val))
				.ToDictionary(x => x.Key, x => x.Value, StringComparer.InvariantCultureIgnoreCase);

			var app = new ExpressApplication();
			app.Get(
				"",
				req => req.Text("Hi!")
				);

			app.Get(
				"text/{x}/{y}",
				req => req.Text(
					string.Format("x={0}, y={1}",
						req.RouteData.Values["x"],
						req.RouteData.Values["y"])
					));

			app.Get(
				"json/{x}/{y}",
				req => req.Json(
					new
					{
						x = req.RouteData.Values["x"],
						y = req.RouteData.Values["y"]
					}));

			var port = options.Get("port", 1111);
			using (new HttpServer(app, port, 4))
			{
				Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
				Console.ReadLine();
			}
		}
	}

	internal static class Extensions
	{
		public static T Get<T>(this IDictionary<string, string> options, string name, T defaultValue)
		{
			string val;
			if (!options.TryGetValue(name, out val))
				return defaultValue;
			return (T) Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
		}
	}
}
