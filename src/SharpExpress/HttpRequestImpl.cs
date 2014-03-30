using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Common implementation of <see cref="HttpRequestBase"/>.
	/// </summary>
	internal sealed class HttpRequestImpl : HttpRequestBase, IHttpRequest
	{
		private readonly HttpContextImpl _context;
		private readonly string _method;
		private readonly Uri _url;
		private readonly string _protocol;
		private readonly NameValueCollection _queryString;
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
				_queryString = _url.Query.ParseQueryString().ToNameValueCollection(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_method = "GET";
				_url = new Uri(baseUri + "/404");
				_protocol = "HTTP/1.0";
				_queryString = new NameValueCollection();
			}
				
			_headers = (
				from l in header.Skip(1)
				let i = l.IndexOf(':')
				where i >= 0
				let key = l.Substring(0, i).Trim()
				let val = l.Substring(i + 1).Trim()
				select new KeyValuePair<string, string>(key, val)
				).ToNameValueCollection(StringComparer.OrdinalIgnoreCase);

			var len = GetContentLength();
			_body = len > 0 ? (Stream) stream : new MemoryStream(new byte[0], false);
		}

		#region HttpRequestBase Members

		public override byte[] BinaryRead(int count)
		{
			var buf = new byte[count];
			_body.Read(buf, 0, count);
			return buf;
		}

		public override void ValidateInput()
		{
		}

		public override string[] AcceptTypes
		{
			get
			{
				var val = _headers.Get(HttpRequestHeader.Accept);
				return (from x in val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					let s = x.Trim()
					where s.Length > 0
					select s).ToArray();
			}
		}

		public override string ContentType
		{
			get { return _headers.Get(HttpRequestHeader.ContentType); }
			set { _headers.Set(HttpRequestHeader.ContentType, value); }
		}

		public override int ContentLength
		{
			get { return GetContentLength(); }
		}

		public override Encoding ContentEncoding
		{
			get
			{
				// TODO get from headers
				return Encoding.UTF8;
			}
			set { throw new NotSupportedException(); }
		}

		public override HttpCookieCollection Cookies
		{
			get
			{
				var list = new HttpCookieCollection();

				// TODO

				return list;
			}
		}

		public override Stream Filter { get; set; }

		public override string Path
		{
			get { return _url.LocalPath; }
		}

		public override string ApplicationPath
		{
			get { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		public override string AppRelativeCurrentExecutionFilePath
		{
			get { return "~/"; }
		}

		public override HttpBrowserCapabilitiesBase Browser
		{
			get { return base.Browser; }
		}

		public override string PathInfo
		{
			get { return _url.LocalPath.TrimStart('/'); }
		}

		public override string RawUrl
		{
			get { return _url.ToString(); }
		}

		public override string RequestType { get; set; }

		public override int TotalBytes
		{
			get { return (int)_body.Length; }
		}

		public override Uri Url
		{
			get { return _url; }
		}

		public override Uri UrlReferrer
		{
			get
			{
				var s = _headers.Get(HttpRequestHeader.Referer);
				Uri result;
				if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out result))
					return result;
				return null;
			}
		}

		public override string UserAgent
		{
			get { return _headers.Get(HttpRequestHeader.UserAgent); }
		}

		public override string UserHostAddress
		{
			get
			{
				// TODO get from socket LocalEndPoint
				return "";
			}
		}

		public override string UserHostName
		{
			get { return _headers.Get(HttpRequestHeader.Host); }
		}

		public override NameValueCollection Headers
		{
			get { return _headers; }
		}

		public Stream Body
		{
			get { return _body; }
		}

		public Encoding Encoding { get; set; }

		public override NameValueCollection QueryString
		{
			get { return _queryString; }
		}

		public override string HttpMethod
		{
			get { return _method; }
		}

		public override Stream InputStream
		{
			get { return _body; }
		}

		public override bool IsAuthenticated
		{
			get { return _context.IsAuthenticated; }
		}

		public override bool IsLocal
		{
			get
			{
				// TODO
				// return LocalEndPoint.Address == RemoteEndPoint.Address;
				return false;
			}
		}

		public override bool IsSecureConnection
		{
			get { return false; }
		}

		#endregion

		private int GetContentLength()
		{
			var s = _headers.Get(HttpRequestHeader.ContentLength);
			int len;
			return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out len) ? len : 0;
		}
	}
}