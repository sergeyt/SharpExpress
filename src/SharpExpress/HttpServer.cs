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

			IListener listener;

			// init asp.net host
			if (settings.AspNetHost)
			{
				var appHost = new AppHost(settings);
				appHost.Init();
				// TODO support TCP listener for asp.net hosting
				listener = new HttpListenerImpl(app, settings);
			}
			else
			{
				listener = new TcpListenerImpl(app, settings);
			}

			_server = new ServerImpl(listener, settings.WorkerCount);
		}

		public void Dispose()
		{
			_server.Stop();
		}
	}
}
