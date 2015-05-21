using System;
using System.Text;
using AppBuilder.Db.DDL;
using AppBuilder.Db.DML;

namespace AppBuilder.Db
{
	public static class QueryProvider
	{
		public static DbQuery GetSelectQuery(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			var capacity = 16 + table.Name.Length + (6 * table.Columns.Length);
			var buffer = new StringBuilder(@"SELECT ", capacity);

			var addSeparator = false;
			foreach (var column in table.Columns)
			{
				if (addSeparator)
				{
					buffer.Append(@", ");
				}
				buffer.Append(column.Name);
				addSeparator = true;
			}

			buffer.Append(@" FROM ");
			buffer.Append(table.Name);

			return new DbQuery(buffer.ToString());
		}
	}
}