using System;

namespace SharpExpress
{
	/// <summary>
	/// Static middleware to serve static files.
	/// </summary>
	public static class StaticMiddleware
	{
		/// <summary>
		/// Serves specified directory
		/// </summary>
		/// <param name="app">The application to extend.</param>
		/// <param name="url"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		public static ExpressApplication Static(this ExpressApplication app, string url, string dir)
		{
			return app.Get(url, req =>
			{
				throw new NotImplementedException();
			});
		}
	}
}
