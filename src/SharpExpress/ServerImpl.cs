using System;
using System.Collections.Generic;
using System.Threading;

namespace SharpExpress
{
	internal interface IListener
	{
		bool IsListening { get; }

		void Start();
		void Stop();
		void Close();

		IAsyncResult Begin(AsyncCallback callback, object state);
		object End(IAsyncResult ar);

		void ProcessRequest(object context);
	}

	/// <summary>
	/// Common server implementation.
	/// </summary>
	internal sealed class ServerImpl
	{
		private readonly IListener _listener;
		private readonly Thread _listenerThread;
		private readonly Thread[] _workers;
		private readonly ManualResetEvent _stop = new ManualResetEvent(false);
		private readonly ManualResetEvent _ready = new ManualResetEvent(false);
		private readonly Queue<object> _queue = new Queue<object>();

		public ServerImpl(IListener listener, int workerCount)
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
				var context = _listener.Begin(OnContextReady, _listener);
				if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
					return;
			}
		}

		private void OnContextReady(IAsyncResult ar)
		{
			try
			{
				var listener = (IListener)ar.AsyncState;

				// If not listening return immediately as this method is called on last time after Close()
				if (!listener.IsListening)
					return;

				var context = listener.End(ar);

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
