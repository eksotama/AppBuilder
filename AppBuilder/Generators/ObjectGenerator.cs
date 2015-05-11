using System;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Clr.Providers;

namespace AppBuilder.Generators
{
	public static class ObjectGenerator
	{
		public static string Generate(ClrClass @class, bool readOnly)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(2 * 1024);
			buffer.Append(@"public sealed class ");
			buffer.Append(@class.Name);
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
			Action<StringBuilder, ClrProperty> appender = AppendMutableProperty;
			if (readOnly)
			{
				appender = AppendReadOnlyProperty;
			}
			foreach (var property in @class.Properties)
			{
				appender(buffer, property);
				buffer.AppendLine();
			}
		}

		private static void AppendMutableProperty(StringBuilder buffer, ClrProperty property)
		{
			AppendProperty(buffer, property, @" { get; set; }");
		}

		private static void AppendReadOnlyProperty(StringBuilder buffer, ClrProperty property)
		{
			AppendProperty(buffer, property, @" { get; private set; }");
		}

		private static void AppendProperty(StringBuilder buffer, ClrProperty property, string storeModifier)
		{
			buffer.Append(@"public ");
			buffer.Append(PropertyTypeProvider.GetPropertyType(property));
			buffer.Append(@" ");
			buffer.Append(property.Name);
			buffer.Append(storeModifier);
		}

		private static void AppendContructor(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			Action<StringBuilder, ClrClass> appender = AppendEmptyConstructor;
			if (readOnly)
			{
				appender = AppendConstructorWithParameters;
			}
			appender(buffer, @class);
		}

		private static void AppendEmptyConstructor(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"()");
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			foreach (var property in @class.Properties)
			{
				AppendInitToDefaultValue(buffer, property);
			}
			buffer.AppendLine(@"}");
		}

		private static void AppendConstructorWithParameters(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"(");
			AppendParameters(buffer, @class, AppendParameter);
			buffer.Append(@")");
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			AppendParametersCheck(buffer, @class);
			foreach (var property in @class.Properties)
			{
				AppendInitToParameterName(buffer, property);
			}
			buffer.AppendLine(@"}");
		}

		private static void AppendInitToDefaultValue(StringBuilder buffer, ClrProperty property)
		{
			AppendInitializationTo(buffer, property, DefaultValueProvider.GetDefaultValue(property));
		}

		private static void AppendParameters(StringBuilder buffer, ClrClass @class, Action<StringBuilder, ClrProperty> appender)
		{
			var addSeparator = false;
			foreach (var property in @class.Properties)
			{
				if (addSeparator)
				{
					buffer.Append(@", ");
				}
				appender(buffer, property);
				addSeparator = true;
			}
		}

		private static void AppendParameter(StringBuilder buffer, ClrProperty property)
		{
			buffer.Append(PropertyTypeProvider.GetPropertyType(property));
			buffer.Append(@" ");
			buffer.Append(ParameterNameProvider.GetParameterName(property));
		}

		private static void AppendParametersCheck(StringBuilder buffer, ClrClass @class)
		{
			var oldLength = buffer.Length;
			foreach (var property in @class.Properties)
			{
				if (!property.Nullable && property.IsReferenceType)
				{
					AppendParameterCheck(buffer, ParameterNameProvider.GetParameterName(property));
				}
			}
			// Separate argument checks with properies assignments
			if (oldLength != buffer.Length)
			{
				buffer.AppendLine();
			}
		}

		private static void AppendParameterCheck(StringBuilder buffer, string name)
		{
			buffer.Append(@"if (");
			buffer.Append(name);
			StringUtils.LowerFirst(buffer, name);
			buffer.Append(@" == null ) throw new ArgumentNullException(""");
			buffer.Append(name);
			StringUtils.LowerFirst(buffer, name);
			buffer.Append(@""");");
			buffer.AppendLine();
		}

		private static void AppendInitToParameterName(StringBuilder buffer, ClrProperty property)
		{
			AppendInitializationTo(buffer, property, ParameterNameProvider.GetParameterName(property));
		}

		private static void AppendInitializationTo(StringBuilder buffer, ClrProperty property, string value)
		{
			buffer.Append(@"this.");
			buffer.Append(property.Name);
			buffer.Append(@" = ");
			buffer.Append(value);
			buffer.Append(@";");
			buffer.AppendLine();
		}
	}
}