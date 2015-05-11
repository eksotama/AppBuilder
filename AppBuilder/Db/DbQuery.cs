using System;
using System.Text;

namespace AppBuilder.Db
{
	public static class DbQuery
	{
		public static string GetSelect(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			var buffer = new StringBuilder(@"SELECT ");

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