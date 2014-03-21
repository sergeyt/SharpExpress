using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace SharpExpress
{
	public interface IMessage
	{
		Stream Body { get; }
		Encoding Encoding { get; set; }
		NameValueCollection Headers { get; }
		HttpCookieCollection Cookies { get; }
	}

	public interface IRequest : IMessage
	{
		string HttpMethod { get; }
		Uri Url { get; }
		NameValueCollection QueryString { get; }
	}

	public interface IResponse : IMessage
	{
		string Status { get; set; }
		int StatusCode { get; set; }
		int SubStatusCode { get; set; }
		string StatusDescription { get; set; }

		string ContentType { get; set; }

		TextWriter Output { get; }

		void Redirect(string url);
	}

	public interface IHttpContext
	{
		IRequest Request { get; }
		IResponse Response { get; }
	}
}
