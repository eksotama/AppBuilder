using System;

namespace AppBuilder.Db
{
	public sealed class DbForeignKey
	{
		public string Table { get; private set; }
		public string Column { get; private set; }

		public DbForeignKey(string table, string column)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (column == null) throw new ArgumentNullException("column");
			if (table == string.Empty) throw new ArgumentOutOfRangeException("table");
			if (column == string.Empty) throw new ArgumentOutOfRangeException("column");

			this.Table = table;
			this.Column = column;
		}
	}
}