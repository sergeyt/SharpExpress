using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace SharpExpress
{
	/// <summary>
	/// Common implementation of <see cref="HttpResponseBase"/>.
	/// </summary>
	internal sealed class HttpResponseImpl : HttpResponseBase, IHttpResponse
	{
		private readonly IHttpChannel _channel;
		private TextWriter _output;
		private MemoryStream _body;
		private readonly NameValueCollection _headers = new NameValueCollection(StringComparer.OrdinalIgnoreCase);

		public HttpResponseImpl(IHttpChannel channel)
		{
			if (channel == null) throw new ArgumentNullException("channel");

			_channel = channel;
			_body = new MemoryStream();
			_output = new StreamWriter(_body);
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
			_body.Write(buffer, 0, buffer.Length);
		}

		public override void Clear()
		{
			ClearContent();
			ClearHeaders();
		}

		public override void ClearContent()
		{
			_body = new MemoryStream();
			_output = new StreamWriter(_body);
		}

		public override void ClearHeaders()
		{
			_headers.Clear();
		}

		public override void Close()
		{
		}

		public override void DisableKernelCache()
		{
		}

		public override void End()
		{
			Send();
		}

		public override void Flush()
		{
			_output.Flush();
			_body.Flush();
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

		public override string ContentType
		{
			get { return _headers.Get(HttpRequestHeader.ContentType); }
			set { _headers.Set(HttpRequestHeader.ContentType, value); }
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
			get { return _output; }
		}

		public override string RedirectLocation { get; set; }

		public override Stream OutputStream
		{
			get { return _body; }
		}

		public override string Status { get; set; }
		public override int StatusCode { get; set; }
		public override string StatusDescription { get; set; }
		public override int SubStatusCode { get; set; }

		public override bool SuppressContent { get; set; }
		public override bool TrySkipIisCustomErrors { get; set; }

		private void Send()
		{
			const string eol = "\r\n";
			var contentLength = _body.Length;
				
			var sb = new StringBuilder();
			sb.Append("HTTP/1.1 " + StatusCode + " " + HttpWorkerRequest.GetStatusDescription(StatusCode) + eol);
			sb.Append("Server: Express/" + AssemblyInfo.Version + eol);
			sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + eol);

			if (contentLength >= 0)
			{
				sb.Append("Content-Length: " + contentLength + eol);
			}

			foreach (string key in _headers)
			{
				var val = _headers[key];
				sb.Append(key + ": " + val + eol);
			}

			sb.Append(eol);

			var header = Encoding.UTF8.GetBytes(sb.ToString());

			_output.Flush();
			_body.Flush();
			var body = _body.ToArray();
				
			try
			{
				_channel.Send(header.Concat(body).ToArray());
			}
			catch (Exception)
			{
			}
		}
	}
}