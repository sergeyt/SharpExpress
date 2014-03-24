using System;

namespace SharpExpress
{
	public sealed class HttpServerSettings
	{
		public HttpServerSettings()
		{
			WorkerCount = 4;
			VirtualDir = "/";
			PhisycalDir = Environment.CurrentDirectory;
		}

		public int Port { get; set; }
		public int WorkerCount { get; set; }
		public string VirtualDir { get; set; }
		public string PhisycalDir { get; set; }
		public bool AspNetHost { get; set; }
	}
}