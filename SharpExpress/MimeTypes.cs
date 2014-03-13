using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Routing;

namespace SharpExpress
{
	public static class MimeTypes
	{
		// TODO support more mime types
		private static readonly Dictionary<string, string> Map
			= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{".css", "text/css"},
				{".html", "text/html"},
				{".htm", "text/html"},
				{".js", "text/javascript"},
				{".json", "application/json"},
				{".xml", "text/xml"}
			};

		public static void ContentTypeByPath(this RequestContext req, string path)
		{
			ContentTypeByExtension(req, Path.GetExtension(path));
		}

		public static void ContentTypeByExtension(this RequestContext req, string ext)
		{
			if (req == null) throw new ArgumentNullException("req");

			string type;
			if (!Map.TryGetValue(ext, out type))
				throw new NotSupportedException("Unsupported file type");

			req.HttpContext.Response.ContentType = type;
		}
	}
}
