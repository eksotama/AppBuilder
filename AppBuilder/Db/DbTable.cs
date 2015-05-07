using System;
using System.Text;

namespace AppBuilder.Db
{
	public sealed class DbTable
	{
		public string Name { get; private set; }
		public DbColumn[] Columns { get; private set; }

		public DbTable(string name, DbColumn[] columns)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (columns == null) throw new ArgumentNullException("columns");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");
			if (columns.Length == 0) throw new ArgumentOutOfRangeException("columns");

			this.Name = name;
			this.Columns = columns;
		}

		public static void AppendSelectQuery(StringBuilder buffer, DbTable table)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (table == null) throw new ArgumentNullException("table");

			buffer.Append(@"SELECT ");

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
		}
	}
}