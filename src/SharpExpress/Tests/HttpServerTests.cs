#if NUNIT

using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;

using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture(HttpServerMode.TcpListener)]
	[TestFixture(HttpServerMode.HttpListener)]
	public class HttpServerTests
	{
		private readonly HttpServerSettings _settings;
		private readonly Func<WebClient> _createClient;

		public HttpServerTests(HttpServerMode mode)
		{
			_settings = new HttpServerSettings {Port = 1111, Mode = mode};
			_createClient = () => new WebClient
			{
				BaseAddress = string.Format("http://localhost:{0}", _settings.Port),
				Encoding = Encoding.UTF8
			};
		}

		[TestCase("test")]
		public void Get(string path)
		{
			var app = new ExpressApplication();

			app.Get<string, string>("{name}", name => name);

			using (new HttpServer(app, _settings))
			using (var client = _createClient())
			{
				var json = client.DownloadString(path);
				var serializer = new JavaScriptSerializer();
				var d = serializer.DeserializeObject(json) as IDictionary<string,object>;
				Assert.AreEqual(path, d["d"]);
			}
		}

		[Test]
		public void Post()
		{
			var app = new ExpressApplication();
			app.Post("test", req =>
			{
				var reader = new StreamReader(req.HttpContext.Request.InputStream);
				var s = reader.ReadToEnd();
				req.HttpContext.Response.StatusCode = 200;
				req.HttpContext.Response.Write(s);
			});

			using (new HttpServer(app, _settings))
			using (var client = _createClient())
			{
				var body = Enumerable.Range(0, 1000).Select(i => "abcd").Aggregate("", (acc, val) => acc + val);
				var res = client.UploadData("test", "POST", Encoding.UTF8.GetBytes(body));
				Assert.AreEqual(body, Encoding.UTF8.GetString(res));
			}
		}
	}
}

#endif
