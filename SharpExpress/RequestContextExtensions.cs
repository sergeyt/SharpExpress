using System;
using System.Net;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace SharpExpress
{
	public static class RequestContextExtensions
	{
		public static void Text(this RequestContext req, string text)
		{
			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) HttpStatusCode.OK;
			ctx.Response.ContentType = "text/plain";
			ctx.Response.Write(text);
		}

		public static void Json(this RequestContext req, object data)
		{
			if (data == null) throw new ArgumentNullException("data");

			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) HttpStatusCode.OK;
			ctx.Response.ContentType = "text/json";

			var d = new {d = data};
			var serializer = new JavaScriptSerializer();
			var json = serializer.Serialize(d);

			ctx.Response.Output.Write(json);
		}

		public static void Xml(this RequestContext req, object data)
		{
			if (data == null) throw new ArgumentNullException("data");
			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) HttpStatusCode.OK;
			ctx.Response.ContentType = "text/xml";

			var serializer = new XmlSerializer(data.GetType());
			serializer.Serialize(ctx.Response.Output, data);
		}

		public static void Error(this RequestContext req, Exception e)
		{
			req.Error("{0} request failed. Exception: {1}", req.HttpContext.Request.Url, e);
		}

		public static void Error(this RequestContext req, string format, params object[] args)
		{
			var message = string.Format(format, args);
			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
			ctx.Response.ContentType = "text/plain";
			ctx.Response.Write(message);
		}

		public static void NotFound(this RequestContext req, string path)
		{
			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) HttpStatusCode.NotFound;
			ctx.Response.ContentType = "text/plain";
			ctx.Response.Output.Write("Resource '{0}' not found!", path);
		}
	}
}