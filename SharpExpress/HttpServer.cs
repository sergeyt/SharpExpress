using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SharpExpress
{
	/// <summary>
	/// Micro http server.
	/// </summary>
	public sealed class HttpServer : IDisposable
	{
		private readonly HttpListener _listener = new HttpListener();
		private readonly Thread _listenerThread;
		private readonly Thread[] _workers;
		private readonly ManualResetEvent _stop = new ManualResetEvent(false);
		private readonly ManualResetEvent _ready = new ManualResetEvent(false);
		private readonly Queue<HttpListenerContext> _queue = new Queue<HttpListenerContext>();
		private readonly ExpressApplication _app;

		// TODO more settings
		public HttpServer(ExpressApplication app, int port, int workerCount)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (workerCount <= 0) throw new ArgumentOutOfRangeException("workerCount");

			_app = app;

			_listener.Prefixes.Add(string.Format(@"http://+:{0}/", port));
			_listenerThread = new Thread(Listen);
			
			_listener.Start();
			_listenerThread.Start();

			_workers = new Thread[workerCount];
			for (int i = 0; i < _workers.Length; i++)
			{
				_workers[i] = new Thread(Worker);
				_workers[i].Start();
			}
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
			var ctx = new HttpContextImpl(context.Request, context.Response);
			var res = ctx.Response;

			using (res.OutputStream)
			{
				try
				{
					if (!_app.Process(ctx))
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
	}
}
