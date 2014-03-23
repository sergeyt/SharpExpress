using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace SharpExpress
{
	internal sealed class TcpContextImpl : HttpContextBase, IHttpContext
	{
		private readonly RequestImpl _request;
		private readonly ResponseImpl _response;

		public TcpContextImpl(Socket socket, HttpServerSettings settings)
		{
			_request = new RequestImpl(this, socket);
			_response = new ResponseImpl(socket);
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}

		public override HttpResponseBase Response
		{
			get { return _response; }
		}

		IRequest IHttpContext.Request
		{
			get { return _request; }
		}

		IResponse IHttpContext.Response
		{
			get { return _response; }
		}

		public bool IsAuthenticated
		{
			get
			{
				// TODO
				return true;
			}
		}

		#region class RequestImpl

		private class RequestImpl : HttpRequestBase, IRequest
		{
			private readonly TcpContextImpl _context;
			private readonly string _method;
			private readonly Uri _url;
			private string _protocol;
			private NameValueCollection _queryString;
			private readonly NameValueCollection _headers;
			private readonly Stream _body;
			
			public RequestImpl(TcpContextImpl context, Socket socket)
			{
				_context = context;

				var headerBytes = socket.ReadRequestBytes(32 * 1024);
				var headerLines = headerBytes.ReadLines(Encoding.UTF8).ToArray();
				var firstLine = headerLines[0].Split(' ');

				_method = firstLine[0];
				_url = new Uri(firstLine[1]);
				_protocol = firstLine.Length == 3 ? firstLine[2] : "HTTP/1.0";
				_queryString = _url.Query.ParseQueryString().ToNameValueCollection();
				
				_headers = (
					from l in headerLines.Skip(1)
					let i = l.IndexOf(':')
					where i >= 0
					let key = l.Substring(0, i).Trim()
					let val = l.Substring(i + 1).Trim()
					select new KeyValuePair<string, string>(key, val)
					).ToNameValueCollection();
			}

			#region HttpRequestBase Members

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
				get
				{
					var val = Get(HttpRequestHeader.Accept);
					return (from x in val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
							let s = x.Trim()
							where s.Length > 0
							select s).ToArray();
				}
			}

			public override string ContentType
			{
				get { return Get(HttpRequestHeader.ContentType); }
				set { throw new NotSupportedException(); }
			}

			public override int ContentLength
			{
				get
				{
					var s = Get(HttpRequestHeader.ContentLength);
					int len;
					if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out len))
					{
						return len;
					}
					return -1;
				}
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
				get { return (int)InputStream.Length; }
			}

			public override Uri Url
			{
				get { return _url; }
			}

			public override Uri UrlReferrer
			{
				get
				{
					var s = Get(HttpRequestHeader.Referer);
					Uri result;
					if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out result))
						return result;
					return null;
				}
			}

			public override string UserAgent
			{
				get { return Get(HttpRequestHeader.UserAgent); }
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
				get { return Get(HttpRequestHeader.Host); }
			}

			public override NameValueCollection Headers
			{
				get { return _headers; }
			}

			public Stream Body
			{
				get { return InputStream; }
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

			private string Get(HttpRequestHeader header)
			{
				return _headers.Get(header.ToString());
			}
		}

		#endregion

		#region class ResponseImpl

		private class ResponseImpl : HttpResponseBase, IResponse
		{
			private TextWriter _output;
			private readonly Stream _outputStream;
			private readonly NameValueCollection _headers;

			public ResponseImpl(Socket socket)
			{
				_outputStream = new MemoryStream();
				_headers = new NameValueCollection();
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
				_headers.Set(name, value);
			}

			public override void AppendCookie(HttpCookie cookie)
			{
			}

			public override void AppendHeader(string name, string value)
			{
				_headers.Set(name, value);
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

			public override Encoding ContentEncoding { get; set; }

			public override string ContentType { get; set; }

			public override HttpCookieCollection Cookies
			{
				get { return base.Cookies; }
			}

			public override int Expires { get; set; }
			public override DateTime ExpiresAbsolute { get; set; }
			public override Stream Filter { get; set; }

			public override NameValueCollection Headers
			{
				get { return _headers; }
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
				get { return _outputStream; }
			}

			public override string Status { get; set; }
			public override int StatusCode { get; set; }
			public override string StatusDescription { get; set; }
			public override int SubStatusCode { get; set; }

			public override bool SuppressContent { get; set; }
			public override bool TrySkipIisCustomErrors { get; set; }
		}

		#endregion
	}
}