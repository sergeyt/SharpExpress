#if NUNIT
using System.Linq;
using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture]
	public class ParseTests
	{
		[TestCase("name=value", "[name][value]")]
		[TestCase("?name=value", "[name][value]")]
		[TestCase("name1=val1&name2=val2", "[name1][val1];[name2][val2]")]
		[TestCase("?name1=val1&name2=val2", "[name1][val1];[name2][val2]")]
		public static void ParseQueryString(string qs, string expected)
		{
			var pairs = QueryString.Parse(qs).ToArray();
			var result = string.Join(";", (from p in pairs select "[" + p.Key + "]" + "[" + p.Value + "]").ToArray());
			Assert.AreEqual(expected, result);
		}
	}
}
#endif
