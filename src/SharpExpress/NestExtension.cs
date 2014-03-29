namespace SharpExpress
{
	/// <summary>
	/// Allows to compose <see cref="ExpressApplication"/>s.
	/// </summary>
	public static class NestExtension
	{
		public static ExpressApplication Nest(this ExpressApplication app, string prefix, ExpressApplication nested)
		{
			var url = prefix.EndsWith("/") ? prefix + "{*}" : prefix + "/{*}";
			return app.All(url, req => nested.Process(req.HttpContext));
		}
	}
}
