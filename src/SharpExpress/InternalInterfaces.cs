using System;
using System.Collections.Specialized;
using System.IO;

namespace SharpExpress
{
	internal interface IHttpChannel
	{
		string Path { get; }
		string Method { get; }
		NameValueCollection Headers { get; }
		Stream Body { get; }
		
		void Send(byte[] response);
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
