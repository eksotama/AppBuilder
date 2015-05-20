using System;
using System.Text;
using AppBuilder.Db;

namespace AppBuilder
{
	public static class DbSchemaGenerator
	{
		public static string Generate(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			var buffer = new StringBuilder(1024);

			foreach (var table in schema.Tables)
			{
				Generate(table, buffer);
				buffer.AppendLine();
			}

			return buffer.ToString();
		}

		private static void Generate(DbTable table, StringBuilder buffer)
		{
			buffer.Append(@"CREATE TABLE");
			buffer.Append(' ');
			buffer.Append('[');
			buffer.Append(table.Name);
			buffer.Append(']');
			buffer.AppendLine();
			buffer.AppendLine(@"(");

			var addSeparator = false;
			foreach (var column in table.Columns)
			{
				if (addSeparator)
				{
					buffer.Append(',');
					buffer.AppendLine();
				}
				AppendColumn(buffer, column);
				addSeparator = true;
			}
			foreach (var column in table.Columns)
			{
				if (column.DbForeignKey != null)
				{
					AppendForeignKey(buffer, column);
				}
			}
			buffer.AppendLine();
			buffer.AppendLine(@")");
		}

		private static void AppendColumn(StringBuilder buffer, DbColumn column)
		{
			buffer.Append('\t');
			buffer.Append('[');
			buffer.Append(column.Name);
			buffer.Append(']');
			buffer.Append(' ');
			buffer.Append(column.Type.Name);
			buffer.Append(' ');
			AppendNullability(buffer, column);
			AppendPrimaryKey(buffer, column);
		}

		private static void AppendNullability(StringBuilder buffer, DbColumn column)
		{
			buffer.Append(column.AllowNull ? @"NULL" : @"NOT NULL");
		}

		private static void AppendPrimaryKey(StringBuilder buffer, DbColumn column)
		{
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