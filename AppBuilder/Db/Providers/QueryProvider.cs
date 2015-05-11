using System;
using System.Text;

namespace AppBuilder.Db.Providers
{
	public static class QueryProvider
	{
		public static string GetSelect(DbTable table)
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

			return buffer.ToString();
		}
	}
}