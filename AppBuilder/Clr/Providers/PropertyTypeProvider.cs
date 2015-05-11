using System;

namespace AppBuilder.Clr.Providers
{
	public static class PropertyTypeProvider
	{
		public static string GetPropertyType(ClrProperty property)
		{
			if (property == null) throw new ArgumentNullException("property");

			var type = property.Type;
			if ((type == ClrType.Integer || type == ClrType.Decimal || type == ClrType.DateTime) && property.Nullable)
			{
				return type.NullableName;
			}
			return type.Name;
		}
	}
}