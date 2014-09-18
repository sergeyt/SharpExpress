using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Common <see cref="HttpContextBase"/> implementation.
	/// </summary>
	internal sealed class HttpContextImpl : HttpContextBase, IHttpContext
	{
		private readonly HttpRequestImpl _request;
		private readonly HttpResponseImpl _response;

		public HttpContextImpl(IHttpChannel channel, HttpServerSettings settings)
		{
			_request = new HttpRequestImpl(this, channel, settings);
			_response = new HttpResponseImpl(channel);
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}

		public override HttpResponseBase Response
		{
			get { return _response; }
		}

		IHttpRequest IHttpContext.Request
		{
			get { return _request; }
		}

		IHttpResponse IHttpContext.Response
		{
			get { return _response; }
		}

		public bool IsAuthenticated
		{
			get
			{
				// TODO
				return true;
			}
		}
	}
}