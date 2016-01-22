namespace SharpExpress
{
	/// <summary>
	/// Extension to support CORS.
	/// </summary>
	public static class CorsExtension
	{
		public static ExpressApplication EnableCors(this ExpressApplication app, string route = "{*url}")
		{
			return app.Options(route, req =>
			{
				// TODO config CORS headers
				// http://enable-cors.org/server_expressjs.html
				req.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
				req.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
			});
		}
	}
}
