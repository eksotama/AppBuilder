using System;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class AdapterGenerator
	{
		private static readonly char Space = ' ';
		private static readonly char Semicolon = ';';
		private static readonly char Comma = ',';
		private static readonly char Tab = '\t';

		public static string GenerateCode(ClrClass @class, DbTable table, DbTable[] tables)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (table == null) throw new ArgumentNullException("table");
			if (tables == null) throw new ArgumentNullException("tables");

			var className = @class.Name;
			var tableName = table.Name;
			var buffer = new StringBuilder(1024);

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"sealed");
			buffer.Append(Space);
			buffer.Append(@"class");
			buffer.Append(Space);
			buffer.Append(tableName);
			buffer.Append(@"Adapter");
			buffer.AppendLine();

			buffer.AppendLine(@"{");

			if (table.IsReadOnly)
			{
				var foreignKeyTables = GetForeignKeyTables(table, tables);
				if (foreignKeyTables.Length > 0)
				{
					AppendFields(buffer, foreignKeyTables);
					buffer.AppendLine();
					AppendContructor(buffer, tableName, foreignKeyTables);
					buffer.AppendLine();
				}
				AppendFillMethod(buffer, className, table);
				AppendCreatorMethod(buffer, className, @class, foreignKeyTables);
				AppendSelectMethod(buffer, className);
			}
			else
			{
				// TODO  :!!!
				buffer.AppendLine(@"//" + QueryCreator.GetSelect(table).Statement);
			}

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendFields(StringBuilder buffer, DbTable[] tables)
		{
			foreach (var table in tables)
			{
				buffer.Append(Tab);
				buffer.AppendFormat(@"private readonly Dictionary<long, {0}> _{1};", table.ClassName, NameProvider.ToParamterName(table.Name));
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
				var name = parameterNames[i] = NameProvider.ToParamterName(table.Name);
				buffer.AppendFormat(@"Dictionary<long, {0}> {1}", table.ClassName, name);
			}
			buffer.AppendLine(@")");
			buffer.Append(Tab);
			buffer.AppendLine(@"{");

			foreach (var name in parameterNames)
			{
				AppendParameterCheck(buffer, name);
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

		private static void AppendParameterCheck(StringBuilder buffer, string name)
		{
			buffer.Append(Tab);
			buffer.Append(Tab);
			buffer.AppendFormat(@"if ({0} == null) throw new ArgumentNullException(""{0}"");", name);
			buffer.AppendLine();
		}

		private static DbTable[] GetForeignKeyTables(DbTable table, DbTable[] tables)
		{
			var total = 0;
			foreach (var column in table.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					total++;
				}
			}

			var index = 0;
			var foreignKeyTables = new DbTable[total];

			foreach (var column in table.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					foreignKeyTables[index++] = FindTableByName(tables, foreignKey.Table);
				}
			}

			return foreignKeyTables;
		}

		private static DbTable FindTableByName(DbTable[] tables, string name)
		{
			foreach (var t in tables)
			{
				if (t.Name == name)
				{
					return t;
				}
			}
			return null;
		}

		private static void AppendFillMethod(StringBuilder buffer, string className, DbTable table)
		{
			var parameterName = NameProvider.ToParamterName(table.Name);

			buffer.Append(Tab);
			buffer.AppendFormat(@"public void Fill(Dictionary<long, {0}> {1})", className, parameterName);
			buffer.AppendLine();

			buffer.Append(Tab);
			buffer.AppendLine(@"{");

			AppendParameterCheck(buffer, parameterName);
			buffer.AppendLine();

			buffer.Append(Tab);
			buffer.Append(Tab);
			buffer.AppendFormat(@"var query = @""{0}"";", QueryCreator.GetSelect(table).Statement);
			buffer.AppendLine();
			buffer.AppendLine();

			buffer.Append(Tab);
			buffer.Append(Tab);
			buffer.AppendFormat(@"QueryHelper.Fill({0}, query, this.Creator, this.Selector);", parameterName);
			buffer.AppendLine();

			buffer.Append(Tab);
			buffer.AppendLine(@"}");
		}

		private static void AppendCreatorMethod(StringBuilder buffer, string className, ClrClass @class, DbTable[] foreignKeyTables)
		{
			buffer.AppendLine();
			buffer.Append(Tab);
			buffer.AppendFormat(@"private {0} Creator(IDataReader r)", className);
			buffer.AppendLine();

			buffer.Append(Tab);
			buffer.AppendLine(@"{");

			var properties = @class.Properties;
			var parameters = new string[properties.Length];
			for (var index = 0; index < properties.Length; index++)
			{
				var property = properties[index];
				var type = property.Type;
				var name = parameters[index] = NameProvider.ToParamterName(property.Name);

				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.Append(@"var");
				buffer.Append(Space);
				buffer.Append(name);
				buffer.Append(@" = ");

				buffer.Append(type.DefaultValue);
				buffer.Append(Semicolon);
				buffer.AppendLine();

				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.AppendFormat(@"if (!r.IsDBNull({0}))", index);
				buffer.AppendLine();

				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.AppendLine(@"{");

				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.Append(name);
				buffer.Append(@" = ");
				if (!type.IsBuiltIn)
				{
					buffer.Append('_');
					foreach (var table in foreignKeyTables)
					{
						if (table.ClassName == type.Name)
						{
							buffer.Append(NameProvider.ToParamterName(table.Name));
							break;
						}
					}
					buffer.Append('[');
				}
				buffer.Append(@"r.");
				buffer.Append(type.ReaderMethod);
				buffer.Append('(');
				buffer.Append(index);
				buffer.Append(')');
				if (!type.IsBuiltIn)
				{
					buffer.Append(']');
				}
				buffer.Append(Semicolon);
				buffer.AppendLine();

				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.AppendLine(@"}");
			}

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

			buffer.Append(Tab);
			buffer.AppendLine(@"}");
		}

		private static void AppendSelectMethod(StringBuilder buffer, string className)
		{
			buffer.AppendLine();
			buffer.Append(Tab);
			buffer.AppendFormat(@"private long Selector({0} {1}) {{ return {1}.Id; }}", className, char.ToLowerInvariant(className[0]));
			buffer.AppendLine();
		}
	}
}