using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SharpExpress;

namespace ConsoleServer
{
	internal static class Program
	{
		private static void Main(string[] args)
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

			Func<double, double> square = x => x*x;

			app.Get("", req => req.Text("Hi!"))
				.Get(
					"text/{x}/{y}",
					req => req.Text(
						string.Format("x={0}, y={1}",
							req.RouteData.Values["x"],
							req.RouteData.Values["y"]))
				)
				.Get(
					"json/{x}/{y}",
					req => req.Json(
						new
						{
							x = req.RouteData.Values["x"],
							y = req.RouteData.Values["y"]
						})
				)
				.Get("math/square/{x}", square)
				.WebService<MathService>("math.svc");

			var port = options.Get("port", 1111);
			var mode = options.Get("mode", "tcp");

			var settings = new HttpServerSettings {Port = port};

			switch (mode.ToLowerInvariant())
			{
				case "http":
					settings.Mode = HttpServerMode.HttpListener;
					break;

				default:
					settings.Mode = HttpServerMode.TcpListener;
					break;
			}

			using (new HttpServer(app, settings))
			{
				Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
				Console.ReadLine();
			}
		}

		private static T Get<T>(this IDictionary<string, string> options, string name, T defaultValue)
		{
			string val;
			if (!options.TryGetValue(name, out val))
				return defaultValue;
			return (T) Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
		}
	}
}
