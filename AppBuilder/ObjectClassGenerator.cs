using System;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	public static class ObjectClassGenerator
	{
		public static string Generate(ClrClass @class, bool readOnly)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(1024 * 2);
			ClassGenerator.AppendClassDefinition(buffer, @class);
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			AppendProperties(buffer, @class, readOnly);
			buffer.AppendLine();
			AppendContructor(buffer, @class, readOnly);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendProperties(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			Action<StringBuilder, ClrProperty> appender = ClrProperty.AppendMutableProperty;
			if (readOnly)
			{
				appender = ClrProperty.AppendReadOnlyProperty;
			}
			foreach (var property in @class.Properties)
			{
				appender(buffer, property);
				buffer.AppendLine();
			}
		}

		private static void AppendContructor(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			Action<StringBuilder, ClrClass> appender = ClrClass.AppendEmptyConstructor;
			if (readOnly)
			{
				appender = ClrClass.AppendConstructorWithParameters;
			}
			appender(buffer, @class);
		}
	}
}