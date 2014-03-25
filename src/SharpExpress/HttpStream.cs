using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpExpress
{
	internal delegate int ReadFunc(byte[] buffer, int offset, int count);

	internal sealed class HttpStream : Stream
	{
		private const int MaxHeaderSize = 32*1024;
		private readonly ReadFunc _read;
		private MemoryStream _firstPacket;
		
		public HttpStream(ReadFunc read)
		{
			if (read == null) throw new ArgumentNullException("read");

			_read = read;
		}

		public string[] ReadHeader()
		{
			var buf = new byte[MaxHeaderSize];
			var size = _read(buf, 0, MaxHeaderSize);
			if (size < buf.Length)
			{
				var temp = new byte[size];
				Buffer.BlockCopy(buf, 0, temp, 0, size);
				buf = temp;
			}

			var encoding = Encoding.UTF8;

			var headerSize = 0;
			var header = new List<string>();
			foreach (var range in buf.GetLineRanges())
			{
				if (range.Length == 0)
				{
					headerSize += 2;
					break;
				}
				var line = encoding.GetString(buf, range.Start, range.Length);
				header.Add(line);
				headerSize += range.Length + 2;
			}

			if (headerSize < buf.Length)
			{
				var packet = new byte[buf.Length - headerSize];
				Buffer.BlockCopy(buf, headerSize, packet, 0, packet.Length);
				_firstPacket = new MemoryStream(packet);
			}

			return header.ToArray();
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
			var len = 0;

			if (_firstPacket != null)
			{
				len = _firstPacket.Read(buffer, offset, count);

				if (_firstPacket.Position >= _firstPacket.Length)
				{
					_firstPacket = null;
				}
			}

			len += _read(buffer, offset + len, count);

			return len;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}
}