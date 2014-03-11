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

			var port = options.Get("port", 1111);
			using (var server = new HttpServer(port))
			{
				var app = server.App;
				app.Get("", app.Text("Hello, World!"));

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
