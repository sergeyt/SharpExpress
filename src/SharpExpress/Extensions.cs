using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpExpress
{
	internal static class Extensions
	{
		public static T Get<T>(this IDictionary<string, object> data, string key, T defval)
		{
			object val;
			if (!data.TryGetValue(key, out val))
			{
				return defval;
			}
			return (T)val.ConvertTo(Equals(defval, null) ? typeof(T) : defval.GetType());
		}

		public static object Get(this IDictionary<string, object> data, string key, object defval, Type valueType)
		{
			object val;
			if (!data.TryGetValue(key, out val))
			{
				return defval;
			}
			return val.ConvertTo(valueType);
		}

		public static NameValueCollection ToNameValueCollection(this IEnumerable<KeyValuePair<string, string>> pairs, StringComparer comparer)
		{
			var collection = new NameValueCollection(comparer);
			collection.AddRange(pairs);
			return collection;
		}

		public static void AddRange(this NameValueCollection collection, IEnumerable<KeyValuePair<string, string>> pairs)
		{
			foreach (var pair in pairs)
			{
				collection.Set(pair.Key, pair.Value);
			}
		}

		public static TR IfNotNull<T, TR>(this T obj, Func<T, TR> func) where T : class
		{
			return obj != null ? func(obj) : default (TR);
		}

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
