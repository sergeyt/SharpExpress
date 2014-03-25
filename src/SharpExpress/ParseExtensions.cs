using System.Collections.Generic;
using System.Web;

namespace SharpExpress
{
	internal struct Range
	{
		public readonly int Start;
		public readonly int Length;

		public Range(int start, int length)
		{
			Start = start;
			Length = length;
		}
	}

	internal static class ParseExtensions
	{
		public static IEnumerable<Range> GetLineRanges(this byte[] bytes)
		{
			if (bytes == null)
			{
				yield break;
			}

			var start = 0;
			for (var i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] == '\n')
				{
					int len = i - start;
					if (len > 0 && bytes[i - 1] == (byte)'\r')
					{
						len--;
					}
					yield return new Range(start, len);
					start = i + 1;
				}
			}

			if (start < bytes.Length)
			{
				yield return new Range(start, bytes.Length - start);
			}
		}

		public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(this string s)
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
