using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppBuilder.Db.DDL;
using AppBuilder.Db.DML;

namespace AppBuilder.Db
{
	public static class QueryCreator
	{
		public static DbQuery GetSelect(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			return new DbQuery(GetSelectBuffer(table).ToString());
		}

		public static DbQuery GetSelect(DbTable table, DbColumn[] columns)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (columns == null) throw new ArgumentNullException("columns");
			if (columns.Length == 0) throw new ArgumentOutOfRangeException("columns");

			var parameters = GetParameters(columns);

			var buffer = GetSelectBuffer(table);
			buffer.Append(@" WHERE ");
			AppendParameters(buffer, columns, parameters);

			return new DbQuery(buffer.ToString(), parameters);
		}

		public static DbQuery GetSelect(DbTable headerTable, DbTable detailsTable)
		{
			if (headerTable == null) throw new ArgumentNullException("headerTable");
			if (detailsTable == null) throw new ArgumentNullException("detailsTable");

			var headerAlias = @"_" + char.ToLowerInvariant(headerTable.Name[0]);
			var detailsAlias = @"_" + char.ToLowerInvariant(detailsTable.Name[0]);
			if (headerAlias == detailsAlias)
			{
				detailsAlias += @"1";
			}
			var primaryKey = headerTable.Columns.Single(c => c.IsPrimaryKey);
			var foreignKey = detailsTable.Columns.Single(c => c.DbForeignKey != null && c.DbForeignKey.Table == headerTable.Name);

			var buffer = new StringBuilder();
			buffer.Append(@"SELECT ");
			AppendNames(buffer, headerTable.Columns.Select(c => headerAlias + @"." + c.Name).ToArray());
			AppendSeparator(buffer);
			AppendNames(buffer, detailsTable.Columns.Where(c => c.DbForeignKey == null || c.DbForeignKey.Table != headerTable.Name).Select(c => detailsAlias + @"." + c.Name).ToArray());
			buffer.Append(@" FROM ");
			buffer.Append(headerTable.Name);
			buffer.Append(@" ");
			buffer.Append(headerAlias);
			buffer.Append(@" ");
			buffer.Append(@" INNER JOIN ");
			buffer.Append(detailsTable.Name);
			buffer.Append(@" ");
			buffer.Append(detailsAlias);
			buffer.Append(@" ");
			buffer.Append(@"ON");
			buffer.Append(@" ");
			buffer.Append(headerAlias);
			buffer.Append(@".");
			buffer.Append(primaryKey.Name);
			buffer.Append(@" = ");
			buffer.Append(detailsAlias);
			buffer.Append(@".");
			buffer.Append(foreignKey.Name);

			return new DbQuery(buffer.ToString());
		}

		public static DbQuery GetInsert(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			return new DbQuery(GetInsertBuffer(table).ToString());
		}

		public static DbQuery GetUpdate(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			return new DbQuery(GetUpdateBuffer(table).ToString());
		}

		public static DbQuery GetDelete(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			return new DbQuery(GetDeleteBuffer(table).ToString());
		}

		private static StringBuilder GetSelectBuffer(DbTable table)
		{
			var buffer = new StringBuilder(256);

			buffer.Append(@"SELECT ");
			AppendColumnNames(buffer, table.Columns);

			buffer.Append(@" FROM ");
			buffer.Append(table.Name);

			return buffer;
		}

		private static StringBuilder GetInsertBuffer(DbTable table)
		{
			var buffer = new StringBuilder(256);

			buffer.Append(@"INSERT INTO ");
			buffer.Append(table.Name);
			buffer.Append('(');
			AppendColumnNames(buffer, table.Columns);
			buffer.Append(')');
			buffer.Append(@" VALUES ");
			buffer.Append('(');
			AppendParameterNames(buffer, GetParameters(table.Columns));
			buffer.Append(')');

			return buffer;
		}

		private static StringBuilder GetUpdateBuffer(DbTable table)
		{
			var buffer = new StringBuilder(256);

			var updateColumns = table.Columns.Where(c => !c.IsPrimaryKey).ToArray();
			var updateParameters = GetParameters(updateColumns);

			var primaryKeyColumns = table.Columns.Where(c => c.IsPrimaryKey).ToArray();
			var primaryKeyParameters = GetParameters(primaryKeyColumns);

			buffer.Append(@"UPDATE ");
			buffer.Append(table.Name);
			buffer.Append(@" SET ");
			AppendParameters(buffer, updateColumns, updateParameters);
			buffer.Append(@" WHERE ");
			AppendParameters(buffer, primaryKeyColumns, primaryKeyParameters);

			return buffer;
		}

		private static StringBuilder GetDeleteBuffer(DbTable table)
		{
			var buffer = new StringBuilder(256);

			var primaryKeyColumns = table.Columns.Where(c => c.IsPrimaryKey).ToArray();
			var primaryKeyParameters = GetParameters(primaryKeyColumns);

			buffer.Append(@"DELETE");
			buffer.Append(@" FROM ");
			buffer.Append(table.Name);
			buffer.Append(@" WHERE ");
			AppendParameters(buffer, primaryKeyColumns, primaryKeyParameters);

			return buffer;
		}

		private static void AppendParameters(StringBuilder buffer, IEnumerable<DbColumn> columns, IEnumerable<DbQueryParameter> parameters)
		{
			AppendNames(buffer, columns.Zip(parameters, (column, parameter) => column.Name + @" = " + parameter.Name).ToArray());
		}

		private static void AppendColumnNames(StringBuilder buffer, IEnumerable<DbColumn> columns)
		{
			AppendNames(buffer, columns.Select(v => v.Name).ToArray());
		}

		private static void AppendParameterNames(StringBuilder buffer, IEnumerable<DbQueryParameter> parameters)
		{
			AppendNames(buffer, parameters.Select(v => v.Name).ToArray());
		}

		private static void AppendNames(StringBuilder buffer, string[] names)
		{
			for (var i = 0; i < names.Length; i++)
			{
				if (i > 0)
				{
					AppendSeparator(buffer);
				}
				buffer.Append(names[i]);
			}
		}

		private static void AppendSeparator(StringBuilder buffer)
		{
			buffer.Append(@", ");
		}

		private static DbQueryParameter[] GetParameters(DbColumn[] columns)
		{
			var parameters = new DbQueryParameter[columns.Length];

			for (var i = 0; i < columns.Length; i++)
			{
				parameters[i] = new DbQueryParameter(@"@" + NameProvider.ToParamterName(columns[i].Name));
			}

			return parameters;
		}
	}
}