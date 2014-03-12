using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace SharpExpress
{
	/// <summary>
	/// Static middleware to serve static files.
	/// </summary>
	public static class StaticMiddleware
	{
		/// <summary>
		/// Serves specified directory
		/// </summary>
		/// <param name="app">The application to extend.</param>
		/// <param name="url"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		public static ExpressApplication Static(this ExpressApplication app, string url, string dir)
		{
			if (!Directory.Exists(dir))
			{
				throw new DirectoryNotFoundException(
					string.Format("Specified directory '{0}' does not exist.", dir)
					);
			}

			return app.Get(url, req =>
			{
				var path = ResolveFile(url, dir, req.HttpContext.Request.Path);
				req.SendFile(path);
			});
		}

		private static string ResolveFile(string url, string dir, string vpath)
		{
			// TODO fix path resolving
			var regex = new Regex(@"\{\*[^\}]+\}");
			var m = regex.Match(url);
			var relpath = vpath.Substring(m.Success ? m.Index : url.Length).TrimStart('/');
			return Path.Combine(dir, relpath);
		}

		private static readonly Dictionary<string, string> MimeTypes
			= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{".css", "text/css"},
				{".html", "text/html"},
				{".htm", "text/html"},
				{".js", "text/javascript"},
				{".json", "application/json"},
				{".xml", "text/xml"}
			};

		public static void SendFile(this RequestContext req, string path)
		{
			var ext = Path.GetExtension(path);

			string type;
			if (!MimeTypes.TryGetValue(ext, out type))
				throw new NotSupportedException("Unsupported file type");

			var res = req.HttpContext.Response;
			res.StatusCode = (int)HttpStatusCode.OK;
			res.ContentType = type;

			using (var fs = File.OpenRead(path))
				fs.CopyTo(res.OutputStream);
		}

		public static void CopyTo(this Stream stream, Stream target)
		{
			var buf = new byte[4*1024];
			int len;
			while ((len = stream.Read(buf, 0, buf.Length)) > 0)
			{
				target.Write(buf, 0, len);
			}
		}
	}
}
