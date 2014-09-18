using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
			return _listener.BeginAcceptTcpClient(callback, state);
		}

		public object EndClient(IAsyncResult ar)
		{
			return _listener.EndAcceptTcpClient(ar);
		}

		public void ProcessRequest(object context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			var client = context as TcpClient;
			if (client == null)
				throw new ArgumentException("Bad context. Expected TcpClient.", "context");

			var channel = new HttpChannelImpl(client);
			channel.ProcessRequest(_handler, _settings);
			client.Close();
		}

		private class HttpChannelImpl : IHttpChannel
		{
			private readonly TcpClient _client;
			private readonly StringBuilder _sb = new StringBuilder();
			
			public HttpChannelImpl(TcpClient client)
			{
				if (client == null) throw new ArgumentNullException("client");

				_client = client;

				var headers = new List<string>();
				while (true)
				{
					var header = ReadLine();
					if (header.Length == 0) break;
					headers.Add(header);
					// Debug.Print(header);
				}

				Method = "GET";
				Path = "/";

				if (headers.Count > 0)
				{
					var parts = headers[0].Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
					Method = parts[0];
					Path = parts[1];
				}

				Headers = new NameValueCollection(StringComparer.OrdinalIgnoreCase);

				Headers.AddRange(
					from l in headers.Skip(1)
					let i = l.IndexOf(':')
					where i >= 0
					let key = l.Substring(0, i).Trim()
					let val = l.Substring(i + 1).Trim()
					select new KeyValuePair<string, string>(key, val)
					);

				var len = Headers.GetContentLength();

				Body = len > 0
					? (Stream) new RequestStream(_client.GetStream(), len)
					: new MemoryStream(new byte[0], false);
			}

			public string Path { get; private set; }
			public string Method { get; private set; }
			public NameValueCollection Headers { get; private set; }
			public Stream Body { get; private set; }

			public void Send(byte[] response)
			{
				var stream = _client.GetStream();
				stream.Write(response, 0, response.Length);
				stream.Flush();
			}

			private string ReadLine()
			{
				var input = _client.GetStream();
				_sb.Length = 0;

				while (true)
				{
					var c = input.ReadByte();
					if (c == '\r') continue;
					if (c < 0 || c == '\n') break;
					_sb.Append(Convert.ToChar(c));
				}

				return _sb.ToString();
			}
		}

		private sealed class RequestStream : Stream
		{
			private readonly Stream _input;
			private readonly int _length;
			private int _readCount;

			public RequestStream(Stream input, int length)
			{
				_input = input;
				_length = length;
			}

			public override void Flush()
			{
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (_readCount >= _length) return 0;
				var len = _input.Read(buffer, offset, count);
				_readCount += len;
				return len;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			public override bool CanRead { get { return true; } }
			public override bool CanSeek { get { return false; } }
			public override bool CanWrite { get { return false; } }

			public override long Length
			{
				get { return _length; }
			}

			public override long Position { get; set; }
		}
	}
}
