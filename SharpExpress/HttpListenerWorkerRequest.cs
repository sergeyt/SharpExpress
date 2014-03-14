using System;
using System.Net;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Implements <see cref="HttpWorkerRequest"/> using <see cref="HttpListenerContext"/>.
	/// </summary>
	internal sealed class HttpListenerWorkerRequest : HttpWorkerRequest
	{
		private readonly HttpListenerContext _context;
		private readonly string _virtualDir;
		private readonly string _physicalDir;

		public HttpListenerWorkerRequest(HttpListenerContext context, string vdir, string pdir)
		{
			if (context == null) throw new ArgumentNullException("context");

			_context = context;
			_virtualDir = vdir;
			_physicalDir = pdir;
		}

		public override string GetUriPath()
		{
			return _context.Request.Url.LocalPath;
		}

		public override string GetQueryString()
		{
			return _context.Request.Url.Query;
		}

		public override string GetRawUrl()
		{
			return _context.Request.RawUrl;
		}

		public override string GetHttpVerbName()
		{
			return _context.Request.HttpMethod;
		}

		public override string GetHttpVersion()
		{
			return string.Format("HTTP/{0}.{1}",
				_context.Request.ProtocolVersion.Major,
				_context.Request.ProtocolVersion.Minor);
		}

		public override string GetRemoteAddress()
		{
			return _context.Request.RemoteEndPoint.Address.ToString();
		}

		public override int GetRemotePort()
		{
			return _context.Request.RemoteEndPoint.Port;
		}

		public override string GetLocalAddress()
		{
			return _context.Request.LocalEndPoint.Address.ToString();
		}

		public override int GetLocalPort()
		{
			return _context.Request.LocalEndPoint.Port;
		}

		public override void SendStatus(int statusCode, string statusDescription)
		{
			_context.Response.StatusCode = statusCode;
			_context.Response.StatusDescription = statusDescription;
		}

		public override void SendKnownResponseHeader(int index, string value)
		{
			var name = GetKnownResponseHeaderName(index);
			if (string.Equals(name, "Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
				return;

			_context.Response.Headers[name] = value;
		}

		public override void SendUnknownResponseHeader(string name, string value)
		{
			_context.Response.Headers[name] = value;
		}

		public override void SendResponseFromMemory(byte[] data, int length)
		{
			_context.Response.OutputStream.Write(data, 0, length);
		}

		public override void SendResponseFromFile(string filename, long offset, long length)
		{
			throw new NotImplementedException();
		}

		public override void SendResponseFromFile(IntPtr handle, long offset, long length)
		{
			throw new NotImplementedException();
		}

		public override void FlushResponse(bool finalFlush)
		{
		}

		public override void EndOfRequest()
		{
			_context.Response.OutputStream.Close();
			_context.Response.Close();
		}

		public override int ReadEntityBody(byte[] buffer, int size)
		{
			return _context.Request.InputStream.Read(buffer, 0, size);
		}

		public override string GetAppPath()
		{
			return _virtualDir;
		}

		public override string GetAppPathTranslated()
		{
			return _physicalDir;
		}
	}
}
