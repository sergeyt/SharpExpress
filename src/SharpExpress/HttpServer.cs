using System;
using System.Threading;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Micro http server.
	/// </summary>
	public sealed class HttpServer : MarshalByRefObject, IDisposable
	{
		private readonly IHttpListener _listener;
		private readonly Thread _listenerThread;
		private readonly ManualResetEvent _stop = new ManualResetEvent(false);
		private readonly object _lock = new object();
		private bool _stoped;

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

			_listener = listener;
			_listener.Start();

			_listenerThread = new Thread(Listen);
			_listenerThread.Start();
		}

		public void Dispose()
		{
			Stop();
		}

		private void Stop()
		{
			_stop.Set();
			_listenerThread.Join();

			lock (_lock)
			{
				_listener.Stop();
				_listener.Close();
				_stoped = true;
			}
		}

		private void Listen()
		{
			while (_listener.IsListening)
			{
				var context = _listener.BeginClient(OnContextReady, _listener);
				if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
					return;
			}
		}

		private void OnContextReady(IAsyncResult ar)
		{
			try
			{
				object context;

				lock (_lock)
				{
					// If not listening return immediately as this method is called on last time after Close()
					if (!_listener.IsListening || _stoped)
						return;

					context = _listener.EndClient(ar);
				}

				_listener.ProcessRequest(context);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
