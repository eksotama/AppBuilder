using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class DbSchemaParser
	{
		private static readonly string[] SeparatorArray = { @";" + Environment.NewLine };

		public static DbSchema Parse(string script)
		{
			if (script == null) throw new ArgumentNullException("script");

			var tables = script
				.Split(SeparatorArray, StringSplitOptions.RemoveEmptyEntries)
				.Select(ParseTable)
				.ToArray();
			return new DbSchema(string.Empty, tables);
		}

		public static string GenerateScript(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			var buffer = new StringBuilder(1024);

			foreach (var table in schema.Tables)
			{
				AppendCreateTableScript(buffer, table);
				buffer.Append(SeparatorArray[0]);
			}

			return buffer.ToString();
		}

		private static DbTable ParseTable(string script)
		{
			var lines = StringUtils.RemoveBrackets(script)
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			var tableName = StringUtils.ExtractBetween(lines[0], @"CREATE TABLE", Environment.NewLine).Trim();

			var columns = new List<DbColumn>();
			for (var index = 2; index < lines.Length - 1; index++)
			{
				var value = lines[index].Trim();
				var columnName = StringUtils.ExtractBetween(value, @"FOREIGN KEY", @")");
				if (columnName != string.Empty)
				{
					columnName = columnName.Trim().Substring(1);

					var foreignKey = ParseForeignKey(value);
					for (var i = 0; i < columns.Count; i++)
					{
						var column = columns[i];
						if (column.Name == columnName)
						{
							columns[i] = new DbColumn(column.Type, column.Name, foreignKey, column.AllowNull, column.IsPrimaryKey);
							break;
						}
					}
				}
				else
				{
					columns.Add(ParseColumn(value));
				}
			}

			return new DbTable(tableName, columns.ToArray());
		}

		private static DbForeignKey ParseForeignKey(string input)
		{
			var value = StringUtils.ExtractBetween(input, @"REFERENCES ", @")");
			var index = value.IndexOf('(');
			var table = value.Substring(0, index).Trim();
			var column = value.Substring(index + 1).Trim();
			return new DbForeignKey(StringUtils.UpperFirst(table), StringUtils.UpperFirst(column));
		}

		private static DbColumn ParseColumn(string input)
		{
			var name = input.Substring(0, input.IndexOf(' '));
			var type = DbColumnType.Parse(StringUtils.ExtractBetween(input, @" ", @" "));
			var allowNull = input.IndexOf(@"NOT NULL", StringComparison.OrdinalIgnoreCase) < 0;
			var isPrimaryKey = input.IndexOf(@"PRIMARY KEY", StringComparison.OrdinalIgnoreCase) >= 0;
			return new DbColumn(type, name, allowNull, isPrimaryKey);
		}

		private static void AppendCreateTableScript(StringBuilder buffer, DbTable table)
		{
			buffer.Append(@"CREATE TABLE");
			buffer.Append(' ');
			buffer.Append('[');
			buffer.Append(table.Name);
			buffer.Append(']');
			buffer.AppendLine();
			buffer.AppendLine(@"(");

			var columns = table.Columns;
			for (var i = 0; i < columns.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.AppendLine();
				}
				AppendColumnDefinition(buffer, columns[i]);
			}
			foreach (var column in columns)
			{
				if (column.DbForeignKey != null)
				{
					AppendForeignKey(buffer, column);
				}
			}
			buffer.AppendLine();
			buffer.AppendLine(@")");
		}

		private static void AppendColumnDefinition(StringBuilder buffer, DbColumn column)
		{
			buffer.Append('\t');
			buffer.Append('[');
			buffer.Append(column.Name);
			buffer.Append(']');
			buffer.Append(' ');
			buffer.Append(column.Type.Name);
			buffer.Append(' ');
			buffer.Append(column.AllowNull ? @"NULL" : @"NOT NULL");
			if (column.IsPrimaryKey)
			{
				buffer.Append(' ');
				buffer.Append(@"PRIMARY KEY AUTOINCREMENT");
			}
		}

		private static void AppendForeignKey(StringBuilder buffer, DbColumn column)
		{
			buffer.Append(',');
			buffer.AppendLine();
			buffer.Append('\t');
			buffer.Append(@"FOREIGN KEY");
			buffer.Append('(');
			buffer.Append('[');
			buffer.Append(column.Name);
			buffer.Append(']');
			buffer.Append(')');

			var key = column.DbForeignKey;
			buffer.Append(' ');
			buffer.Append(@"REFERENCES");
			buffer.Append(' ');
			buffer.Append('[');
			buffer.Append(key.Table);
			buffer.Append(']');

			buffer.Append(' ');

			buffer.Append('(');
			buffer.Append('[');
			buffer.Append(key.Column);
			buffer.Append(']');
			buffer.Append(')');
		}
	}
}