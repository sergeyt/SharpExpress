using System;
using System.IO.Pipes;
using System.Text;

namespace PipeClient
{
	class Program
	{
		static void Main()
		{
			try
			{
				var pipe = new NamedPipeClientStream(".", "sharp-express", PipeDirection.InOut);
				pipe.Connect();

				var encoding = Encoding.UTF8;
				var sb = new StringBuilder();
				sb.Append("GET / HTTP/1.1\r\n");
				sb.Append("Header1: Hi!\r\n");
				sb.Append("\r\n");

				var bytes = encoding.GetBytes(sb.ToString());
				pipe.Write(bytes, 0, bytes.Length);
				pipe.Flush();

				pipe.WaitForPipeDrain();

				var buf = new byte[64*1024];
				var size = pipe.Read(buf, 0, buf.Length);
				var message = encoding.GetString(buf, 0, size);
				Console.WriteLine(message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
