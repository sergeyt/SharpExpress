using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Common implementation of <see cref="HttpRequestBase"/>.
	/// </summary>
	internal sealed class HttpRequestImpl : HttpRequestBaseImpl
	{
		private readonly HttpContextImpl _context;
			
		public HttpRequestImpl(HttpContextImpl context, IHttpChannel channel, HttpServerSettings settings)
		{
			_context = context;

			Method = channel.Method;

			var path = channel.Path;
			if (!path.StartsWith("/")) path = "/" + path;
			Uri = new Uri(string.Format("http://localhost:{0}{1}", settings.Port, path));

			_headers.AddRange(
				from key in channel.Headers.AllKeys
				select new KeyValuePair<string, string>(key, channel.Headers[key])
				);

			var len = _headers.GetContentLength();
			Body = len > 0 ? channel.Body : new MemoryStream(new byte[0], false);
		}

		public override bool IsAuthenticated
		{
			get { return _context.IsAuthenticated; }
		}
	}
}
