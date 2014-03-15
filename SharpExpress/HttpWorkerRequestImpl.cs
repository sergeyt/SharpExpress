using System;
using System.Net;
using System.Web;
using System.Web.Hosting;

namespace SharpExpress
{
	/// <summary>
	/// Implements <see cref="HttpWorkerRequest"/> using <see cref="HttpListenerContext"/>.
	/// </summary>
	internal sealed class HttpWorkerRequestImpl : SimpleWorkerRequest
	{
		private readonly HttpListenerContext _listenerContext;
		private readonly HttpContextBase _context;

		private static string TrimQuery(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			return s.StartsWith("?") ? s.Substring(1) : s;
		}

		public HttpWorkerRequestImpl(HttpListenerContext context, HttpServerSettings settings)
			: base(
				context.Request.Url.LocalPath,
				TrimQuery(context.Request.Url.Query),
				null)
		{
			_listenerContext = context;
			_context = new HttpContextImpl(context, settings);
		}

		public HttpWorkerRequestImpl(HttpContextBase context, HttpServerSettings settings)
			: base(
				context.Request.Url.LocalPath,
				TrimQuery(context.Request.Url.Query),
				null)
		{
			_context = context;
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
			return base.GetHttpVersion();
		}

		private IPEndPoint RemoteEndPoint
		{
			get { return _listenerContext.IfNotNull(x => x.Request.RemoteEndPoint); }
		}

		private IPEndPoint LocalEndPoint
		{
			get { return _listenerContext.IfNotNull(x => x.Request.LocalEndPoint); }
		}

		public override string GetRemoteAddress()
		{
			if (RemoteEndPoint != null)
			{
				return RemoteEndPoint.Address.ToString();
			}
			return base.GetRemoteAddress();
		}

		public override int GetRemotePort()
		{
			if (RemoteEndPoint != null)
			{
				return RemoteEndPoint.Port;
			}
			return base.GetRemotePort();
		}

		public override string GetLocalAddress()
		{
			if (LocalEndPoint != null)
			{
				return LocalEndPoint.Address.ToString();
			}
			return base.GetLocalAddress();
		}

		public override int GetLocalPort()
		{
			if (LocalEndPoint != null)
			{
				return LocalEndPoint.Port;
			}
			return base.GetLocalPort();
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
	}
}
