using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Common interface for HTTP request/response.
	/// </summary>
	public interface IHttpMessage
	{
		Stream Body { get; }
		Encoding Encoding { get; set; }
		NameValueCollection Headers { get; }
		HttpCookieCollection Cookies { get; }
	}

	/// <summary>
	/// HTTP request.
	/// </summary>
	public interface IHttpRequest : IHttpMessage
	{
		string HttpMethod { get; }
		Uri Url { get; }
		NameValueCollection QueryString { get; }
	}

	/// <summary>
	/// HTTP response.
	/// </summary>
	public interface IHttpResponse : IHttpMessage
	{
		string Status { get; set; }
		int StatusCode { get; set; }
		int SubStatusCode { get; set; }
		string StatusDescription { get; set; }

		string ContentType { get; set; }

		TextWriter Output { get; }

		void Redirect(string url);
	}

	/// <summary>
	/// HTTP context.
	/// </summary>
	public interface IHttpContext
	{
		// TODO route data

		IHttpRequest Request { get; }
		IHttpResponse Response { get; }
	}
}
