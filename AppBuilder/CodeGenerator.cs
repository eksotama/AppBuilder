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

		public static string GetClass(ClrClass @class, bool immutable)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder();

			buffer.Append(@"public");
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
					buffer.AppendLine(GetProperty(property, immutable));
				}
				buffer.AppendLine();
			}

			ClrContructor contructor;
			if (immutable)
			{
				contructor = new ClrContructor(@class.Name, GetParameters(properties));
			}
			else
			{
				contructor = new ClrContructor(@class.Name, properties);
			}
			buffer.AppendLine(GetContructor(contructor));

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string GetHelper(ClrClass @class, NameProvider nameProvider)
		{
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(1024);
			var name = @class.Name;
			var varName = StringUtils.LowerFirst(nameProvider.GetDbName(name));

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"sealed class");
			buffer.Append(Space);
			buffer.Append(name);
			buffer.Append(@"Helper");
			buffer.AppendLine(@"{");

			// Field
			buffer.Append(@"private readonly");
			buffer.Append(Space);
			buffer.Append(@"Dictionary<long, ");
			buffer.Append(name);
			buffer.Append(@"> _");
			buffer.Append(varName);
			buffer.Append(@" = new ");
			buffer.Append(@"Dictionary<long, ");
			buffer.Append(name);
			buffer.AppendLine(@">();");

			// Property
			buffer.AppendLine();
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"Dictionary<long, ");
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
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"void Load(");
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

		//private sealed class DictionaryField
		//{
		//	public ClrProperty Property { get; private set; }
		//	public string Type { get; private set; }
		//	public string Name { get; private set; }

		//	public DictionaryField(ClrProperty property, string type, string name)
		//	{
		//		if (property == null) throw new ArgumentNullException("property");
		//		if (type == null) throw new ArgumentNullException("type");
		//		if (name == null) throw new ArgumentNullException("name");

		//		this.Property = property;
		//		this.Type = type;
		//		this.Name = name;
		//	}
		//}

		//public static string Generate(DbTable table, NameProvider nameProvider, ClrClass @class, bool readOnly)
		//{
		//	if (@class == null) throw new ArgumentNullException("class");
		//	if (nameProvider == null) throw new ArgumentNullException("nameProvider");
		//	if (table == null) throw new ArgumentNullException("table");

		//	var buffer = new StringBuilder(2 * 1024);
		//	buffer.Append(@"public sealed class ");
		//	buffer.Append(@class.Name);
		//	buffer.Append(@"Adapter : I");
		//	buffer.Append(@class.Name);
		//	buffer.Append(@"Adapter");
		//	buffer.AppendLine(@"{");
		//	var dictionaries = GetDictionaries(@class, nameProvider);
		//	if (dictionaries.Count > 0)
		//	{
		//		AppendConstructor(buffer, @class, dictionaries);
		//	}
		//	AppendFillMethod(buffer, @class, QueryProvider.GetSelect(table));
		//	AppendCreatorMethod(buffer, @class, readOnly, dictionaries);
		//	AppendSelectorMethod(buffer, @class);
		//	buffer.AppendLine(@"}");

		//	return buffer.ToString();
		//}

		//public static string GenerateInterface(DbTable table, NameProvider nameProvider, ClrClass @class)
		//{
		//	if (table == null) throw new ArgumentNullException("table");
		//	if (nameProvider == null) throw new ArgumentNullException("nameProvider");
		//	if (@class == null) throw new ArgumentNullException("class");

		//	var buffer = new StringBuilder(@"public interface I", 128);

		//	buffer.Append(@class.Name);
		//	buffer.Append(@"Adapter");
		//	buffer.AppendLine(@"{");
		//	buffer.Append(@"void Fill(Dictionary<long, ");
		//	buffer.Append(@class.Name);
		//	buffer.Append(@"> ");
		//	buffer.Append(StringUtils.LowerFirst(nameProvider.GetDbName(@class.Name)));
		//	buffer.Append(@");");
		//	buffer.AppendLine(@"}");

		//	return buffer.ToString();
		//}

		//private static void AppendConstructor(StringBuilder buffer, ClrClass @class, List<DictionaryField> dictionaryFields)
		//{
		//	foreach (var dictionaryField in dictionaryFields)
		//	{
		//		AppendField(buffer, dictionaryField);
		//	}
		//	buffer.AppendLine();

		//	buffer.Append(@"public ");
		//	buffer.Append(@class.Name);
		//	buffer.Append(@"Adapter(");
		//	foreach (var dictionaryField in dictionaryFields)
		//	{
		//		AppendParameter(buffer, dictionaryField);
		//	}
		//	buffer[buffer.Length - 1] = ')';
		//	buffer.AppendLine(@"{");
		//	foreach (var dictionaryField in dictionaryFields)
		//	{
		//		AppendCheck(buffer, dictionaryField);
		//	}
		//	buffer.AppendLine();
		//	foreach (var dictionaryField in dictionaryFields)
		//	{
		//		AppendAssignment(buffer, dictionaryField);
		//	}
		//	buffer.AppendLine(@"}");
		//	buffer.AppendLine();
		//}

		//private static List<DictionaryField> GetDictionaries(ClrClass @class, NameProvider nameProvider)
		//{
		//	var dictionaries = new List<DictionaryField>(1);
		//	foreach (var property in @class.Properties)
		//	{
		//		if (!property.Type.IsBuiltIn)
		//		{
		//			var type = PropertyTypeProvider.GetPropertyType(property);
		//			dictionaries.Add(new DictionaryField(property, type, StringUtils.LowerFirst(nameProvider.GetDbName(type))));
		//		}
		//	}
		//	return dictionaries;
		//}

		//private static void AppendField(StringBuilder buffer, DictionaryField dictionaryField)
		//{
		//	buffer.Append(@"private readonly ");
		//	AppendDictionary(buffer, dictionaryField, @"_");
		//	buffer.AppendLine(@";");
		//}

		//private static void AppendParameter(StringBuilder buffer, DictionaryField dictionaryField)
		//{
		//	AppendDictionary(buffer, dictionaryField);
		//	buffer.Append(@",");
		//}

		//private static void AppendDictionary(StringBuilder buffer, DictionaryField dictionaryField, string fieldModifier = "")
		//{
		//	buffer.Append(@"Dictionary<long, ");
		//	buffer.Append(dictionaryField.Type);
		//	buffer.Append(@"> ");
		//	buffer.Append(fieldModifier);
		//	buffer.Append(dictionaryField.Name);
		//}

		//private static void AppendCheck(StringBuilder buffer, DictionaryField dictionaryField)
		//{
		//	AppendCheck(buffer, dictionaryField.Name);
		//}

		//private static void AppendCheck(StringBuilder buffer, string name)
		//{
		//	buffer.Append(@"if (");
		//	buffer.Append(name);
		//	buffer.Append(@" == null) throw new ArgumentNullException(""");
		//	buffer.Append(name);
		//	buffer.AppendLine(@""");");
		//}

		//private static void AppendAssignment(StringBuilder buffer, DictionaryField dictionaryField)
		//{
		//	buffer.Append(@"_");
		//	buffer.Append(dictionaryField.Name);
		//	buffer.Append(@" = ");
		//	buffer.Append(dictionaryField.Name);
		//	buffer.AppendLine(@";");
		//}

		//private static void AppendFillMethod(StringBuilder buffer, ClrClass @class, string query)
		//{
		//	buffer.Append(@"public void Fill(");
		//	buffer.Append(@"Dictionary<long, ");
		//	buffer.Append(@class.Name);
		//	buffer.AppendLine(@"> items) {");
		//	AppendCheck(buffer, @"items");
		//	buffer.AppendLine();
		//	buffer.Append(@"var query = """);
		//	buffer.Append(query);
		//	buffer.AppendLine(@""";");
		//	buffer.Append(@"QueryHelper.Fill(items, query, this.Creator");
		//	buffer.AppendLine(@", this.Selector);");
		//	buffer.AppendLine(@"}");
		//	buffer.AppendLine();
		//}

		//private static void AppendCreatorMethod(StringBuilder buffer, ClrClass @class, bool readOnly, List<DictionaryField> dictionaryFields)
		//{
		//	var name = @class.Name;
		//	buffer.Append(@"private ");
		//	buffer.Append(name);
		//	buffer.AppendLine(@" Creator(IDataReader r)");
		//	buffer.AppendLine(@"{");

		//	var properties = @class.Properties;
		//	for (var index = 0; index < properties.Length; index++)
		//	{
		//		var property = properties[index];

		//		buffer.Append(@"var ");
		//		var parameterName = ParameterNameProvider.GetParameterName(property);
		//		buffer.Append(parameterName);
		//		buffer.Append(@" = ");
		//		buffer.Append(DefaultValueProvider.GetDefaultValue(property));
		//		buffer.AppendLine(@";");
		//		buffer.Append(@"if (!r.IsDBNull(");
		//		buffer.Append(index);
		//		buffer.AppendLine(@")){");
		//		buffer.Append(parameterName);
		//		buffer.Append(@" = ");
		//		AppendDataReaderValue(buffer, property, index, dictionaryFields);
		//		buffer.AppendLine(@";}");
		//	}

		//	buffer.Append(@"return new ");
		//	buffer.Append(name);
		//	if (readOnly)
		//	{
		//		buffer.Append(@"(");
		//		foreach (var property in @class.Properties)
		//		{
		//			buffer.Append(ParameterNameProvider.GetParameterName(property));
		//			buffer.Append(',');
		//		}
		//		buffer[buffer.Length - 1] = ')';
		//	}
		//	else
		//	{
		//		buffer.Append(@"{");
		//		foreach (var property in @class.Properties)
		//		{
		//			buffer.Append(property.Name);
		//			buffer.Append(@" = ");
		//			buffer.Append(ParameterNameProvider.GetParameterName(property));
		//			buffer.Append(',');
		//		}
		//		buffer[buffer.Length - 1] = '}';
		//	}
		//	buffer.AppendLine(@";");
		//	buffer.AppendLine(@"}");
		//}

		//private static void AppendDataReaderValue(StringBuilder buffer, ClrProperty property, int index, IEnumerable<DictionaryField> dictionaryFields)
		//{
		//	if (buffer == null) throw new ArgumentNullException("buffer");
		//	if (property == null) throw new ArgumentNullException("property");

		//	var type = property.Type;
		//	if (type.IsBuiltIn)
		//	{
		//		AppendDataReaderValue(buffer, type, index);
		//		return;
		//	}
		//	buffer.Append(@"_");
		//	foreach (var dictionaryField in dictionaryFields)
		//	{
		//		if (dictionaryField.Property == property)
		//		{
		//			buffer.Append(dictionaryField.Name);
		//			break;
		//		}
		//	}
		//	buffer.Append(@"[");
		//	AppendDataReaderValue(buffer, ClrType.Integer, index);
		//	buffer.Append(@"]");
		//}

		//private static void AppendDataReaderValue(StringBuilder buffer, ClrType type, int index)
		//{
		//	buffer.Append(@"r.");
		//	buffer.Append(GetReaderMethod(type));
		//	buffer.Append(@"(");
		//	buffer.Append(index);
		//	buffer.Append(@")");
		//}

		//private static void AppendSelectorMethod(StringBuilder buffer, ClrClass @class)
		//{
		//	buffer.AppendLine();

		//	var name = @class.Name;
		//	var varName = char.ToLowerInvariant(name[0]);
		//	var primaryKeyProperty = default(ClrProperty);
		//	foreach (var property in @class.Properties)
		//	{
		//		if (property.Type == ClrType.Integer && property.Name.EndsWith(@"Id"))
		//		{
		//			primaryKeyProperty = property;
		//			break;
		//		}
		//	}

		//	buffer.Append(@"private long Selector(");
		//	buffer.Append(name);
		//	buffer.Append(@" ");
		//	buffer.Append(varName);
		//	buffer.Append(@") { return ");
		//	buffer.Append(varName);
		//	buffer.Append(@".");
		//	buffer.Append(primaryKeyProperty.Name);
		//	buffer.Append(@";}");
		//	buffer.AppendLine();
		//}

		//private static string GetReaderMethod(ClrType type)
		//{
		//	if (type == ClrType.Integer)
		//	{
		//		return @"GetInt64";
		//	}
		//	if (type == ClrType.String)
		//	{
		//		return @"GetString";
		//	}
		//	if (type == ClrType.Decimal)
		//	{
		//		return @"GetDecimal";
		//	}
		//	if (type == ClrType.DateTime)
		//	{
		//		return @"GetDateTime";
		//	}
		//	if (type == ClrType.Bytes)
		//	{
		//		return @"GetBytes";
		//	}
		//	return string.Empty;
		//}





		private static string GetIsReadOnly(bool isReadOnly)
		{
			return isReadOnly ? @"readonly" : string.Empty;
		}

		private static string GetIsSealed(bool isSealed)
		{
			return isSealed ? @"sealed" : string.Empty;
		}

		private static string GetField(ClrField field)
		{
			if (field == null) throw new ArgumentNullException("field");

			var buffer = new StringBuilder();

			buffer.Append(@"private");
			buffer.Append(Space);
			AppendReadOnly(field, buffer);
			buffer.Append(field.Type.Name);
			buffer.Append(Space);
			buffer.Append(field.Name);
			AppendInitialValue(field, buffer);
			buffer.Append(Semicolumn);

			return buffer.ToString();
		}

		private static void AppendInitialValue(ClrField field, StringBuilder buffer)
		{
			var initialValue = field.InitialValue;
			if (initialValue != string.Empty)
			{
				buffer.Append(@" = ");
				buffer.Append(initialValue);
			}
		}

		private static void AppendReadOnly(ClrField field, StringBuilder buffer)
		{
			var readOnly = GetIsReadOnly(field.IsReadOnly);
			if (readOnly != string.Empty)
			{
				buffer.Append(readOnly);
				buffer.Append(Space);
			}
		}

		private static string GetProperty(ClrProperty property, bool immutable)
		{
			if (property == null) throw new ArgumentNullException("property");

			var buffer = new StringBuilder();

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(property.Type.Name);
			buffer.Append(Space);
			buffer.Append(property.Name);
			buffer.Append(Space);
			buffer.Append(GetBackingField(immutable));

			return buffer.ToString();
		}

		private static string GetContructor(ClrContructor definition)
		{
			var buffer = new StringBuilder();

			buffer.Append(@"public");
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

		private static void AppendPropertyAssignment(StringBuilder buffer, ClrParameter parameter)
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

		private static void AppendPropertyInitialization(StringBuilder buffer, ClrProperty parameter)
		{
			buffer.Append(@"this.");
			buffer.Append(parameter.Name);
			buffer.Append(@" = ");
			buffer.Append(parameter.Type.DefaultValue);
			buffer.AppendLine(@";");
		}

		private static void AppendCheck(StringBuilder buffer, ClrParameter parameter)
		{
			var name = parameter.Name;
			buffer.Append(@"if (");
			buffer.Append(name);
			buffer.Append(@" == null) throw new ArgumentNullException(""");
			buffer.Append(name);
			buffer.Append(@""");");
			buffer.AppendLine();
		}

		private static void AppendParameter(StringBuilder buffer, ClrParameter parameter)
		{
			var type = parameter.Type.Name;
			var name = parameter.Name;

			buffer.Append(type);
			buffer.Append(Space);
			buffer.Append(name);
		}

		private static string GetBackingField(bool immutable)
		{
			return immutable ? @"{ get; private set; }" : @"{ get; set; }";
		}

		private static ClrParameter[] GetParameters(IReadOnlyList<ClrProperty> properties)
		{
			var parameters = new ClrParameter[properties.Count];
			for (var i = 0; i < properties.Count; i++)
			{
				var p = properties[i];
				parameters[i] = new ClrParameter(p.Type, p.Name);
			}
			return parameters;
		}
	}
}