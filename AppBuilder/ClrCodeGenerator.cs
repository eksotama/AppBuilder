using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class ClrCodeGenerator
	{
		private static readonly char Space = ' ';
		private static readonly char Comma = ',';
		private static readonly char Semicolumn = ';';

		public static string GetClassCode(ClrClass @class)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(1024);

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"sealed");
			buffer.Append(Space);
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

			var properties = @class.Properties;
			if (properties.Length > 0)
			{
				foreach (var property in properties)
				{
					buffer.AppendLine(GetProperty(property));
				}
				buffer.AppendLine();
			}

			buffer.Append(GetContructor(new ClrContructor(@class.Name, GetParameters(properties))));
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string GetHelperCode(ClrClass @class, NameProvider nameProvider)
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

		public static string GetAdapterInterface(ClrClass @class, NameProvider nameProvider)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");

			var buffer = new StringBuilder(@"public interface I", 128);

			buffer.Append(@class.Name);
			buffer.Append(@"Adapter");
			buffer.AppendLine(@"{");
			if (@class.Properties.All(p => p.Type.IsBuiltIn))
			{
				buffer.Append(@"void Fill(");
				buffer.Append(@"Dictionary<long, ");
				buffer.Append(@class.Name);
				buffer.Append(@"> ");
				buffer.Append(StringUtils.LowerFirst(nameProvider.GetDbName(@class.Name)));
				buffer.Append(@");");
			}
			else
			{
				buffer.Append(@"List<");
				buffer.Append(@class.Name);
				buffer.Append(@"> ");
				buffer.Append(@"GetAll();");
			}

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string GetAdapter(ClrClass @class, NameProvider nameProvider, DbTable table)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (table == null) throw new ArgumentNullException("table");

			var buffer = new StringBuilder(2 * 1024);
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"sealed class");
			buffer.Append(Space);
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter");
			buffer.Append(Space);
			// TODO : TEMP disable
			//buffer.Append(':');
			//buffer.Append(Space);
			//buffer.Append('I');
			//buffer.Append(@class.Name);
			//buffer.Append(@"Adapter");
			buffer.AppendLine(@"{");
			var fields = GetFileds(@class, nameProvider);
			if (fields.Length > 0)
			{
				AppendConstructor(buffer, @class, fields);
				//AppendGetMethod(buffer, @class, QueryProvider.GetSelect(table));
			}
			else
			{
				//AppendFillMethod(buffer, @class, QueryProvider.GetSelect(table));
			}
			AppendCreatorMethod(buffer, @class, fields);
			if (fields.Length == 0)
			{
				AppendSelectorMethod(buffer, @class);
			}
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static ClrField[] GetFileds(ClrClass @class, NameProvider nameProvider)
		{
			var fields = new List<ClrField>();

			foreach (var property in @class.Properties)
			{
				var type = property.Type;
				if (!type.IsBuiltIn && !type.IsCollection)
				{
					fields.Add(new ClrField(type, nameProvider.GetDbName(type.Name), property: property));
				}
			}

			return fields.ToArray();
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class, ClrField[] fields)
		{
			foreach (var field in fields)
			{
				AppendField(buffer, field);
			}
			buffer.AppendLine();

			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter(");
			var parameters = new ClrParameter[fields.Length];
			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				parameters[i] = new ClrParameter(field.Type, field.Name.Substring(1));
			}
			foreach (var parameter in parameters)
			{
				AppendDictionaryParameter(buffer, parameter);
			}
			buffer[buffer.Length - 1] = ')';
			buffer.AppendLine(@"{");
			foreach (var parameter in parameters)
			{
				AppendCheck(buffer, parameter.Name);
			}
			buffer.AppendLine();
			for (var i = 0; i < fields.Length; i++)
			{
				AppendFieldAssignment(buffer, fields[i], parameters[i]);
			}
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendField(StringBuilder buffer, ClrField field)
		{
			buffer.Append(@"private readonly");
			buffer.Append(Space);
			AppendDictionary(buffer, field.Type, field.Name);
			buffer.Append(Semicolumn);
			buffer.AppendLine();
		}

		private static void AppendDictionary(StringBuilder buffer, ClrType type, string name)
		{
			buffer.Append(@"Dictionary<long, ");
			buffer.Append(type.Name);
			buffer.Append(@"> ");
			buffer.Append(name);
		}

		private static void AppendDictionaryParameter(StringBuilder buffer, ClrParameter parameter)
		{
			AppendDictionary(buffer, parameter.Type, parameter.Name);
			buffer.Append(Comma);
		}

		private static void AppendFieldAssignment(StringBuilder buffer, ClrField field, ClrParameter parameter)
		{
			buffer.Append(field.Name);
			buffer.Append(@" = ");
			buffer.Append(parameter.Name);
			buffer.Append(Semicolumn);
			buffer.AppendLine();
		}

		private static void AppendFillMethod(StringBuilder buffer, ClrClass @class, string query)
		{
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"void Fill(");
			buffer.Append(@"Dictionary<long, ");
			buffer.Append(@class.Name);
			buffer.Append(@"> items)");
			buffer.AppendLine(@"{");
			AppendCheck(buffer, @"items");
			buffer.AppendLine();
			buffer.Append(@"var query = @""");
			buffer.Append(query);
			buffer.AppendLine(@""";");
			buffer.AppendLine(@"QueryHelper.Fill(items, query, this.Creator, this.Selector);");
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendGetMethod(StringBuilder buffer, ClrClass @class, string query)
		{
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"List<");
			buffer.Append(@class.Name);
			buffer.Append(@"> GetAll()");
			buffer.AppendLine(@"{");
			buffer.Append(@"var query = @""");
			buffer.Append(query);
			buffer.AppendLine(@""";");
			buffer.AppendLine(@"return QueryHelper.Get(query, this.Creator);");
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendCreatorMethod(StringBuilder buffer, ClrClass @class, ClrField[] fields)
		{
			var name = @class.Name;
			buffer.Append(@"private");
			buffer.Append(Space);
			buffer.Append(name);
			buffer.AppendLine(@" Creator(IDataReader r)");
			buffer.AppendLine(@"{");

			var properties = @class.Properties;
			var parameters = GetParameters(properties);
			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				var parameter = parameters[i];

				buffer.Append(@"var ");
				buffer.Append(parameter.Name);
				buffer.Append(@" = ");
				buffer.Append(property.Type.DefaultValue);
				buffer.Append(Semicolumn);
				buffer.AppendLine();
				buffer.Append(@"if (!r.IsDBNull(");
				buffer.Append(i);
				buffer.Append(@")");
				buffer.Append(@")");
				buffer.AppendLine(@"{");
				buffer.Append(parameter.Name);
				buffer.Append(@" = ");
				AppendReaderGetValue(buffer, property, i, fields);
				buffer.Append(Semicolumn);
				buffer.AppendLine(@"}");
			}

			buffer.Append(@"return new ");
			buffer.Append(name);
			buffer.Append(@"(");
			foreach (var parameter in parameters)
			{
				buffer.Append(parameter.Name);
				buffer.Append(Comma);
			}
			buffer[buffer.Length - 1] = ')';
			buffer.AppendLine(@";");
			buffer.AppendLine(@"}");
		}

		private static void AppendReaderGetValue(StringBuilder buffer, ClrProperty property, int index, IEnumerable<ClrField> fields)
		{
			var type = property.Type;
			if (type.IsBuiltIn)
			{
				AppendReaderGetValue(buffer, type, index);
			}
			else
			{
				// TODO : !!!
				foreach (var field in fields)
				{
					if (field.Property == property)
					{
						buffer.Append(field.Name);
						break;
					}
				}
				buffer.Append(@"[");
				AppendReaderGetValue(buffer, ClrType.Long, index);
				buffer.Append(@"]");
			}
		}

		private static void AppendReaderGetValue(StringBuilder buffer, ClrType type, int index)
		{
			buffer.Append(@"r.");
			buffer.Append(type.ReaderMethod);
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
				if (property.Type == ClrType.Long && property.Name.EndsWith(@"Id"))
				{
					primaryKeyProperty = property;
					break;
				}
			}

			buffer.Append(@"private long Selector(");
			buffer.Append(name);
			buffer.Append(Space);
			buffer.Append(varName);
			buffer.Append(@") { return ");
			buffer.Append(varName);
			buffer.Append(@".");
			buffer.Append(primaryKeyProperty.Name);
			buffer.Append(@";}");
			buffer.AppendLine();
		}

		private static string GetProperty(ClrProperty property)
		{
			if (property == null) throw new ArgumentNullException("property");

			var buffer = new StringBuilder();

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(property.Type.Name);
			buffer.Append(Space);
			buffer.Append(property.Name);
			buffer.Append(Space);
			buffer.Append(GetBackingField());

			return buffer.ToString();
		}

		private static string GetContructor(ClrContructor contructor)
		{
			var buffer = new StringBuilder();

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(contructor.Name);
			buffer.Append('(');
			var addSeparator = false;
			foreach (var parameter in contructor.Parameters)
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
			foreach (var parameter in contructor.Parameters)
			{
				if (parameter.Type.CheckValue)
				{
					AppendCheck(buffer, parameter);
					hasChecks = true;
				}
			}
			if (hasChecks)
			{
				buffer.AppendLine();
			}
			foreach (var parameter in contructor.Parameters)
			{
				AppendPropertyAssignment(buffer, parameter);
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

		private static void AppendCheck(StringBuilder buffer, ClrParameter parameter)
		{
			AppendCheck(buffer, parameter.Name);
		}

		private static void AppendCheck(StringBuilder buffer, string name)
		{
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

		private static string GetBackingField()
		{
			return @"{ get; private set; }";
		}

		private static ClrParameter[] GetParameters(IReadOnlyList<ClrProperty> properties)
		{
			var parameters = new ClrParameter[properties.Count];
			for (var i = 0; i < properties.Count; i++)
			{
				var property = properties[i];
				parameters[i] = new ClrParameter(property.Type, property.Name);
			}
			return parameters;
		}
	}
}