using System;
using System.Net;
using System.Web;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace SharpExpress
{
	public class ExpressApi
	{
		public Action<RequestContext> Text(string text)
		{
			return req =>
			{
				var ctx = req.HttpContext;
				ctx.Response.StatusCode = (int) HttpStatusCode.OK;
				ctx.Response.ContentType = "text/plain";
				ctx.Response.Write(text);
			};
		}

		public Action<RequestContext> Json(object data)
		{
			if (data == null) throw new ArgumentNullException("data");

			return req =>
			{
				var ctx = req.HttpContext;
				ctx.Response.StatusCode = (int) HttpStatusCode.OK;
				ctx.Response.ContentType = "text/json";

				var d = new {d = data};
				var serializer = new JavaScriptSerializer();
				var json = serializer.Serialize(d);

				ctx.Response.Output.Write(json);
			};
		}

		public Action<RequestContext> Xml(object data)
		{
			if (data == null) throw new ArgumentNullException("data");

			return req =>
			{
				var ctx = req.HttpContext;
				ctx.Response.StatusCode = (int) HttpStatusCode.OK;
				ctx.Response.ContentType = "text/xml";

				var serializer = new XmlSerializer(data.GetType());
				serializer.Serialize(ctx.Response.Output, data);
			};
		}

		public Action<RequestContext> Error(HttpRequestBase request, Exception e)
		{
			return Error("{0} request failed. Exception: {1}", request.Url, e);
		}

		public Action<RequestContext> Error(string format, params object[] args)
		{
			var message = string.Format(format, args);
			return req =>
			{
				var ctx = req.HttpContext;
				ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				ctx.Response.ContentType = "text/plain";
				ctx.Response.Write(message);
			};
		}

		public Action<RequestContext> NotFound(string path)
		{
			return req =>
			{
				var ctx = req.HttpContext;
				ctx.Response.StatusCode = (int) HttpStatusCode.NotFound;
				ctx.Response.ContentType = "text/plain";
				ctx.Response.Output.Write("Resource '{0}' not found!", path);
			};
		}
	}
}