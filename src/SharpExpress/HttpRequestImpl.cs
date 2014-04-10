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
		private readonly string _protocol;
			
		public HttpRequestImpl(HttpContextImpl context, IHttpChannel channel, HttpServerSettings settings)
		{
			_context = context;

			var stream = channel.Receive();
			var header = stream.ReadHeader();

			var baseUri = String.Format("http://localhost:{0}", settings.Port);

			if (header.Length > 0)
			{
				var firstLine = header[0].Split(' ');
				Method = firstLine[0];
				Uri = new Uri(baseUri + firstLine[1]);
				_protocol = firstLine.Length == 3 ? firstLine[2] : "HTTP/1.0";
			}
			else
			{
				Method = "GET";
				Uri = new Uri(baseUri + "/404");
				_protocol = "HTTP/1.0";
			}

			_headers.AddRange(
				from l in header.Skip(1)
				let i = l.IndexOf(':')
				where i >= 0
				let key = l.Substring(0, i).Trim()
				let val = l.Substring(i + 1).Trim()
				select new KeyValuePair<string, string>(key, val)
				);

			var len = _headers.GetContentLength();
			Body = len > 0 ? (Stream) stream : new MemoryStream(new byte[0], false);
		}

		public override bool IsAuthenticated
		{
			get { return _context.IsAuthenticated; }
		}
	}
}
