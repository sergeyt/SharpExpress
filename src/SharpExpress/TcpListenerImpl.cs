using System;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Implements <see cref="IHttpListener"/> using <see cref="TcpListener"/>.
	/// </summary>
	internal sealed class TcpListenerImpl : IHttpListener
	{
		private readonly IHttpHandler _handler;
		private readonly HttpServerSettings _settings;
		private readonly TcpListener _listener;

		public TcpListenerImpl(IHttpHandler handler, HttpServerSettings settings)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			if (settings == null) throw new ArgumentNullException("settings");

			_handler = handler;
			_settings = settings;

			// TODO endpoint setting
			_listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, settings.Port));
		}

		public bool IsListening { get; private set; }

		public void Start()
		{
			if (IsListening)
				throw new InvalidOperationException("Server is already started!");

			_listener.Start();
			IsListening = true;
		}

		public void Stop()
		{
			_listener.Stop();
			IsListening = false;
		}

		public void Close()
		{
		}

		public IAsyncResult BeginClient(AsyncCallback callback, object state)
		{
			return _listener.BeginAcceptSocket(callback, state);
		}

		public object EndClient(IAsyncResult ar)
		{
			return _listener.EndAcceptSocket(ar);
		}

		public void ProcessRequest(object context)
		{
			try
			{
				var socket = (Socket)context;
				var channel = new SocketChannel(socket);
				channel.ProcessRequest(_handler, _settings);
				socket.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private class SocketChannel : IHttpChannel
		{
			private readonly Socket _socket;
			private readonly HttpStream _stream;

			public SocketChannel(Socket socket)
			{
				_socket = socket;
				_stream = new HttpStream((buffer, offset, count) => socket.Receive(buffer, offset, count, SocketFlags.None));
			}

			public HttpStream Receive()
			{
				return _stream;
			}

			public void Send(byte[] packet)
			{
				_socket.Send(packet);
			}
		}
	}
}
