using System;

namespace AppBuilder.Db.DDL
{
	public sealed class DbTable
	{
		public string Name { get; private set; }
		public DbColumn[] Columns { get; private set; }
		public bool IsReadOnly { get; private set; }
		public string ClassName { get; private set; }

		public DbTable(string name, DbColumn[] columns, bool isReadOnly, string className = null)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (columns == null) throw new ArgumentNullException("columns");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");
			if (columns.Length == 0) throw new ArgumentOutOfRangeException("columns");

			this.Name = NameProvider.ToTableName(name);
			this.Columns = columns;
			this.IsReadOnly = isReadOnly;
			this.ClassName = className ?? this.Name.Substring(0, this.Name.Length - 1);
		}

		public static DbTable ReadOnly(string name, DbColumn[] columns)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (columns == null) throw new ArgumentNullException("columns");

			return new DbTable(name, columns, true);
		}

		public static DbTable Normal(string name, DbColumn[] columns, string className = null)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (columns == null) throw new ArgumentNullException("columns");

			return new DbTable(name, columns, false, className);
		}

		public static DbTable Create(string name, params DbColumn[] columns)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (columns == null) throw new ArgumentNullException("columns");

			return new DbTable(name, columns, true);
		}
	}
}