using System.Collections.Generic;
using System.Web;

namespace SharpExpress
{
	internal static class QueryString
	{
		public static IEnumerable<KeyValuePair<string, string>> Parse(string s)
		{
			var start = 0;
			if (s.StartsWith("?"))
			{
				start++;
			}

			for (var i = start; i < s.Length; i++)
			{
				if (s[i] == '=')
				{
					var len = i - start;
					var name = s.Substring(start, len);

					var j = s.IndexOf('&', i + 1);
					j = j >= 0 ? j - 1 : s.Length - 1;
					
					var val = HttpUtility.UrlDecode(s.Substring(i + 1, j - i));
					yield return new KeyValuePair<string, string>(name, val);

					i = j + 1;
					start = i + 1;
				}
			}
		}
	}
}
