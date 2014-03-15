﻿using System;
using System.IO;
using System.Reflection;
using System.Text;

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
	}
}