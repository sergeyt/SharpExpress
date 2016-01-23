#if NUNIT
using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture]
	public class MimeTypesFixture
	{
		[TestCase(".css", "text/css")]
		[TestCase(".html", "text/html")]
		[TestCase(".htm", "text/html")]
		[TestCase(".js", "application/javascript")]
		[TestCase(".json", "application/json")]
		[TestCase(".xml", "application/xml")]
		[TestCase("css", "text/css")]
		[TestCase("html", "text/html")]
		[TestCase("htm", "text/html")]
		[TestCase("js", "application/javascript")]
		[TestCase("json", "application/json")]
		[TestCase("xml", "application/xml")]
		public void ByExtension(string ext, string expected)
		{
			Assert.AreEqual(expected, MimeType.ByExtension(ext));
		}
	}
}
#endif
