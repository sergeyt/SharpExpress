using System;
using System.Collections.Generic;
using System.Threading;

namespace SharpExpress
{
	/// <summary>
	/// Common server implementation.
	/// </summary>
	internal sealed class ServerImpl
	{
		private readonly IHttpListener _listener;
		private readonly Thread _listenerThread;
		private readonly Thread[] _workers;
		private readonly ManualResetEvent _stop = new ManualResetEvent(false);
		private readonly ManualResetEvent _ready = new ManualResetEvent(false);
		private readonly Queue<object> _queue = new Queue<object>();

		public ServerImpl(IHttpListener listener, int workerCount)
		{
			if (listener == null) throw new ArgumentNullException("listener");

			_listener = listener;
			_listener.Start();

			_listenerThread = new Thread(Listen);
			_listenerThread.Start();

			_workers = new Thread[workerCount];
			for (int i = 0; i < _workers.Length; i++)
			{
				_workers[i] = new Thread(Worker);
				_workers[i].Start();
			}
		}

		public void Stop()
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
				var context = _listener.BeginClient(OnContextReady, _listener);
				if (0 == WaitHandle.WaitAny(new[] {_stop, context.AsyncWaitHandle}))
					return;
			}
		}

		private void OnContextReady(IAsyncResult ar)
		{
			try
			{
				// If not listening return immediately as this method is called on last time after Close()
				if (!_listener.IsListening)
					return;

				var context = _listener.EndClient(ar);

				lock (_queue)
				{
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
				object context;
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

				_listener.ProcessRequest(context);
			}
		}
	}
}
