using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Micro http server.
	/// </summary>
	public sealed class HttpServer : IDisposable
	{
		private readonly HttpListener _listener = new HttpListener();
		private readonly Thread _listenerThread;
		private readonly Thread[] _workers = new Thread[8];
		private readonly ManualResetEvent _stop = new ManualResetEvent(false);
		private readonly ManualResetEvent _ready = new ManualResetEvent(false);
		private readonly Queue<HttpListenerContext> _queue = new Queue<HttpListenerContext>();

		// TODO more settings
		public HttpServer(int port)
		{
			App = new ExpressApplication();

			_listener.Prefixes.Add(string.Format(@"http://+:{0}/", port));
			_listenerThread = new Thread(Consume);
			
			_listener.Start();
			_listenerThread.Start();

			for (int i = 0; i < _workers.Length; i++)
			{
				_workers[i] = new Thread(Worker);
				_workers[i].Start();
			}
		}

		public ExpressApplication App { get; private set; }

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

		private void Consume()
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

			try
			{
				if (!App.Process(ctx))
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
			res.Close();
		}
	}
}
