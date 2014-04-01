using System.Collections.Specialized;
using System.Globalization;
using System.Net;

namespace SharpExpress
{
	internal static class HttpExtensions
	{
		public static string Get(this NameValueCollection headers, HttpRequestHeader header)
		{
			return headers.Get(ToString(header));
		}

		public static void Set(this NameValueCollection headers, HttpRequestHeader header, string value)
		{
			headers.Set(ToString(header), value);
		}

		public static int GetContentLength(this NameValueCollection headers)
		{
			var s = headers.Get(HttpRequestHeader.ContentLength);
			int len;
			return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out len) ? len : 0;
		}

		private static string ToString(HttpRequestHeader value)
		{
			switch (value)
			{
				case HttpRequestHeader.CacheControl:
					return "Cache-Control";
				case HttpRequestHeader.KeepAlive:
					return "Keep-Alive";
				case HttpRequestHeader.TransferEncoding:
					return "Transfer-Encoding";
				case HttpRequestHeader.ContentLength:
					return "Content-Length";
				case HttpRequestHeader.ContentType:
					return "Content-Type";
				case HttpRequestHeader.ContentEncoding:
					return "Content-Encoding";
				case HttpRequestHeader.ContentLanguage:
					return "Content-Language";
				case HttpRequestHeader.ContentLocation:
					return "Content-Location";
				case HttpRequestHeader.ContentMd5:
					return "Content-MD5";
				case HttpRequestHeader.ContentRange:
					return "Content-Range";
				case HttpRequestHeader.LastModified:
					return "Last-Modified";
				case HttpRequestHeader.AcceptCharset:
					return "Accept-Charset";
				case HttpRequestHeader.AcceptEncoding:
					return "Accept-Encoding";
				case HttpRequestHeader.AcceptLanguage:
					return "Accept-Language";
				case HttpRequestHeader.IfMatch:
					return "If-Match";
				case HttpRequestHeader.IfModifiedSince:
					return "If-Modified-Since";
				case HttpRequestHeader.IfNoneMatch:
					return "If-None-Match";
				case HttpRequestHeader.IfRange:
					return "If-Range";
				case HttpRequestHeader.IfUnmodifiedSince:
					return "If-Unmodified-Since";
				case HttpRequestHeader.MaxForwards:
					return "Max-Forwards";
				case HttpRequestHeader.ProxyAuthorization:
					return "Proxy-Authorization";
				case HttpRequestHeader.UserAgent:
					return "User-Agent";
				default:
					return value.ToString();
			}
		}
	}
}