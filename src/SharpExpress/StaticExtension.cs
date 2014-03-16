using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace SharpExpress
{
	/// <summary>
	/// Static middleware to serve static files.
	/// </summary>
	public static class StaticExtension
	{
		/// <summary>
		/// Serves specified directory.
		/// </summary>
		/// <param name="app">The application to extend.</param>
		/// <param name="url">The url prefix.</param>
		/// <param name="dir">The directory to serve.</param>
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
				var relpath = req.ResolveRelativePath(url);
				var path = Path.Combine(dir, relpath);
				req.SendFile(path);
			});
		}

		internal static string ResolveRelativePath(this RequestContext req, string url)
		{
			// TODO tests
			var vpath = req.HttpContext.Request.Path;
			var regex = new Regex(@"\{[^\}]*\*[^\}]*\}");
			var m = regex.Match(url);
			return vpath.Substring(m.Success ? m.Index : url.Length).TrimStart('/');
		}

		public static void SendFile(this RequestContext req, string path)
		{
			req.ContentTypeByPath(path);

			var res = req.HttpContext.Response;
			res.StatusCode = (int)HttpStatusCode.OK;
			
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
