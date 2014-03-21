using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Implements <see cref="HttpRequestBase"/> wrapping <see cref="HttpListenerRequest"/>.
	/// </summary>
	internal sealed class HttpRequestImpl : HttpRequestBase, IRequest
	{
		private readonly HttpListenerRequest _request;

		public HttpRequestImpl(HttpListenerRequest request)
		{
			_request = request;
		}

		public override byte[] BinaryRead(int count)
		{
			var buf = new byte[count];
			InputStream.Read(buf, 0, count);
			return buf;
		}

		public override int[] MapImageCoordinates(string imageFieldName)
		{
			return base.MapImageCoordinates(imageFieldName);
		}

		public override string MapPath(string virtualPath)
		{
			return base.MapPath(virtualPath);
		}

		public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			return base.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
		}

		public override void ValidateInput()
		{
		}

		public override void SaveAs(string filename, bool includeHeaders)
		{
			base.SaveAs(filename, includeHeaders);
		}

		public override string[] AcceptTypes
		{
			get { return _request.AcceptTypes; }
		}

		public override string ContentType
		{
			get { return _request.ContentType; }
			set { throw new NotSupportedException(); }
		}

		public override int ContentLength
		{
			get { return (int)_request.ContentLength64; }
		}

		public override HttpClientCertificate ClientCertificate
		{
			get { return base.ClientCertificate; }
		}

		public override Encoding ContentEncoding
		{
			get { return _request.ContentEncoding; }
			set { throw new NotSupportedException(); }
		}

		public override HttpCookieCollection Cookies
		{
			get
			{
				var list = new HttpCookieCollection();

				foreach (Cookie cookie in _request.Cookies)
				{
					list.Add(new HttpCookie(cookie.Name, cookie.Value)
					{
						Domain = cookie.Domain,
						Expires = cookie.Expires,
						HttpOnly = cookie.HttpOnly,
						Path = cookie.Path,
						Secure = cookie.Secure
					});
				}

				return list;
			}
		}

		public override string CurrentExecutionFilePath
		{
			get { return base.CurrentExecutionFilePath; }
		}

		public override string FilePath
		{
			get { return base.FilePath; }
		}

		public override HttpFileCollectionBase Files
		{
			get { return base.Files; }
		}

		public override Stream Filter { get; set; }

		public override NameValueCollection Params
		{
			get { return base.Params; }
		}

		public override string Path
		{
			get { return _request.Url.LocalPath; }
		}

		public override string ApplicationPath
		{
			get { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		public override string AnonymousID
		{
			get { return base.AnonymousID; }
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
			get { return _request.Url.LocalPath.TrimStart('/'); }
		}

		public override string PhysicalApplicationPath
		{
			get { return base.PhysicalApplicationPath; }
		}

		public override string PhysicalPath
		{
			get { return base.PhysicalPath; }
		}

		public override string RawUrl
		{
			get { return _request.Url.ToString(); }
		}

		public override string RequestType { get; set; }

		public override NameValueCollection ServerVariables
		{
			get { return base.ServerVariables; }
		}

		public override int TotalBytes
		{
			get { return (int)InputStream.Length; }
		}

		public override Uri Url
		{
			get { return _request.Url; }
		}

		public override Uri UrlReferrer
		{
			get { return _request.UrlReferrer; }
		}

		public override string UserAgent
		{
			get { return _request.UserAgent; }
		}

		public override string[] UserLanguages
		{
			get { return base.UserLanguages; }
		}

		public override string UserHostAddress
		{
			get { return _request.UserHostAddress; }
		}

		public override string UserHostName
		{
			get { return _request.UserHostName; }
		}

		public override NameValueCollection Headers
		{
			get { return _request.Headers; }
		}

		public Stream Body
		{
			get { return InputStream; }
		}

		public Encoding Encoding { get; set; }

		public override NameValueCollection QueryString
		{
			get { return _request.QueryString; }
		}

		public override string this[string key]
		{
			get { return base[key]; }
		}

		public override NameValueCollection Form
		{
			get { return base.Form; }
		}

		public override string HttpMethod
		{
			get { return _request.HttpMethod; }
		}

		public override Stream InputStream
		{
			get { return _request.InputStream; }
		}

		public override bool IsAuthenticated
		{
			get { return _request.IsAuthenticated; }
		}

		public override bool IsLocal
		{
			get { return _request.IsLocal; }
		}

		public override bool IsSecureConnection
		{
			get { return _request.IsSecureConnection; }
		}

		public override WindowsIdentity LogonUserIdentity
		{
			get { return base.LogonUserIdentity; }
		}
	}
}