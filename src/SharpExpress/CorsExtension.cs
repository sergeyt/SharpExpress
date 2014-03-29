namespace SharpExpress
{
	/// <summary>
	/// Extension to support CORS.
	/// </summary>
	public static class CorsExtension
	{
		public static ExpressApplication Cors(this ExpressApplication app, string url)
		{
			return app.Options(url, req =>
			{
				// TODO config CORS headers
				// http://enable-cors.org/server_expressjs.html
				req.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
				req.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
			});
		}

		public static ExpressApplication Cors(this ExpressApplication app)
		{
			return app.Cors("{*}");
		}
	}
}
