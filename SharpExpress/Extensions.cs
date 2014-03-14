using System;
using System.Reflection;

namespace SharpExpress
{
	internal static class Extensions
	{
		public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
		{
			var attrs = (T[]) provider.GetCustomAttributes(typeof(T), inherit);
			return attrs.Length > 0 ? attrs[0] : null;
		}
	}
}
