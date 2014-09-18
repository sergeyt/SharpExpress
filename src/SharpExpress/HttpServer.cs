using System;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Micro http server.
	/// </summary>
	public sealed class HttpServer : MarshalByRefObject, IDisposable
	{
		private readonly ServerImpl _server;

		public HttpServer(IHttpHandler app, HttpServerSettings settings)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (settings == null) throw new ArgumentNullException("settings");

			// init asp.net host
			if (settings.AspNetHost)
			{
				var appHost = new AppHost(settings);
				appHost.Init();
			}

			IHttpListener listener;
			
			switch (settings.Mode)
			{
				case HttpServerMode.TcpListener:
					listener = new TcpListenerImpl(app, settings);
					break;
				case HttpServerMode.HttpListener:
					listener = new HttpListenerImpl(app, settings);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			_server = new ServerImpl(listener, settings.WorkerCount);
		}

		public void Dispose()
		{
			_server.Stop();
		}
	}
}
