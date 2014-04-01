using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Routing;

namespace SharpExpress
{
	/// <summary>
	/// Reusable handler for embedded resources.
	/// </summary>
	public static class EmbeddedExtension
	{
		/// <summary>
		/// Serves embedded resources of specified assembly.
		/// </summary>
		/// <param name="app">The application to extend.</param>
		/// <param name="urlPrefix">The url prefix.</param>
		/// <param name="assembly">The assembly to get resources from.</param>
		/// <param name="resourcePrefix">The name prefix of embedded resource.</param>
		public static ExpressApplication Embedded(this ExpressApplication app,
			string urlPrefix, Assembly assembly, string resourcePrefix)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (assembly == null) throw new ArgumentNullException("assembly");

			assembly
				.GetManifestResourceNames()
				.Where(s => s.StartsWith(resourcePrefix))
				.ToList()
				.ForEach(name =>
				{
					var path = name.Substring(resourcePrefix.Length);

					app.Get(Combine(urlPrefix, path), req =>
					{
						using (var rs = assembly.GetManifestResourceStream(name))
						{
							req.ContentTypeByPath(name);
							req.SendStream(rs);
						}
					});
				});

			return app;
		}

		public static void SendStream(this RequestContext req, Stream stream)
		{
			if (req == null) throw new ArgumentNullException("req");

			var res = req.HttpContext.Response;
			res.StatusCode = (int)HttpStatusCode.OK;

			stream.CopyTo(res.OutputStream);
		}

		private static string Combine(string prefix, string suffix)
		{
			if (string.IsNullOrEmpty(prefix)) return suffix;
			return prefix.EndsWith("/") ? prefix + suffix : prefix + "/" + suffix;
		}
	}
}
