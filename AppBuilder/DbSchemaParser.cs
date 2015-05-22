using System;
using System.Collections.Generic;
using System.Text;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class DbSchemaParser
	{
		private static readonly string Space = @" ";
		private static readonly string Semicolon = @";";
		private static readonly string Comma = @",";
		private static readonly string OpenBrace = @"(";
		private static readonly string CloseBrace = @")";
		private static readonly char Tab = '\t';
		private static readonly string[] SeparatorArray = { Semicolon + Environment.NewLine };

		public static DbSchema Parse(string script)
		{
			if (script == null) throw new ArgumentNullException("script");

			var entries = script.Split(SeparatorArray, StringSplitOptions.RemoveEmptyEntries);
			var tables = new DbTable[entries.Length];
			for (var i = 0; i < tables.Length; i++)
			{
				tables[i] = ParseTable(entries[i]);
			}
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
			var lines = RemoveBrackets(script)
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			var tableName = ExtractBetween(lines[0], @"CREATE TABLE", Environment.NewLine).Trim();

			var columns = new List<DbColumn>();
			for (var index = 2; index < lines.Length - 1; index++)
			{
				var value = lines[index].Trim();
				var columnName = ExtractBetween(value, @"FOREIGN KEY", CloseBrace);
				if (columnName != string.Empty)
				{
					columnName = columnName.Trim().Substring(1);

					var foreignKey = ParseForeignKey(value);
					foreach (var column in columns)
					{
						if (column.Name == columnName)
						{
							column.DbForeignKey = foreignKey;
							break;
						}
					}
				}
				else
				{
					columns.Add(ParseColumn(value));
				}
			}

			return new DbTable(tableName, columns.ToArray(), null);
		}

		private static DbForeignKey ParseForeignKey(string input)
		{
			var value = ExtractBetween(input, @"REFERENCES", CloseBrace);
			var index = value.IndexOf(OpenBrace, StringComparison.Ordinal);
			var table = value.Substring(0, index).Trim();
			var column = value.Substring(index + 1).Trim();
			return new DbForeignKey(table, column);
		}

		private static DbColumn ParseColumn(string input)
		{
			var name = input.Substring(0, input.IndexOf(Space, StringComparison.Ordinal));
			var type = DbColumnType.Parse(ExtractBetween(input, Space, Space));
			var allowNull = input.IndexOf(@"NOT NULL", StringComparison.OrdinalIgnoreCase) < 0;
			var isPrimaryKey = input.IndexOf(@"PRIMARY KEY", StringComparison.OrdinalIgnoreCase) >= 0;
			return new DbColumn(type, name, allowNull, isPrimaryKey);
		}

		private static void AppendCreateTableScript(StringBuilder buffer, DbTable table)
		{
			buffer.Append(@"CREATE TABLE");
			buffer.Append(Space);
			AppendWithBrackets(buffer, table.Name);
			buffer.AppendLine();
			buffer.AppendLine(OpenBrace);

			var columns = table.Columns;
			for (var i = 0; i < columns.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(Comma);
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
			buffer.AppendLine(CloseBrace);
		}

		private static void AppendColumnDefinition(StringBuilder buffer, DbColumn column)
		{
			buffer.Append(Tab);
			AppendWithBrackets(buffer, column.Name);
			buffer.Append(Space);
			buffer.Append(column.Type.Name);
			buffer.Append(Space);
			buffer.Append(column.AllowNull ? @"NULL" : @"NOT NULL");
			if (column.IsPrimaryKey)
			{
				buffer.Append(Space);
				buffer.Append(@"PRIMARY KEY AUTOINCREMENT");
			}
		}

		private static void AppendForeignKey(StringBuilder buffer, DbColumn column)
		{
			buffer.Append(Comma);
			buffer.AppendLine();
			buffer.Append(Tab);
			buffer.Append(@"FOREIGN KEY");
			AppendWithBraces(buffer, column.Name);
			buffer.Append(Space);
			buffer.Append(@"REFERENCES");
			buffer.Append(Space);
			AppendWithBrackets(buffer, column.DbForeignKey.Table);
			buffer.Append(Space);
			AppendWithBraces(buffer, column.DbForeignKey.Column);
		}

		private static void AppendWithBraces(StringBuilder buffer, string value)
		{
			buffer.Append(OpenBrace);
			AppendWithBrackets(buffer, value);
			buffer.Append(CloseBrace);
		}

		private static void AppendWithBrackets(StringBuilder buffer, string value)
		{
			buffer.Append('[');
			buffer.Append(value);
			buffer.Append(']');
		}

		private static string RemoveBrackets(string schema)
		{
			var buffer = new StringBuilder(schema.Length);

			foreach (var symbol in schema)
			{
				if (symbol == '[' || symbol == ']')
				{
					continue;
				}
				buffer.Append(symbol);
			}

			return buffer.ToString();
		}

		private static string ExtractBetween(string input, string begin, string end)
		{
			var index = input.IndexOf(begin, StringComparison.OrdinalIgnoreCase);
			if (index >= 0)
			{
				var start = index + begin.Length;
				var stop = input.IndexOf(end, start, StringComparison.OrdinalIgnoreCase);
				if (stop >= 0)
				{
					return input.Substring(start, stop - start);
				}
				return input.Substring(start);
			}

			return string.Empty;
		}
	}
}