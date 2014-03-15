﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpExpress
{
	internal static class Extensions
	{
		public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
		{
			var attrs = (T[]) provider.GetCustomAttributes(typeof(T), inherit);
			return attrs.Length > 0 ? attrs[0] : null;
		}

		public static void Write(this Stream stream, string s)
		{
			var bytes = Encoding.UTF8.GetBytes(s);
			stream.Write(bytes, 0, bytes.Length);
		}

		public static bool Like(this string s, string wildcard)
		{
			return WildcardRegex(wildcard).IsMatch(s);
		}

		public static Regex WildcardRegex(this string wildcard)
		{
			return new Regex(
				"^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$"
				, RegexOptions.Compiled | RegexOptions.IgnoreCase
				);
		}
	}
}
