using System;
using System.Collections.Generic;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public sealed class AdapterField
	{
		public string Type { get; private set; }
		public string Name { get; private set; }

		public AdapterField(string type, string name)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.Type = type;
			this.Name = name;
		}
	}

	public static class Stable
	{
		public static DbTable FindTableByForeignKey(DbSchema schema, DbForeignKey foreignKey)
		{
			if (schema == null) throw new ArgumentNullException("schema");
			if (foreignKey == null) throw new ArgumentNullException("foreignKey");

			var foreignKeyTableName = foreignKey.Table;

			foreach (var table in schema.Tables)
			{
				if (table.Name == foreignKeyTableName)
				{
					return table;
				}
			}

			return null;
		}

		public static AdapterField FindFieldByType(AdapterField[] fields, ClrType type)
		{
			if (fields == null) throw new ArgumentNullException("fields");
			if (type == null) throw new ArgumentNullException("type");

			var typeName = type.Name;

			foreach (var field in fields)
			{
				if (field.Type == typeName)
				{
					return field;
				}
			}

			return null;
		}

		public static int GetForeignKeysCount(IEnumerable<DbColumn> columns)
		{
			if (columns == null) throw new ArgumentNullException("columns");

			var count = 0;

			foreach (var column in columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					count++;
				}
			}

			return count;
		}

		public static DbTable[] GetForeignKeyTables(DbTable table, DbSchema schema)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (schema == null) throw new ArgumentNullException("schema");

			var foreignKeyTables = new DbTable[GetForeignKeysCount(table.Columns)];

			var index = 0;
			foreach (var column in table.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					foreignKeyTables[index++] = FindTableByForeignKey(schema, foreignKey);
				}
			}

			return RemoveCollectionTypeForeignKeys(foreignKeyTables, schema);
		}

		private static DbTable[] RemoveCollectionTypeForeignKeys(DbTable[] foreignKeyTables, DbSchema schema)
		{
			foreach (var table in foreignKeyTables)
			{
				var collectionType = GetCollectionType(DbTableConverter.ToClrClass(table, schema.Tables).Properties);
				if (collectionType != null)
				{
					var tables = new DbTable[foreignKeyTables.Length - 1];

					var index = 0;
					foreach (var current in foreignKeyTables)
					{
						if (current != table)
						{
							tables[index++] = current;
						}
					}

					return tables;
				}
			}

			return foreignKeyTables;
		}

		public static ClrType GetCollectionType(IEnumerable<ClrProperty> properties)
		{
			if (properties == null) throw new ArgumentNullException("properties");

			foreach (var property in properties)
			{
				var type = property.Type;
				if (type.IsCollection)
				{
					return type;
				}
			}

			return null;
		}

		public static string GetSelector(string className)
		{
			if (className == null) throw new ArgumentNullException("className");

			return string.Format(@"private long Selector({0} {1}) {{ return {1}.Id; }}", className, char.ToLowerInvariant(className[0]));
		}

		public static string GetFill(string className, DbTable table)
		{
			if (className == null) throw new ArgumentNullException("className");
			if (table == null) throw new ArgumentNullException("table");

			var template = @"public void Fill(Dictionary<long, {0}> {1})
	{{
		if ({1} == null) throw new ArgumentNullException(""{1}"");

		var query = @""{2}"";

		QueryHelper.Fill({1}, query, this.{0}Creator, this.Selector);
	}}";
			return string.Format(template,
				className,
				NameProvider.ToParameterName(table.Name),
				QueryCreator.GetSelect(table).Statement);
		}

		public static string GetGetAll(string className, DbTable table)
		{
			if (className == null) throw new ArgumentNullException("className");
			if (table == null) throw new ArgumentNullException("table");

			var template = @"public List<{0}> GetAll()
	{{
		var query = @""{1}"";

		return QueryHelper.Get(query, this.{0}Creator);
	}}";
			return string.Format(template,
				className,
				QueryCreator.GetSelect(table).Statement);
		}

		public static string GetAdapterReadonOnly(ClrClass @class, DbTable table, DbTable[] foreignKeyTables)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (table == null) throw new ArgumentNullException("table");
			if (foreignKeyTables == null) throw new ArgumentNullException("foreignKeyTables");

			var fields = GetDictionaryFields(foreignKeyTables);
			var fill = GetFill(@class.Name, table);
			var creator = GetCreatorForReadOnlyTable(@class, fields);
			var selector = GetSelector(@class.Name);

			if (foreignKeyTables.Length > 0)
			{
				var template = @"public sealed class {0}Adapter
{{
	{4}

	{5}

	{1}

	{2}

	{3}
}}";


				var fieldDefinitions = GetFieldDefinitions(fields);
				var constructor = GetConstructor(table, fields);

				return string.Format(template, table.Name, fill, creator, selector, fieldDefinitions, constructor);
			}
			else
			{
				var template = @"public sealed class {0}Adapter
{{
	{1}

	{2}

	{3}
}}";
				return string.Format(template, table.Name, fill, creator, selector);
			}
		}

		public static string GetAdapter(ClrClass @class, DbTable table, DbTable[] foreignKeyTables)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (table == null) throw new ArgumentNullException("table");
			if (foreignKeyTables == null) throw new ArgumentNullException("foreignKeyTables");

			if (@class == null) throw new ArgumentNullException("class");
			if (table == null) throw new ArgumentNullException("table");
			if (foreignKeyTables == null) throw new ArgumentNullException("foreignKeyTables");

			var fields = GetDictionaryFields(foreignKeyTables);
			var fill = GetGetAll(@class.Name, table);
			var creator = GetCreatorForNormalTable(@class, fields);

			if (foreignKeyTables.Length > 0)
			{
				var template = @"public sealed class {0}Adapter
{{
	{3}

	{4}

	{1}

	{2}
}}";


				var fieldDefinitions = GetFieldDefinitions(fields);
				var constructor = GetConstructor(table, fields);

				return string.Format(template, table.Name, fill, creator, fieldDefinitions, constructor);
			}
			else
			{
				var template = @"public sealed class {0}Adapter
{{
	{1}

	{2}

	{3}
}}";
				return string.Format(template, table.Name, fill, creator, "TODO:!!!");
			}
		}

		public static string GetAdapterWithCollection(ClrClass @class, DbTable table, DbTable[] foreignKeyTables)
		{
			throw new NotImplementedException();
		}


		public static string GetConstructor(DbTable table, AdapterField[] fields)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (fields == null) throw new ArgumentNullException("fields");

			var template = @"public {0}Adapter({1})
	{{
		{2}

		{3}
	}}";
			var parameterValues = new string[fields.Length];
			var checkValues = new string[fields.Length];
			var assignementValues = new string[fields.Length];

			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				var type = field.Type;
				var name = field.Name;
				parameterValues[i] = string.Format(@"Dictionary<long, {0}> ", type) + name;
				checkValues[i] = string.Format(@"if ({0} == null) throw new ArgumentNullException(""{0}"");", name);
				assignementValues[i] = @"_" + name + @" = " + name + @";";
			}

			var parameters = string.Join(@", ", parameterValues);
			var checks = string.Join(Environment.NewLine + "\t\t", checkValues);
			var assignments = string.Join(Environment.NewLine + "\t\t", assignementValues);

			return string.Format(template, table.Name, parameters, checks, assignments);
		}

		public static string GetFieldDefinitions(AdapterField[] fields)
		{
			if (fields == null) throw new ArgumentNullException("fields");

			var values = new string[fields.Length];

			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				values[i] = string.Format("private readonly Dictionary<long, {0}> _{1};", field.Type, field.Name);
			}

			return string.Join(Environment.NewLine + "\t", values);
		}

		public static AdapterField[] GetDictionaryFields(DbTable[] tables)
		{
			if (tables == null) throw new ArgumentNullException("tables");

			var fields = new AdapterField[tables.Length];

			for (var index = 0; index < tables.Length; index++)
			{
				var table = tables[index];
				fields[index] = new AdapterField(table.ClassName, NameProvider.ToParameterName(table.Name));
			}

			return fields;
		}

		public static string[] GetParameterNames(ClrProperty[] properties)
		{
			var names = new string[properties.Length];

			for (var i = 0; i < properties.Length; i++)
			{
				names[i] = NameProvider.ToParameterName(properties[i].Name);
			}

			return names;
		}

		public static string GetCreatorForReadOnlyTable(ClrClass @class, AdapterField[] fields)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (fields == null) throw new ArgumentNullException("fields");

			var template = @"private {0} {0}Creator(IDataReader r)
	{{
		{1}
	
		return new {0}({2});
	}}";
			var properties = @class.Properties;

			var names = GetParameterNames(properties);
			var values = new string[properties.Length];

			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];

				string value;
				var type = property.Type;
				if (type.IsBuiltIn)
				{
					value = GetReadValue(names[i], property.Type, i);
				}
				else
				{
					value = GetReadValueWithMapping(names[i], property.Type, i, FindFieldByType(fields, property.Type));
				}

				values[i] = value;
			}

			return string.Format(template, @class.Name,
				string.Join(Environment.NewLine + "\t\t", values),
				string.Join(@", ", names));
		}

		public static string GetCreatorForNormalTable(ClrClass @class, AdapterField[] fields)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (fields == null) throw new ArgumentNullException("fields");

			var template = @"private {0} {0}Creator(IDataReader r{3})
	{{
		{1}
	
		return new {0}({2});
	}}";
			var properties = @class.Properties;

			var valueIndex = 0;
			var names = GetParameterNames(properties);
			var values = new string[properties.Length];

			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];

				var value = string.Empty;
				var type = property.Type;
				if (type.IsBuiltIn)
				{
					value = GetReadValue(names[i], property.Type, valueIndex++);
				}
				else
				{
					var field = FindFieldByType(fields, property.Type);
					if (field != null)
					{
						value = GetReadValueWithMapping(names[i], property.Type, valueIndex++, field);
					}
				}
				values[i] = value;
			}

			values = GetValuesWithoutEmpties(values);

			return string.Format(template, @class.Name,
				string.Join(Environment.NewLine + "\t\t", values),
				string.Join(@", ", names));
		}

		public static string[] GetValuesWithoutEmpties(string[] values)
		{
			if (values == null) throw new ArgumentNullException("values");

			foreach (var value in values)
			{
				if (value == string.Empty)
				{
					var withoutEmpties = new string[values.Length - 1];

					var index = 0;
					foreach (var innerValue in values)
					{
						if (innerValue != string.Empty)
						{
							withoutEmpties[index++] = innerValue;
						}
					}

					return withoutEmpties;
				}
			}

			return values;
		}

		public static string GetReadValue(string varName, ClrType type, int index)
		{
			if (varName == null) throw new ArgumentNullException("varName");
			if (type == null) throw new ArgumentNullException("type");

			return string.Format(@"
		var {0} = {1};
		if (!r.IsDBNull({2}))
		{{
			{0} = r.{3}({2});
		}}", varName, type.DefaultValue, index, type.ReaderMethod);
		}

		public static string GetReadValueWithMapping(string varName, ClrType type, int index, AdapterField field)
		{
			if (varName == null) throw new ArgumentNullException("varName");
			if (type == null) throw new ArgumentNullException("type");
			if (field == null) throw new ArgumentNullException("field");

			return string.Format(@"
		var {0} = {1};
		if (!r.IsDBNull({2}))
		{{
			{0} = _{4}[r.{3}({2})];
		}}", varName, type.DefaultValue, index, type.ReaderMethod, field.Name);
		}
	}

	public static class AdapterGenerator
	{
		private static readonly char Space = ' ';
		private static readonly char Semicolon = ';';
		private static readonly char Comma = ',';
		private static readonly char Tab = '\t';

		public static string GenerateCode(ClrClass @class, DbTable table, DbSchema schema)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (table == null) throw new ArgumentNullException("table");
			if (schema == null) throw new ArgumentNullException("schema");

			var foreignKeyTables = Stable.GetForeignKeyTables(table, schema);
			var collectionType = Stable.GetCollectionType(@class.Properties);

			if (table.IsReadOnly)
			{
				return Stable.GetAdapterReadonOnly(@class, table, foreignKeyTables);
			}
			if (collectionType == null)
			{
				return Stable.GetAdapter(@class, table, foreignKeyTables);
			}
			return Stable.GetAdapterWithCollection(@class, table, foreignKeyTables);
		}

		private static void AppendIdReaderMethod(StringBuilder buffer)
		{
			buffer.AppendLine();
			buffer.Append(Tab);
			buffer.AppendFormat(@"private long IdReader(IDataReader r) {{ return r.GetInt64(0); }}");
			buffer.AppendLine();
		}

		private static void AppendFields(StringBuilder buffer, DbTable[] tables)
		{
			foreach (var table in tables)
			{
				buffer.Append(Tab);
				buffer.AppendFormat(@"private readonly Dictionary<long, {0}> _{1};", table.ClassName, NameProvider.ToParameterName(table.Name));
				buffer.AppendLine();
			}
		}

		private static void AppendContructor(StringBuilder buffer, string tableName, DbTable[] tables)
		{
			buffer.Append(Tab);
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(tableName);
			buffer.Append(@"Adapter");
			buffer.Append(@"(");

			var parameterNames = new string[tables.Length];
			for (var i = 0; i < tables.Length; i++)
			{
				var table = tables[i];
				if (i > 0)
				{
					buffer.Append(Comma);
					buffer.Append(Space);
				}
				var name = parameterNames[i] = NameProvider.ToParameterName(table.Name);
				buffer.AppendFormat(@"Dictionary<long, {0}> {1}", table.ClassName, name);
			}
			buffer.AppendLine(@")");
			buffer.Append(Tab);
			buffer.AppendLine(@"{");

			foreach (var name in parameterNames)
			{
				//AppendParameterCheck(buffer, name);
			}

			buffer.AppendLine();

			foreach (var name in parameterNames)
			{
				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.Append('_');
				buffer.Append(name);
				buffer.Append(@" = ");
				buffer.Append(name);
				buffer.Append(Semicolon);
				buffer.AppendLine();
			}

			buffer.Append(Tab);
			buffer.AppendLine(@"}");
		}

		private static void AppendCreatorMethod(StringBuilder buffer, ClrClass @class, DbTable[] foreignKeyTables, int indexOffset = 0)
		{
			buffer.AppendLine();
			buffer.Append(Tab);
			buffer.AppendFormat(@"private {0} {0}Creator(IDataReader r)", @class.Name);
			buffer.AppendLine();

			buffer.Append(Tab);
			buffer.AppendLine(@"{");

			var parameters = GetParameters(@class);

			//AppendParameters(buffer, @class.Name, parameters);

			buffer.Append(Tab);
			buffer.AppendLine(@"}");
		}

		private static Tuple<ClrType, string>[] GetParameters(ClrClass @class)
		{
			var index = 0;
			var properties = @class.Properties;
			var parameters = new Tuple<ClrType, string>[properties.Length];

			foreach (var property in properties)
			{
				parameters[index++] = Tuple.Create(property.Type, NameProvider.ToParameterName(property.Name));
			}

			return parameters;
		}

		private static void AppendParameters(StringBuilder buffer, string className, string[] parameters)
		{
			buffer.Append(Tab);
			buffer.Append(Tab);
			buffer.Append(@"return new");
			buffer.Append(Space);
			buffer.Append(className);
			buffer.Append('(');
			for (var i = 0; i < parameters.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(Comma);
					buffer.Append(Space);
				}
				buffer.Append(parameters[i]);
			}
			buffer.Append(')');
			buffer.Append(Semicolon);
			buffer.AppendLine();
		}
	}
}