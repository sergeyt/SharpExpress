using System;
using System.Net;
using System.Web;

namespace SharpExpress
{
	internal sealed class HttpListenerImpl : IHttpListener
	{
		private readonly IHttpHandler _app;
		private readonly HttpServerSettings _settings;
		private readonly HttpListener _listener;

		public HttpListenerImpl(IHttpHandler app, HttpServerSettings settings)
		{
			_app = app;
			_settings = settings;
			_listener = new HttpListener();
			_listener.Prefixes.Add(string.Format(@"http://+:{0}/", settings.Port));
		}

		public bool IsListening { get { return _listener.IsListening; } }

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
			_listener.Close();
		}

		public IAsyncResult BeginClient(AsyncCallback callback, object state)
		{
			return _listener.BeginGetContext(callback, state);
		}

		public object EndClient(IAsyncResult ar)
		{
			return _listener.EndGetContext(ar);
		}

		public void ProcessRequest(object context)
		{
			try
			{
				ProcessRequestInternal((HttpListenerContext)context);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void ProcessRequestInternal(HttpListenerContext context)
		{
			var app = _app as ExpressApplication;
			if (app != null)
			{
				ProcessExpressRequest(context, app);
				return;
			}

			ProcessWorkerRequest(context);
		}

		private void ProcessExpressRequest(HttpListenerContext context, ExpressApplication app)
		{
			var wrapper = new HttpListenerContextImpl(context, _settings);
			var res = wrapper.Response;

			using (res.OutputStream)
			{
				try
				{
					if (!app.Process(wrapper))
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
				res.End();
			}
		}

		private void ProcessWorkerRequest(HttpListenerContext context)
		{
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
