#if NUNIT
using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture]
	public class MimeTypesFixture
	{
		[TestCase(".css", Result = "text/css")]
		[TestCase(".html", Result = "text/html")]
		[TestCase(".htm", Result = "text/html")]
		[TestCase(".js", Result = "application/javascript")]
		[TestCase(".json", Result = "application/json")]
		[TestCase(".xml", Result = "application/xml")]
		[TestCase("css", Result = "text/css")]
		[TestCase("html", Result = "text/html")]
		[TestCase("htm", Result = "text/html")]
		[TestCase("js", Result = "application/javascript")]
		[TestCase("json", Result = "application/json")]
		[TestCase("xml", Result = "application/xml")]
		public string ByExtension(string ext)
		{
			return MimeType.ByExtension(ext);
		}
	}
}
#endif
