using System;
using System.Net;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Implements <see cref="HttpWorkerRequest"/> using <see cref="HttpListenerContext"/>.
	/// </summary>
	internal sealed class HttpWorkerRequestImpl : HttpWorkerRequest
	{
		private readonly HttpListenerContext _listenerContext;
		private readonly HttpContextBase _context;
		private readonly string _virtualDir;
		private readonly string _physicalDir;

		public HttpWorkerRequestImpl(HttpListenerContext context, HttpServerSettings settings)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (settings == null) throw new ArgumentNullException("settings");

			_listenerContext = context;
			_context = new HttpContextImpl(context, settings);
			_virtualDir = settings.VirtualDir;
			_physicalDir = settings.PhisycalDir;
		}

		public HttpWorkerRequestImpl(HttpContextBase context, HttpServerSettings settings)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (settings == null) throw new ArgumentNullException("settings");

			_context = context;
			_virtualDir = settings.VirtualDir;
			_physicalDir = settings.PhisycalDir;
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
			if (_listenerContext != null)
			{
				return string.Format("HTTP/{0}.{1}",
					_listenerContext.Request.ProtocolVersion.Major,
					_listenerContext.Request.ProtocolVersion.Minor);
			}
			return string.Format("HTTP/{0}.{1}", 1, 1);
		}

		public override string GetRemoteAddress()
		{
			return _context.Request.UserHostAddress;
		}

		public override int GetRemotePort()
		{
			if (_listenerContext != null)
			{
				return _listenerContext.Request.RemoteEndPoint.Port;
			}
			return _context.Request.Url.Port;
		}

		public override string GetLocalAddress()
		{
			if (_listenerContext != null)
			{
				return _listenerContext.Request.LocalEndPoint.Address.ToString();
			}
			// TODO fix
			return "";
		}

		public override int GetLocalPort()
		{
			if (_listenerContext != null)
			{
				return _listenerContext.Request.LocalEndPoint.Port;
			}
			return _context.Request.Url.Port;
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
