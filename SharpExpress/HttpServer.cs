using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web;

namespace SharpExpress
{
	public sealed class HttpServerSettings
	{
		public HttpServerSettings()
		{
			WorkerCount = 4;
			VirtualDir = "/";
			PhisycalDir = Environment.CurrentDirectory;
		}

		public int Port { get; set; }
		public int WorkerCount { get; set; }
		public string VirtualDir { get; set; }
		public string PhisycalDir { get; set; }
	}

	/// <summary>
	/// Micro http server.
	/// </summary>
	public sealed class HttpServer : MarshalByRefObject, IDisposable
	{
		private readonly HttpListener _listener = new HttpListener();
		private readonly Thread _listenerThread;
		private readonly Thread[] _workers;
		private readonly ManualResetEvent _stop = new ManualResetEvent(false);
		private readonly ManualResetEvent _ready = new ManualResetEvent(false);
		private readonly Queue<HttpListenerContext> _queue = new Queue<HttpListenerContext>();
		private readonly IHttpHandler _app;
		private readonly HttpServerSettings _settings;

		// TODO more settings
		public HttpServer(IHttpHandler app, HttpServerSettings settings)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (settings == null) throw new ArgumentNullException("settings");

			_app = app;
			_settings = settings;

			_listener.Prefixes.Add(string.Format(@"http://+:{0}/", settings.Port));
			_listenerThread = new Thread(Listen);
			
			_listener.Start();
			_listenerThread.Start();

			_workers = new Thread[settings.WorkerCount];
			for (int i = 0; i < _workers.Length; i++)
			{
				_workers[i] = new Thread(Worker);
				_workers[i].Start();
			}

			var appHost = new AppHost(settings);
		}

		public void Dispose()
		{
			Stop();
		}

		private void Stop()
		{
			_stop.Set();

			_listenerThread.Join();
			foreach (var worker in _workers)
				worker.Join();

			_listener.Stop();
			_listener.Close();
		}

		private void Listen()
		{
			while (_listener.IsListening)
			{
				var context = _listener.BeginGetContext(ContextReady, _listener);
				if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
					return;
			}
		}

		private void ContextReady(IAsyncResult ar)
		{
			try
			{
				lock (_queue)
				{
					var listener = (HttpListener)ar.AsyncState;
					var context = listener.EndGetContext(ar);
					_queue.Enqueue(context);
					_ready.Set();
				}
			}
			catch
			{
			}
		}

		private void Worker()
		{
			var wait = new WaitHandle[] { _ready, _stop };
			while (0 == WaitHandle.WaitAny(wait))
			{
				HttpListenerContext context;
				lock (_queue)
				{
					if (_queue.Count > 0)
					{
						context = _queue.Dequeue();
					}
					else
					{
						_ready.Reset();
						continue;
					}
				}

				ProcessRequest(context);
			}
		}

		private void ProcessRequest(HttpListenerContext context)
		{
			var app = _app as ExpressApplication;
			if (app != null)
			{
				ProcessExpressRequest(context, app);
				return;
			}


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

		private void ProcessExpressRequest(HttpListenerContext context, ExpressApplication app)
		{
			var wrapper = new HttpContextImpl(context, _settings);
			var res = wrapper.Response;

			using (res.OutputStream)
			{
				try
				{
					if (!app.Process(wrapper))
					{
						res.StatusCode = (int) HttpStatusCode.NotFound;
						res.StatusDescription = "Not found";
						res.ContentType = "text/plain";
						res.Write("Resource not found!");
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);

					res.StatusCode = (int) HttpStatusCode.InternalServerError;
					res.ContentType = "text/plain";
					res.Write(e.ToString());
				}

				res.Flush();
			}
		}
	}
}
