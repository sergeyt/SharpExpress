using System;
using System.Net;
using System.Web;

namespace SharpExpress
{
	internal static class ProcessRequestExtension
	{
		public static void ProcessRequest(this IHttpChannel channel, IHttpHandler handler, HttpServerSettings settings)
		{
			var context = new HttpContextImpl(channel, settings);
			var res = context.Response;

			using (res.OutputStream)
			{
				try
				{
					var app = handler as ExpressApplication;
					if (app != null)
					{
						if (!app.Process(context))
						{
							res.StatusCode = (int)HttpStatusCode.NotFound;
							res.StatusDescription = "Not found";
							res.ContentType = "text/plain";
							res.Write("Resource not found!");
						}

						res.Flush();
						res.End();
					}
					else
					{
						var workerRequest = new HttpWorkerRequestImpl(context, settings);
						handler.ProcessRequest(new HttpContext(workerRequest));
						workerRequest.EndOfRequest();
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);

					res.StatusCode = (int)HttpStatusCode.InternalServerError;
					res.ContentType = "text/plain";
					res.Write(e.ToString());
				}
			}
		}
	}
}