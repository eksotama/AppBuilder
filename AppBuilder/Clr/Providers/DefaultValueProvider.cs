using System;

namespace AppBuilder.Clr.Providers
{
	public static class DefaultValueProvider
	{
		public static string GetDefaultValue(ClrProperty property)
		{
			if (property == null) throw new ArgumentNullException("property");

			var type = property.Type;
			if (type == ClrType.Integer)
			{
				return property.Nullable ? @"default(long?)" : @"0L";
			}
			if (type == ClrType.String)
			{
				return @"string.Empty";
			}
			if (type == ClrType.Decimal)
			{
				return property.Nullable ? @"default(decimal?)" : @"0M";
			}
			if (type == ClrType.DateTime)
			{
				var name = property.Name;
				if (name.EndsWith(@"From"))
				{
					return @"DateTime.MinValue";
				}
				if (name.EndsWith(@"To"))
				{
					return @"DateTime.MaxValue";
				}
				return property.Nullable ? @"default(DateTime?)" : @"DateTime.MinValue";
			}
			if (type == ClrType.Bytes)
			{
				return @"default(byte[])";
			}

			return @"default(" + type.Name + @")";
		}
	}
}