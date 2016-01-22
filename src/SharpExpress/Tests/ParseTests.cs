#if NUNIT
using System.Linq;
using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture]
	public class ParseTests
	{
		[TestCase("name=value", ExpectedResult = "[name][value]")]
		[TestCase("?name=value", ExpectedResult = "[name][value]")]
		[TestCase("name1=val1&name2=val2", ExpectedResult = "[name1][val1];[name2][val2]")]
		[TestCase("?name1=val1&name2=val2", ExpectedResult = "[name1][val1];[name2][val2]")]
		public static string ParseQueryString(string qs)
		{
			var pairs = QueryString.Parse(qs).ToArray();
			return string.Join(";", (from p in pairs select "[" + p.Key + "]" + "[" + p.Value + "]").ToArray());
		}
	}
}
#endif
