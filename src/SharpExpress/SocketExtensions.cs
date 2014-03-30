using System.Net.Sockets;

namespace SharpExpress
{
	internal static class SocketExtensions
	{
		public static int WaitForRequestBytes(this Socket socket)
		{
			try
			{
				if (socket.Available == 0 && socket.Connected)
				{
					socket.Poll(100000, SelectMode.SelectRead);
				}
				return socket.Available;
			}
			// ReSharper disable EmptyGeneralCatchClause
			catch
			// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return 0;
		}

		public static int Read(this Socket socket, byte[] buffer, int offset, int count)
		{
			var availBytes = socket.WaitForRequestBytes();
			if (availBytes == 0)
			{
				return 0;
			}

			try
			{
				return socket.Receive(buffer, offset, count, SocketFlags.None);
			}
			catch
			{
				return 0;
			}
		}
	}
}
