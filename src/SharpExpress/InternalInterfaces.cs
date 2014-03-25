using System;

namespace SharpExpress
{
	internal interface IHttpChannel
	{
		HttpStream Receive();

		void Send(byte[] packet);
	}

	internal interface IHttpListener
	{
		bool IsListening { get; }

		void Start();
		void Stop();
		void Close();

		IAsyncResult BeginClient(AsyncCallback callback, object state);
		object EndClient(IAsyncResult ar);

		void ProcessRequest(object context);
	}
}