using System.Net;
using System.Web;

namespace SharpExpress
{
	internal sealed class HttpContextImpl : HttpContextBase, IHttpContext
	{
		private readonly HttpListenerContext _context;
		private readonly HttpServerSettings _settings;
		private readonly HttpRequestImpl _request;
		private readonly HttpResponseImpl _response;
		private HttpContext _httpContext;

		public HttpContextImpl(HttpListenerContext context, HttpServerSettings settings)
		{
			_context = context;
			_settings = settings;
			_request = new HttpRequestImpl(context.Request);
			_response = new HttpResponseImpl(context.Response);
		}

		public HttpContext HttpContext
		{
			get { return _httpContext ?? (_httpContext = new HttpContext(new HttpWorkerRequestImpl(_context, _settings))); }
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}

		public override HttpResponseBase Response
		{
			get { return _response; }
		}

		IRequest IHttpContext.Request
		{
			get { return _request; }
		}

		IResponse IHttpContext.Response
		{
			get { return _response; }
		}
	}
}