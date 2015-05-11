using System;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	public static class CodeGenerator
	{
		public static string GetModifier(AccessModifier modifier)
		{
			switch (modifier)
			{
				case AccessModifier.Public:
					return @"public";
				case AccessModifier.Private:
					return @"private";
				case AccessModifier.Protected:
					return @"protected";
				case AccessModifier.Internal:
					return @"internal";
				default:
					throw new ArgumentOutOfRangeException("modifier");
			}
		}

		public static string GenerateObject(ClrClass @class, bool readOnly)
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

		private static void AppendProperty(StringBuilder buffer, ClrProperty property, string accessModifier)
		{
			buffer.Append(@"public ");
			buffer.Append(property.PropertyType);
			buffer.Append(@" ");
			buffer.Append(property.Name);
			buffer.Append(accessModifier);
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
				ClrProperty.AppendInitToDefaultValue(buffer, property);
			}
			buffer.AppendLine(@"}");
		}

		private static void AppendConstructorWithParameters(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"(");
			AppendParameters(buffer, @class);
			buffer.Append(@")");
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			AppendParametersCheck(buffer, @class);
			foreach (var property in @class.Properties)
			{
				ClrProperty.AppendInitToParameterName(buffer, property);
			}
			buffer.AppendLine(@"}");
		}

		private static void AppendParametersCheck(StringBuilder buffer, ClrClass @class)
		{
			var oldLength = buffer.Length;
			foreach (var property in @class.Properties)
			{
				if (!property.Nullable && property.IsReferenceType)
				{
					ClrProperty.AppendParameterCheck(buffer, property);
				}
			}
			// Separate argument checks with properies assignments
			if (oldLength != buffer.Length)
			{
				buffer.AppendLine();
			}
		}

		private static void AppendParameters(StringBuilder buffer, ClrClass @class)
		{
			AppendParameters(buffer, @class, ClrProperty.AppendParameter);
		}

		private static void AppendParameterNames(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, ClrProperty.AppendParameterName);
		}

		private static void AppendParametersAssignments(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, ClrProperty.AppendPropertyNameParameterName);
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
	}
}