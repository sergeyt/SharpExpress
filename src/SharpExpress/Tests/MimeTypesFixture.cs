#if NUNIT
using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture]
	public class MimeTypesFixture
	{
		[TestCase(".css", ExpectedResult = "text/css")]
		[TestCase(".html", ExpectedResult = "text/html")]
		[TestCase(".htm", ExpectedResult = "text/html")]
		[TestCase(".js", ExpectedResult = "application/javascript")]
		[TestCase(".json", ExpectedResult = "application/json")]
		[TestCase(".xml", ExpectedResult = "application/xml")]
		[TestCase("css", ExpectedResult = "text/css")]
		[TestCase("html", ExpectedResult = "text/html")]
		[TestCase("htm", ExpectedResult = "text/html")]
		[TestCase("js", ExpectedResult = "application/javascript")]
		[TestCase("json", ExpectedResult = "application/json")]
		[TestCase("xml", ExpectedResult = "application/xml")]
		public string ByExtension(string ext)
		{
			return MimeType.ByExtension(ext);
		}
	}
}
#endif
