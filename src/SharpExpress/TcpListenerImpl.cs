using System;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace SharpExpress
{
	internal sealed class TcpListenerImpl : IListener
	{
		private readonly IHttpHandler _app;
		private readonly HttpServerSettings _settings;
		private readonly TcpListener _listener;

		public TcpListenerImpl(IHttpHandler app, HttpServerSettings settings)
		{
			_app = app;
			_settings = settings;
			_listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, settings.Port));
		}

		public bool IsListening { get; private set; }

		public void Start()
		{
			_listener.Start();
		}

		public void Stop()
		{
			_listener.Stop();
		}

		public void Close()
		{
		}

		public IAsyncResult Begin(AsyncCallback callback, object state)
		{
			return _listener.BeginAcceptSocket(callback, _listener);
		}

		public object End(IAsyncResult ar)
		{
			return _listener.EndAcceptSocket(ar);
		}

		public void ProcessRequest(object context)
		{
			try
			{
				ProcessRequestInternal((Socket)context);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void ProcessRequestInternal(Socket context)
		{
			var app = _app as ExpressApplication;
			if (app != null)
			{
				ProcessExpressRequest(context, app);
				return;
			}

			ProcessWorkerRequest(context);
		}

		private void ProcessExpressRequest(Socket socket, ExpressApplication app)
		{
			var context = new TcpContextImpl(socket, _settings);
			var res = context.Response;

			using (res.OutputStream)
			{
				try
				{
					if (!app.Process(context))
					{
						res.StatusCode = (int)HttpStatusCode.NotFound;
						res.StatusDescription = "Not found";
						res.ContentType = "text/plain";
						res.Write("Resource not found!");
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);

					res.StatusCode = (int)HttpStatusCode.InternalServerError;
					res.ContentType = "text/plain";
					res.Write(e.ToString());
				}

				res.Flush();
			}
		}

		private void ProcessWorkerRequest(Socket socket)
		{
			var context = new TcpContextImpl(socket, _settings);

			var res = context.Response;
			using (res.OutputStream)
			{
				try
				{
					var workerRequest = new HttpWorkerRequestImpl(context, _settings);
					_app.ProcessRequest(new HttpContext(workerRequest));
					workerRequest.EndOfRequest();
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);
					res.StatusCode = (int)HttpStatusCode.InternalServerError;
					res.ContentType = "text/plain";
					res.OutputStream.Write(e.ToString());
				}
			}
		}
	}
}
