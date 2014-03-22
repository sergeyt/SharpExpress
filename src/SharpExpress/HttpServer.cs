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
			var appHost = new AppHost(settings);

			_server = new ServerImpl(new HttpListenerImpl(app, settings), settings.WorkerCount);
		}

		public void Dispose()
		{
			_server.Stop();
		}
	}
}
