using System;

namespace AppBuilder.Clr.Providers
{
	public static class ParameterNameProvider
	{
		public static string GetParameterName(ClrProperty property)
		{
			if (property == null) throw new ArgumentNullException("property");

			return StringUtils.LowerFirst(property.Name);
		}
	}
}