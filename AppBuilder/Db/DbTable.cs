using System;

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
	}
}