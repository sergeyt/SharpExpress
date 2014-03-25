using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Web;

namespace SharpExpress
{
	internal sealed class PipeListener : IHttpListener
	{
		private readonly IHttpHandler _handler;
		private readonly HttpServerSettings _settings;
		private NamedPipeServerStream _server;
		private bool _processing;
		private object _lock = new object();

		public PipeListener(IHttpHandler handler, HttpServerSettings settings)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			if (settings == null) throw new ArgumentNullException("settings");

			_handler = handler;
			_settings = settings;
		}

		public bool IsListening { get; private set; }

		public void Start()
		{
			if (IsListening)
				throw new InvalidOperationException("Server is already started!");

			_server = new NamedPipeServerStream("sharp-express", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
			IsListening = true;
		}

		public void Stop()
		{
			if (_server != null)
			{
				_server.Dispose();
				_server = null;
				IsListening = false;
			}
		}

		public void Close()
		{
		}

		public IAsyncResult Begin(AsyncCallback callback, object state)
		{
			if (_processing)
			{
				Thread.Sleep(10);
				return new WaitResult(state);
			}
			Disconnect();
			return _server.BeginWaitForConnection(callback, state);
		}

		public object End(IAsyncResult ar)
		{
			if (ar is WaitResult) return this;
			_processing = true;
			_server.EndWaitForConnection(ar);
			return this;
		}

		public void ProcessRequest(object context)
		{
			try
			{
				var channel = new PipeChannel(_server);

				channel.ProcessRequest(_handler, _settings);

				Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			_processing = false;
		}

		private void Disconnect()
		{
			lock (_lock)
			{
				if (_server.IsConnected)
				{
					_server.Disconnect();
				}
			}
		}

		private class PipeChannel : IHttpChannel
		{
			private readonly Stream _pipe;
			private readonly HttpStream _httpStream;

			public PipeChannel(Stream pipe)
			{
				_pipe = pipe;
				_httpStream = new HttpStream(pipe.Read);
			}

			public HttpStream Receive()
			{
				return _httpStream;
			}

			public void Send(byte[] packet)
			{
				_pipe.Write(packet, 0, packet.Length);
			}
		}

		private class WaitResult : IAsyncResult
		{
			public WaitResult(object state)
			{
				AsyncState = state;
				AsyncWaitHandle = new ManualResetEvent(true);
			}

			public bool IsCompleted { get { return true; } }
			public WaitHandle AsyncWaitHandle { get; private set; }
			public object AsyncState { get; private set; }
			public bool CompletedSynchronously { get { return true; } }
		}
	}
}
