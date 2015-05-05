using System;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	public static class ClassGenerator
	{
		public static string GenerateObjectClass(ClrClass @class, bool readOnly)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(1024);

			AppendClass(buffer, @class);
			buffer.AppendLine(@"{");
			AppendProperties(buffer, @class, readOnly);
			AppendContructor(buffer, @class, readOnly);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendClass(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@"public sealed class ");
			buffer.AppendLine(@class.Name);
		}

		private static void AppendProperties(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			if (readOnly)
			{
				foreach (var property in @class.Properties)
				{
					AppendReadOnlyProperty(buffer, property);
				}
			}
			else
			{
				foreach (var property in @class.Properties)
				{
					AppendMutableProperty(buffer, property);
				}
			}

			buffer.AppendLine();
		}

		private static void AppendReadOnlyProperty(StringBuilder buffer, ClrProperty property)
		{
			AppendProperty(buffer, property);
			buffer.AppendLine(@" { get; private set; }");
		}

		private static void AppendMutableProperty(StringBuilder buffer, ClrProperty property)
		{
			AppendProperty(buffer, property);
			buffer.AppendLine(@" { get; set; }");
		}

		private static void AppendProperty(StringBuilder buffer, ClrProperty property)
		{
			buffer.Append(@"public ");
			buffer.Append(GetPropertyType(property));
			buffer.Append(@" ");
			buffer.Append(GetPropertyName(property));
		}

		private static void AppendContructor(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			AppendContructorName(buffer, @class);
			buffer.Append(@"(");
			if (readOnly)
			{
				AppendContructorParameters(buffer, @class);
				buffer.AppendLine(@"){");
				AppendContructorParametersChecks(buffer, @class);
				AppendContructorParametersAssignments(buffer, @class);
			}
			else
			{
				// No parameters
				buffer.AppendLine(@"){");
				AppendContructorPropertiesInitialization(buffer, @class);
			}
			buffer.AppendLine(@"}");
		}

		private static void AppendContructorName(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@"public");
			buffer.Append(@" ");
			buffer.Append(@class.Name);
		}

		private static void AppendContructorParameters(StringBuilder buffer, ClrClass @class)
		{
			var addSeparator = false;
			foreach (var property in @class.Properties)
			{
				if (addSeparator)
				{
					buffer.Append(@", ");
				}
				buffer.Append(GetPropertyType(property));
				buffer.Append(@" ");

				AppendParameterName(buffer, property);

				addSeparator = true;
			}
		}

		private static void AppendParameterName(StringBuilder buffer, ClrProperty property)
		{
			// Parameter name - Property name with lowered first letter
			var name = property.Name;
			buffer.Append(name);
			buffer[buffer.Length - name.Length] = char.ToLowerInvariant(name[0]);
		}

		private static void AppendContructorParametersChecks(StringBuilder buffer, ClrClass @class)
		{
			foreach (var property in @class.Properties)
			{
				var isReference = IsReferenceType(property);
				if (isReference)
				{
					buffer.Append(@"if (");
					AppendParameterName(buffer, property);
					buffer.Append(@" == null ) throw new ArgumentNullException(""");
					AppendParameterName(buffer, property);
					buffer.AppendLine(@""");");
				}
			}
			buffer.AppendLine();
		}

		private static void AppendContructorParametersAssignments(StringBuilder buffer, ClrClass @class)
		{
			foreach (var property in @class.Properties)
			{
				buffer.Append(@"this.");
				buffer.Append(GetPropertyName(property));
				buffer.Append(@" = ");
				AppendParameterName(buffer, property);
				buffer.AppendLine(@";");
			}
		}

		private static void AppendContructorPropertiesInitialization(StringBuilder buffer, ClrClass @class)
		{
			foreach (var property in @class.Properties)
			{
				var defaultValue = GetDefaultPropertyValue(property);
				if (defaultValue != string.Empty)
				{
					buffer.Append(@"this.");
					buffer.Append(GetPropertyName(property));
					buffer.Append(@" = ");
					buffer.Append(defaultValue);
					buffer.AppendLine(@";");
				}
			}
		}

		private static bool IsReferenceType(ClrProperty property)
		{
			var type = property.Type;
			if (type == ClrType.Integer || type == ClrType.Decimal || type == ClrType.DateTime)
			{
				return property.Nullable;
			}
			return true;
		}

		private static string GetDefaultPropertyValue(ClrProperty property)
		{
			var type = property.Type;
			if (type == ClrType.DateTime)
			{
				if (property.Name.EndsWith(@"From"))
				{
					return @"DateTime.MinValue";
				}
				if (property.Name.EndsWith(@"To"))
				{
					return @"DateTime.MaxValue";
				}
			}
			if (type == ClrType.String)
			{
				return @"string.Empty";
			}
			return string.Empty;
		}

		private static string GetPropertyType(ClrProperty property)
		{
			var type = property.Type;
			if (type == ClrType.Integer)
			{
				return property.Nullable ? @"long?" : @"long";
			}
			if (type == ClrType.Decimal)
			{
				return property.Nullable ? @"decimal?" : @"decimal";
			}
			if (type == ClrType.DateTime)
			{
				return property.Nullable ? @"DateTime?" : @"DateTime";
			}
			if (type == ClrType.String)
			{
				return @"string";
			}
			if (type == ClrType.Bytes)
			{
				return @"byte[]";
			}
			return type.Name;
		}

		private static string GetPropertyName(ClrProperty property)
		{
			var type = property.Type;
			var propertyName = property.Name;
			if (type == ClrType.Integer || type == ClrType.Decimal || type == ClrType.DateTime || type == ClrType.String || type == ClrType.Bytes)
			{
				return propertyName;
			}

			// Property to other object. Remove ...Id to obtain object property name
			return propertyName.Substring(0, propertyName.Length - @"Id".Length);
		}
	}
}