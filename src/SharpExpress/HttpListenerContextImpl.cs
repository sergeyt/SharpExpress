using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace SharpExpress
{
	/// <summary>
	/// <see cref="HttpContextBase"/> implementation over <see cref="HttpListenerContext"/>.
	/// </summary>
	internal sealed class HttpListenerContextImpl : HttpContextBase, IHttpContext
	{
		private readonly HttpListenerContext _context;
		private readonly HttpServerSettings _settings;
		private readonly RequestImpl _request;
		private readonly ResponseImpl _response;
		private HttpContext _httpContext;

		public HttpListenerContextImpl(HttpListenerContext context, HttpServerSettings settings)
		{
			_context = context;
			_settings = settings;
			_request = new RequestImpl(context.Request);
			_response = new ResponseImpl(context.Response);
		}

		public HttpContext HttpContext
		{
			get { return _httpContext ?? (_httpContext = new HttpContext(new HttpWorkerRequestImpl(_context, _settings))); }
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}

		public override HttpResponseBase Response
		{
			get { return _response; }
		}

		IHttpRequest IHttpContext.Request
		{
			get { return _request; }
		}

		IHttpResponse IHttpContext.Response
		{
			get { return _response; }
		}

		/// <summary>
		/// Implements <see cref="HttpRequestBase"/> wrapping <see cref="HttpListenerRequest"/>.
		/// </summary>
		private sealed class RequestImpl : HttpRequestBase, IHttpRequest
		{
			private readonly HttpListenerRequest _request;

			public RequestImpl(HttpListenerRequest request)
			{
				_request = request;
			}

			public override byte[] BinaryRead(int count)
			{
				var buf = new byte[count];
				InputStream.Read(buf, 0, count);
				return buf;
			}

			public override void ValidateInput()
			{
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

			public override Stream Filter { get; set; }

			public override string Path
			{
				get { return _request.Url.LocalPath; }
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
				get { return _request.Url.LocalPath.TrimStart('/'); }
			}

			public override string RawUrl
			{
				get { return _request.Url.ToString(); }
			}

			public override string RequestType { get; set; }

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
		}

		/// <summary>
		/// Implements <see cref="HttpResponseBase"/> wrapping <see cref="HttpListenerResponse"/>.
		/// </summary>
		private sealed class ResponseImpl : HttpResponseBase, IHttpResponse
		{
			private readonly HttpListenerResponse _response;
			private TextWriter _output;

			public ResponseImpl(HttpListenerResponse response)
			{
				_response = response;
			}

			public override void AddCacheItemDependency(string cacheKey)
			{
			}

			public override void AddCacheItemDependencies(ArrayList cacheKeys)
			{
			}

			public override void AddCacheItemDependencies(string[] cacheKeys)
			{
			}

			public override void AddCacheDependency(params CacheDependency[] dependencies)
			{
			}

			public override void AddFileDependency(string filename)
			{
			}

			public override void AddFileDependencies(ArrayList filenames)
			{
			}

			public override void AddFileDependencies(string[] filenames)
			{
			}

			public override void AddHeader(string name, string value)
			{
				_response.AddHeader(name, value);
			}

			public override void AppendCookie(HttpCookie cookie)
			{
				_response.AppendCookie(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
			}

			public override void AppendHeader(string name, string value)
			{
				_response.AppendHeader(name, value);
			}

			public override void AppendToLog(string param)
			{
			}

			public override string ApplyAppPathModifier(string virtualPath)
			{
				return virtualPath;
			}

			public override void BinaryWrite(byte[] buffer)
			{
				OutputStream.Write(buffer, 0, buffer.Length);
			}

			public override void Clear()
			{
				ClearContent();
				ClearHeaders();
			}

			public override void ClearContent()
			{
				base.ClearContent();
			}

			public override void ClearHeaders()
			{
				base.ClearHeaders();
			}

			public override void Close()
			{
				_output = null;
				_response.Close();
			}

			public override void DisableKernelCache()
			{
			}

			public override void End()
			{
			}

			public override void Flush()
			{
				if (_output != null)
				{
					_output.Flush();
				}

				OutputStream.Flush();
			}

			public override void Pics(string value)
			{
			}

			public override void Redirect(string url)
			{
				base.Redirect(url);
			}

			public override void Redirect(string url, bool endResponse)
			{
				base.Redirect(url, endResponse);
			}

			public override void RemoveOutputCacheItem(string path)
			{
				base.RemoveOutputCacheItem(path);
			}

			public override void SetCookie(HttpCookie cookie)
			{
				base.SetCookie(cookie);
			}

			public override void TransmitFile(string filename)
			{
				base.TransmitFile(filename);
			}

			public override void TransmitFile(string filename, long offset, long length)
			{
				base.TransmitFile(filename, offset, length);
			}

			public override void Write(char ch)
			{
				Output.Write(ch);
			}

			public override void Write(char[] buffer, int index, int count)
			{
				Output.Write(buffer, index, count);
			}

			public override void Write(object obj)
			{
				Output.Write(obj);
			}

			public override void Write(string s)
			{
				Output.Write(s);
			}

			public override void WriteFile(string filename)
			{
				base.WriteFile(filename);
			}

			public override void WriteFile(string filename, bool readIntoMemory)
			{
				base.WriteFile(filename, readIntoMemory);
			}

			public override void WriteFile(string filename, long offset, long size)
			{
				base.WriteFile(filename, offset, size);
			}

			public override void WriteFile(IntPtr fileHandle, long offset, long size)
			{
				base.WriteFile(fileHandle, offset, size);
			}

			public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
			{
				base.WriteSubstitution(callback);
			}

			public override bool Buffer { get; set; }

			public override bool BufferOutput { get; set; }

			public override HttpCachePolicyBase Cache
			{
				get { return null; }
			}

			public override string CacheControl { get; set; }
			public override string Charset { get; set; }

			public override Encoding ContentEncoding
			{
				get { return _response.ContentEncoding; }
				set { _response.ContentEncoding = value; }
			}

			public override string ContentType
			{
				get { return _response.ContentType; }
				set { _response.ContentType = value; }
			}

			public override HttpCookieCollection Cookies
			{
				get { return base.Cookies; }
			}

			public override int Expires { get; set; }
			public override DateTime ExpiresAbsolute { get; set; }
			public override Stream Filter { get; set; }

			public override NameValueCollection Headers
			{
				get { return _response.Headers; }
			}

			public Stream Body
			{
				get { return OutputStream; }
			}

			public Encoding Encoding
			{
				get { return ContentEncoding; }
				set { ContentEncoding = value; }
			}

			public override Encoding HeaderEncoding { get; set; }

			public override bool IsClientConnected
			{
				get { return base.IsClientConnected; }
			}

			public override bool IsRequestBeingRedirected
			{
				get { return base.IsRequestBeingRedirected; }
			}

			public override TextWriter Output
			{
				get { return _output ?? (_output = new StreamWriter(OutputStream)); }
			}

			public override string RedirectLocation { get; set; }

			public override Stream OutputStream
			{
				get { return _response.OutputStream; }
			}

			public override string Status
			{
				get;
				set;
			}

			public override int StatusCode
			{
				get { return _response.StatusCode; }
				set { _response.StatusCode = value; }
			}

			public override string StatusDescription
			{
				get { return _response.StatusDescription; }
				set { _response.StatusDescription = value; }
			}

			public override int SubStatusCode { get; set; }

			public override bool SuppressContent { get; set; }

			public override bool TrySkipIisCustomErrors { get; set; }
		}
	}
}