using System;
using System.ComponentModel;
using System.Globalization;

namespace SharpExpress
{
	internal static class ConvertExtension
	{
		public static object ConvertTo(this object val, Type type)
		{
			if (val == null) return null;

			if (type.IsEnum)
			{
				return Enum.ToObject(type, val);
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Object:
					if (type.IsArray)
					{
						var source = val as Array;
						if (source != null)
						{
							var elemType = type.GetElementType();
							var arr = Array.CreateInstance(elemType, source.Length);
							for (var i = 0; i < source.Length; i++)
							{
								var item = ConvertTo(source.GetValue(i), elemType);
								arr.SetValue(item, i);
							}
							return arr;
						}
					}
					var s = val as String;
					if (s != null && type != typeof(string))
					{
						var converter = TypeDescriptor.GetConverter(type);
						return converter.ConvertFromString(s);
					}
					return val;
				default:
					return Convert.ChangeType(val, type, CultureInfo.InvariantCulture);
			}
		}
	}
}
