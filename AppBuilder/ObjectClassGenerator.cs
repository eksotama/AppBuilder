using System;
using System.Linq;
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
			AppendContructor(buffer, @class, readOnly);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendProperties(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			Action<StringBuilder, ClrProperty> appender = ClrProperty.AppendPublicMutableProperty;
			if (readOnly)
			{
				appender = ClrProperty.AppendPublicReadOnlyProperty;
			}
			foreach (var property in @class.Properties)
			{
				appender(buffer, property);
			}

			buffer.AppendLine();
		}

		private static void AppendContructor(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			ClassGenerator.AppendContructorName(buffer, @class);
			buffer.Append(@"(");
			ClrClass.AppendParameterDefinitions(buffer, @class, readOnly);
			buffer.Append(@")");

			buffer.AppendLine();
			buffer.AppendLine(@"{");
			if (readOnly)
			{
				AppendPropertiesAssignments(buffer, @class);
			}
			else
			{
				AppendPropertiesInitialization(buffer, @class);
			}
			buffer.AppendLine(@"}");
		}

		private static void AppendPropertiesAssignments(StringBuilder buffer, ClrClass @class)
		{
			foreach (var property in @class.Properties.Where(ClrProperty.IsReferenceType))
			{
				ClrProperty.AppendParameterCheck(buffer, property);
			}

			buffer.AppendLine();

			AppendPropertiesAssignments(buffer, @class, ClrProperty.AppendParameterName);
		}

		private static void AppendPropertiesInitialization(StringBuilder buffer, ClrClass @class)
		{
			AppendPropertiesAssignments(buffer, @class, ClrProperty.AppendDefaultValue);
		}

		private static void AppendPropertiesAssignments(StringBuilder buffer, ClrClass @class, Action<StringBuilder, ClrProperty> selector)
		{
			foreach (var property in @class.Properties)
			{
				buffer.Append(@"this.");
				ClrProperty.AppendName(buffer, property);
				buffer.Append(@" = ");
				selector(buffer, property);
				buffer.AppendLine(@";");
			}
		}
	}
}