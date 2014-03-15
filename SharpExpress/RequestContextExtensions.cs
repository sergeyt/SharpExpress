using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace SharpExpress
{
	public static class RequestContextExtensions
	{
		public static T Param<T>(this RequestContext req, string name)
		{
			var val = req.RouteData.Values[name];
			return (T) Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
		}

		public static void Text(this RequestContext req, string text)
		{
			Text(req, text, HttpStatusCode.OK);
		}

		public static void Text(this RequestContext req, string text, HttpStatusCode status)
		{
			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) status;
			ctx.Response.ContentType = "text/plain";
			ctx.Response.Write(text);
		}

		public static void Json(this RequestContext req, object data)
		{
			if (data == null) throw new ArgumentNullException("data");

			var ctx = req.HttpContext;
			ctx.Response.StatusCode = (int) HttpStatusCode.OK;
			ctx.Response.ContentType = "application/json";

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
			Text(req, string.Format(format, args), HttpStatusCode.InternalServerError);
		}

		public static void NotFound(this RequestContext req)
		{
			NotFound(req, req.HttpContext.Request.Path);
		}

		public static void NotFound(this RequestContext req, string path)
		{
			Text(req, string.Format("Resource '{0}' not found!", path), HttpStatusCode.NotFound);
		}

		public static IDictionary<string, object> ParseJson(this RequestContext req)
		{
			using (var reader = new StreamReader(req.HttpContext.Request.InputStream))
			{
				var json = reader.ReadToEnd();
				var serializer = new JavaScriptSerializer();
				return (IDictionary<string, object>) serializer.DeserializeObject(json);
			}
		}
	}
}
