using System;
using System.Collections.Generic;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Clr.Providers;
using AppBuilder.Db;
using AppBuilder.Db.Providers;

namespace AppBuilder.Generators
{
	public static class AdapterGenerator
	{
		private sealed class DictionaryField
		{
			public ClrProperty Property { get; private set; }
			public string Type { get; private set; }
			public string Name { get; private set; }

			public DictionaryField(ClrProperty property, string type, string name)
			{
				if (property == null) throw new ArgumentNullException("property");
				if (type == null) throw new ArgumentNullException("type");
				if (name == null) throw new ArgumentNullException("name");

				this.Property = property;
				this.Type = type;
				this.Name = name;
			}
		}

		public static string Generate(DbTable table, NameProvider nameProvider, ClrClass @class, bool readOnly)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (table == null) throw new ArgumentNullException("table");

			var buffer = new StringBuilder(2 * 1024);
			buffer.Append(@"public sealed class ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter : I");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter");
			buffer.AppendLine(@"{");
			var dictionaries = GetDictionaries(@class, nameProvider);
			if (dictionaries.Count > 0)
			{
				AppendConstructor(buffer, @class, dictionaries);
			}
			AppendFillMethod(buffer, @class, QueryProvider.GetSelect(table));
			AppendCreatorMethod(buffer, @class, readOnly, dictionaries);
			AppendSelectorMethod(buffer, @class);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string GenerateInterface(DbTable table, NameProvider nameProvider, ClrClass @class)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(@"public interface I", 128);

			buffer.Append(@class.Name);
			buffer.Append(@"Adapter");
			buffer.AppendLine(@"{");
			buffer.Append(@"void Fill(Dictionary<long, ");
			buffer.Append(@class.Name);
			buffer.Append(@"> ");
			buffer.Append(StringUtils.LowerFirst(nameProvider.GetDbName(@class.Name)));
			buffer.Append(@");");
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class, List<DictionaryField> dictionaryFields)
		{
			foreach (var dictionaryField in dictionaryFields)
			{
				AppendField(buffer, dictionaryField);
			}
			buffer.AppendLine();

			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter(");
			foreach (var dictionaryField in dictionaryFields)
			{
				AppendParameter(buffer, dictionaryField);
			}
			buffer[buffer.Length - 1] = ')';
			buffer.AppendLine(@"{");
			foreach (var dictionaryField in dictionaryFields)
			{
				AppendCheck(buffer, dictionaryField);
			}
			buffer.AppendLine();
			foreach (var dictionaryField in dictionaryFields)
			{
				AppendAssignment(buffer, dictionaryField);
			}
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static List<DictionaryField> GetDictionaries(ClrClass @class, NameProvider nameProvider)
		{
			var dictionaries = new List<DictionaryField>(1);
			foreach (var property in @class.Properties)
			{
				if (!property.Type.IsBuiltIn)
				{
					var type = PropertyTypeProvider.GetPropertyType(property);
					dictionaries.Add(new DictionaryField(property, type, StringUtils.LowerFirst(nameProvider.GetDbName(type))));
				}
			}
			return dictionaries;
		}

		private static void AppendField(StringBuilder buffer, DictionaryField dictionaryField)
		{
			buffer.Append(@"private readonly ");
			AppendDictionary(buffer, dictionaryField, @"_");
			buffer.AppendLine(@";");
		}

		private static void AppendParameter(StringBuilder buffer, DictionaryField dictionaryField)
		{
			AppendDictionary(buffer, dictionaryField);
			buffer.Append(@",");
		}

		private static void AppendDictionary(StringBuilder buffer, DictionaryField dictionaryField, string fieldModifier = "")
		{
			buffer.Append(@"Dictionary<long, ");
			buffer.Append(dictionaryField.Type);
			buffer.Append(@"> ");
			buffer.Append(fieldModifier);
			buffer.Append(dictionaryField.Name);
		}

		private static void AppendCheck(StringBuilder buffer, DictionaryField dictionaryField)
		{
			AppendCheck(buffer, dictionaryField.Name);
		}

		private static void AppendCheck(StringBuilder buffer, string name)
		{
			buffer.Append(@"if (");
			buffer.Append(name);
			buffer.Append(@" == null) throw new ArgumentNullException(""");
			buffer.Append(name);
			buffer.AppendLine(@""");");
		}

		private static void AppendAssignment(StringBuilder buffer, DictionaryField dictionaryField)
		{
			buffer.Append(@"_");
			buffer.Append(dictionaryField.Name);
			buffer.Append(@" = ");
			buffer.Append(dictionaryField.Name);
			buffer.AppendLine(@";");
		}

		private static void AppendFillMethod(StringBuilder buffer, ClrClass @class, string query)
		{
			buffer.Append(@"public void Fill(");
			buffer.Append(@"Dictionary<long, ");
			buffer.Append(@class.Name);
			buffer.AppendLine(@"> items) {");
			AppendCheck(buffer, @"items");
			buffer.AppendLine();
			buffer.Append(@"var query = """);
			buffer.Append(query);
			buffer.AppendLine(@""";");
			buffer.Append(@"QueryHelper.Fill(items, query, this.Creator");
			buffer.AppendLine(@", this.Selector);");
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendCreatorMethod(StringBuilder buffer, ClrClass @class, bool readOnly, List<DictionaryField> dictionaryFields)
		{
			var name = @class.Name;
			buffer.Append(@"private ");
			buffer.Append(name);
			buffer.AppendLine(@" Creator(IDataReader r)");
			buffer.AppendLine(@"{");

			var properties = @class.Properties;
			for (var index = 0; index < properties.Length; index++)
			{
				var property = properties[index];

				buffer.Append(@"var ");
				var parameterName = ParameterNameProvider.GetParameterName(property);
				buffer.Append(parameterName);
				buffer.Append(@" = ");
				buffer.Append(DefaultValueProvider.GetDefaultValue(property));
				buffer.AppendLine(@";");
				buffer.Append(@"if (!r.IsDBNull(");
				buffer.Append(index);
				buffer.AppendLine(@")){");
				buffer.Append(parameterName);
				buffer.Append(@" = ");
				AppendDataReaderValue(buffer, property, index, dictionaryFields);
				buffer.AppendLine(@";}");
			}

			buffer.Append(@"return new ");
			buffer.Append(name);
			if (readOnly)
			{
				buffer.Append(@"(");
				foreach (var property in @class.Properties)
				{
					buffer.Append(ParameterNameProvider.GetParameterName(property));
					buffer.Append(',');
				}
				buffer[buffer.Length - 1] = ')';
			}
			else
			{
				buffer.Append(@"{");
				foreach (var property in @class.Properties)
				{
					buffer.Append(property.Name);
					buffer.Append(@" = ");
					buffer.Append(ParameterNameProvider.GetParameterName(property));
					buffer.Append(',');
				}
				buffer[buffer.Length - 1] = '}';
			}
			buffer.AppendLine(@";");
			buffer.AppendLine(@"}");
		}

		private static void AppendDataReaderValue(StringBuilder buffer, ClrProperty property, int index, IEnumerable<DictionaryField> dictionaryFields)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			var type = property.Type;
			if (type.IsBuiltIn)
			{
				AppendDataReaderValue(buffer, type, index);
				return;
			}
			buffer.Append(@"_");
			foreach (var dictionaryField in dictionaryFields)
			{
				if (dictionaryField.Property == property)
				{
					buffer.Append(dictionaryField.Name);
					break;
				}
			}
			buffer.Append(@"[");
			AppendDataReaderValue(buffer, ClrType.Integer, index);
			buffer.Append(@"]");
		}

		private static void AppendDataReaderValue(StringBuilder buffer, ClrType type, int index)
		{
			buffer.Append(@"r.");
			buffer.Append(GetReaderMethod(type));
			buffer.Append(@"(");
			buffer.Append(index);
			buffer.Append(@")");
		}

		private static void AppendSelectorMethod(StringBuilder buffer, ClrClass @class)
		{
			buffer.AppendLine();

			var name = @class.Name;
			var varName = char.ToLowerInvariant(name[0]);
			var primaryKeyProperty = default(ClrProperty);
			foreach (var property in @class.Properties)
			{
				if (property.Type == ClrType.Integer && property.Name.EndsWith(@"Id"))
				{
					primaryKeyProperty = property;
					break;
				}
			}

			buffer.Append(@"private long Selector(");
			buffer.Append(name);
			buffer.Append(@" ");
			buffer.Append(varName);
			buffer.Append(@") { return ");
			buffer.Append(varName);
			buffer.Append(@".");
			buffer.Append(primaryKeyProperty.Name);
			buffer.Append(@";}");
			buffer.AppendLine();
		}

		private static string GetReaderMethod(ClrType type)
		{
			if (type == ClrType.Integer)
			{
				return @"GetInt64";
			}
			if (type == ClrType.String)
			{
				return @"GetString";
			}
			if (type == ClrType.Decimal)
			{
				return @"GetDecimal";
			}
			if (type == ClrType.DateTime)
			{
				return @"GetDateTime";
			}
			if (type == ClrType.Bytes)
			{
				return @"GetBytes";
			}
			return string.Empty;
		}
	}
}