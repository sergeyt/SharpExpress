using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Base implementation of <see cref="HttpRequestBase"/> with minimum methods to override.
	/// </summary>
	internal abstract class HttpRequestBaseImpl : HttpRequestBase, IHttpRequest
	{
		protected readonly NameValueCollection _headers = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
		private NameValueCollection _queryString;

		protected HttpRequestBaseImpl()
		{
			Body = Stream.Null;
		}

		// required members to be implemented
		public override string HttpMethod { get { return Method ?? "GET"; } }
		public override Uri Url { get { return Uri; } }
		public override NameValueCollection Headers { get { return _headers; } }
		public Stream Body { get; protected set; }

		protected string Method { get; set; }
		protected Uri Uri { get; set; }

		public override Stream InputStream
		{
			get { return Body; }
		}

		public override string[] AcceptTypes
		{
			get
			{
				var val = Headers.Get(HttpRequestHeader.Accept);
				return (from x in val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					let s = x.Trim()
					where s.Length > 0
					select s).ToArray();
			}
		}

		public override string ContentType
		{
			get { return Headers.Get(HttpRequestHeader.ContentType); }
			set { Headers.Set(HttpRequestHeader.ContentType, value); }
		}

		public override int ContentLength
		{
			get { return Headers.GetContentLength(); }
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
			get { return Url.LocalPath; }
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
			get { return Url.LocalPath.TrimStart('/'); }
		}

		public override string RawUrl
		{
			get { return Url.ToString(); }
		}

		public override string RequestType { get; set; }

		public override int TotalBytes
		{
			get { return (int)Body.Length; }
		}

		public override Uri UrlReferrer
		{
			get
			{
				var s = Headers.Get(HttpRequestHeader.Referer);
				Uri result;
				if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out result))
					return result;
				return null;
			}
		}

		public override string UserAgent
		{
			get { return Headers.Get(HttpRequestHeader.UserAgent); }
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
			get { return Headers.Get(HttpRequestHeader.Host); }
		}

		public Encoding Encoding { get; set; }

		public override NameValueCollection QueryString
		{
			get
			{
				if (_queryString == null && Url != null)
				{
					_queryString = SharpExpress.QueryString.Parse(Url.Query).ToNameValueCollection(StringComparer.OrdinalIgnoreCase);
				}
				return _queryString;
			}
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

		public override byte[] BinaryRead(int count)
		{
			var buf = new byte[count];
			Body.Read(buf, 0, count);
			return buf;
		}

		public override void ValidateInput()
		{
		}

		public override bool IsAuthenticated
		{
			get { return true; }
		}
	}
}
