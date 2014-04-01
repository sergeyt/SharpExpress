using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Common implementation of <see cref="HttpRequestBase"/>.
	/// </summary>
	internal sealed class HttpRequestImpl : RequestBase
	{
		private readonly HttpContextImpl _context;
		private readonly string _method;
		private readonly Uri _url;
		private readonly string _protocol;
		private readonly NameValueCollection _headers;
		private readonly Stream _body;
			
		public HttpRequestImpl(HttpContextImpl context, IHttpChannel channel, HttpServerSettings settings)
		{
			_context = context;

			var stream = channel.Receive();
			var header = stream.ReadHeader();

			var baseUri = String.Format("http://localhost:{0}", settings.Port);

			if (header.Length > 0)
			{
				var firstLine = header[0].Split(' ');
				_method = firstLine[0];
				_url = new Uri(baseUri + firstLine[1]);
				_protocol = firstLine.Length == 3 ? firstLine[2] : "HTTP/1.0";
			}
			else
			{
				_method = "GET";
				_url = new Uri(baseUri + "/404");
				_protocol = "HTTP/1.0";
			}
				
			_headers = (
				from l in header.Skip(1)
				let i = l.IndexOf(':')
				where i >= 0
				let key = l.Substring(0, i).Trim()
				let val = l.Substring(i + 1).Trim()
				select new KeyValuePair<string, string>(key, val)
				).ToNameValueCollection(StringComparer.OrdinalIgnoreCase);

			var len = _headers.GetContentLength();
			_body = len > 0 ? (Stream) stream : new MemoryStream(new byte[0], false);
		}

		public override string HttpMethod
		{
			get { return _method; }
		}

		public override Uri Url
		{
			get { return _url; }
		}

		public override NameValueCollection Headers
		{
			get { return _headers; }
		}

		public override Stream Body
		{
			get { return _body; }
		}

		public override bool IsAuthenticated
		{
			get { return _context.IsAuthenticated; }
		}
	}
}
