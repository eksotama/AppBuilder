using System;
using System.Collections.Generic;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	public static class CodeGenerator
	{
		private static readonly char Space = ' ';
		private static readonly char Comma = ',';
		private static readonly char Semicolumn = ';';

		public static string GetClass(ClassDefinition @class, bool immutable)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder();

			buffer.Append(GetAccessModifier(@class.AccessModifier));
			buffer.Append(Space);
			var @sealed = GetIsSealed(@class.Sealed);
			if (@sealed != string.Empty)
			{
				buffer.Append(@sealed);
				buffer.Append(Space);
			}
			buffer.Append(@"class");
			buffer.Append(Space);
			buffer.Append(@class.Name);
			var interfaces = @class.Interfaces;
			if (interfaces.Count > 0)
			{
				buffer.Append(@" : ");

				var addSeparator = false;
				foreach (var @interface in interfaces)
				{
					if (addSeparator)
					{
						buffer.Append(Comma);
					}
					buffer.Append(@interface);
					addSeparator = true;
				}
			}
			buffer.AppendLine();

			buffer.AppendLine(@"{");

			if (@class.Fields.Count > 0)
			{
				foreach (var field in @class.Fields)
				{
					buffer.AppendLine(GetField(field));
				}
				buffer.AppendLine();
			}
			var properties = @class.Properties;
			if (properties.Length > 0)
			{
				foreach (var property in properties)
				{
					buffer.AppendLine(GetProperty(property));
				}
				buffer.AppendLine();
			}

			ContructorDefinition contructorDefinition;
			if (immutable)
			{
				contructorDefinition = new ContructorDefinition(@class.Name, GetParameters(properties));
			}
			else
			{
				contructorDefinition = new ContructorDefinition(@class.Name, properties);
			}
			buffer.AppendLine(GetContructor(contructorDefinition));

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string GetHelper(ClassDefinition @class, NameProvider nameProvider)
		{
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(1024);
			var name = @class.Name;
			var varName = StringUtils.LowerFirst(nameProvider.GetDbName(name));

			buffer.Append(@"public sealed class ");
			buffer.Append(name);
			buffer.Append(@"Helper");
			buffer.AppendLine(@"{");

			// Field
			buffer.Append(@"private readonly Dictionary<long, ");
			buffer.Append(name);
			buffer.Append(@"> _");
			buffer.Append(varName);
			buffer.Append(@" = new Dictionary<long, ");
			buffer.Append(name);
			buffer.AppendLine(@">();");

			// Property
			buffer.AppendLine();
			buffer.Append(@"public Dictionary<long, ");
			buffer.Append(name);
			buffer.Append(@"> ");
			buffer.Append(varName);
			StringUtils.UpperFirst(buffer, varName);
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			buffer.Append(@"get { return _");
			buffer.Append(varName);
			buffer.Append(@"; }");
			buffer.AppendLine(@"}");
			buffer.AppendLine();

			// Method
			buffer.Append(@"public void Load(");
			buffer.Append(name);
			buffer.Append(@"Adapter adapter)");
			buffer.AppendLine(@"{");
			buffer.AppendLine(@"if (adapter == null) throw new ArgumentNullException(""adapter"");");
			buffer.AppendLine();
			buffer.Append(@"adapter.Fill(_");
			buffer.Append(varName);
			buffer.AppendLine(@");");
			buffer.AppendLine(@"}");
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static string GetAccessModifier(AccessModifier modifier)
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

		private static string GetIsReadOnly(bool isReadOnly)
		{
			return isReadOnly ? @"readonly" : string.Empty;
		}

		private static string GetIsSealed(bool isSealed)
		{
			return isSealed ? @"sealed" : string.Empty;
		}

		private static string GetField(FieldDefinition field)
		{
			if (field == null) throw new ArgumentNullException("field");

			var buffer = new StringBuilder();

			buffer.Append(GetAccessModifier(field.AccessModifier));
			buffer.Append(Space);
			AppendReadOnly(field, buffer);
			buffer.Append(field.Type.Name);
			buffer.Append(Space);
			buffer.Append(field.Name);
			AppendInitialValue(field, buffer);
			buffer.Append(Semicolumn);

			return buffer.ToString();
		}

		private static void AppendInitialValue(FieldDefinition field, StringBuilder buffer)
		{
			var initialValue = field.InitialValue;
			if (initialValue != string.Empty)
			{
				buffer.Append(@" = ");
				buffer.Append(initialValue);
			}
		}

		private static void AppendReadOnly(FieldDefinition field, StringBuilder buffer)
		{
			var readOnly = GetIsReadOnly(field.IsReadOnly);
			if (readOnly != string.Empty)
			{
				buffer.Append(readOnly);
				buffer.Append(Space);
			}
		}

		private static string GetProperty(PropertyDefinition property)
		{
			if (property == null) throw new ArgumentNullException("property");

			var buffer = new StringBuilder();

			buffer.Append(GetAccessModifier(property.AccessModifier));
			buffer.Append(Space);
			buffer.Append(property.Type.Name);
			buffer.Append(Space);
			buffer.Append(property.Name);
			buffer.Append(Space);
			buffer.Append(GetBackingField(property.BackingField));

			return buffer.ToString();
		}

		private static string GetContructor(ContructorDefinition definition)
		{
			var buffer = new StringBuilder();

			buffer.Append(GetAccessModifier(definition.AccessModifier));
			buffer.Append(Space);
			buffer.Append(definition.Name);
			buffer.Append('(');
			var addSeparator = false;
			foreach (var parameter in definition.Parameters)
			{
				if (addSeparator)
				{
					buffer.Append(Comma);
				}
				AppendParameter(buffer, parameter);
				addSeparator = true;
			}
			buffer.Append(')');
			buffer.AppendLine();
			buffer.AppendLine(@"{");

			var hasChecks = false;
			foreach (var parameter in definition.Parameters)
			{
				if (parameter.Type.IsReference)
				{
					AppendCheck(buffer, parameter);
					hasChecks = true;
				}
			}
			if (hasChecks)
			{
				buffer.AppendLine();
			}
			foreach (var parameter in definition.Parameters)
			{
				AppendPropertyAssignment(buffer, parameter);
			}
			foreach (var parameter in definition.Properties)
			{
				AppendPropertyInitialization(buffer, parameter);
			}
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendPropertyAssignment(StringBuilder buffer, ParameterDefinition parameter)
		{
			var name = parameter.Name;
			var upper = char.ToUpperInvariant(name[0]);
			buffer.Append(@"this.");
			buffer.Append(name);
			buffer[buffer.Length - name.Length] = upper;
			buffer.Append(@" = ");
			buffer.Append(name);
			buffer.AppendLine(@";");
		}

		private static void AppendPropertyInitialization(StringBuilder buffer, PropertyDefinition parameter)
		{
			buffer.Append(@"this.");
			buffer.Append(parameter.Name);
			buffer.Append(@" = ");
			buffer.Append(parameter.Type.DefaultValue);
			buffer.AppendLine(@";");
		}

		private static void AppendCheck(StringBuilder buffer, ParameterDefinition parameter)
		{
			var name = parameter.Name;
			buffer.Append(@"if (");
			buffer.Append(name);
			buffer.Append(@" == null) throw new ArgumentNullException(""");
			buffer.Append(name);
			buffer.Append(@""");");
			buffer.AppendLine();
		}

		private static void AppendParameter(StringBuilder buffer, ParameterDefinition parameter)
		{
			var type = parameter.Type.Name;
			var name = parameter.Name;

			buffer.Append(type);
			buffer.Append(Space);
			buffer.Append(name);
		}

		private static string GetBackingField(FieldDefinition field)
		{
			if (field == FieldDefinition.ImmutableAutoProperty)
			{
				return @"{ get; private set; }";
			}
			if (field == FieldDefinition.AutoProperty)
			{
				return @"{ get; set; }";
			}
			// TODO : !!!!!
			return @"{ get { return _name; } set { _name = value; } }";
		}

		private static ParameterDefinition[] GetParameters(IReadOnlyList<PropertyDefinition> properties)
		{
			var parameters = new ParameterDefinition[properties.Count];
			for (var i = 0; i < properties.Count; i++)
			{
				var p = properties[i];
				parameters[i] = new ParameterDefinition(p.Type, p.Name);
			}
			return parameters;
		}
	}
}