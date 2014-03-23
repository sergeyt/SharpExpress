using System;
using System.Net.Sockets;

namespace SharpExpress
{
	internal static class SocketExtensions
	{
		public static int WaitForRequestBytes(this Socket socket)
		{
			int availBytes = 0;

			try
			{
				if (socket.Available == 0)
				{
					socket.Poll(100000, SelectMode.SelectRead);

					if (socket.Available == 0 && socket.Connected)
					{
						socket.Poll(30000000, SelectMode.SelectRead);
					}
				}

				availBytes = socket.Available;
			}
			// ReSharper disable EmptyGeneralCatchClause
			catch
			// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return availBytes;
		}

		public static byte[] ReadRequestBytes(this Socket socket, int maxBytes)
		{
			try
			{
				var availBytes = socket.WaitForRequestBytes();
				if (availBytes == 0)
				{
					return null;
				}

				if (availBytes > maxBytes)
				{
					availBytes = maxBytes;
				}

				int received = 0;
				var buffer = new byte[availBytes];
				if (availBytes > 0)
				{
					received = socket.Receive(buffer, 0, availBytes, SocketFlags.None);
				}

				if (received < availBytes)
				{
					var bytes = new byte[received];
					if (received > 0)
					{
						Buffer.BlockCopy(buffer, 0, bytes, 0, received);
					}
					buffer = bytes;
				}

				return buffer;
			}
			catch
			{
				return null;
			}
		}
	}
}
