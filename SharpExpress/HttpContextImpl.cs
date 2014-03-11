using System.Net;
using System.Web;

namespace SharpExpress
{
	internal sealed class HttpContextImpl : HttpContextBase
	{
		private readonly HttpRequestBase _request;
		private readonly HttpResponseBase _response;

		public HttpContextImpl(HttpListenerRequest request, HttpListenerResponse response)
		{
			_request = new HttpRequestImpl(request);
			_response = new HttpResponseImpl(response);
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}

		public override HttpResponseBase Response
		{
			get { return _response; }
		}
	}
}