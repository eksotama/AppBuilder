using System;

namespace AppBuilder.Db.DDL
{
	public sealed class DbSchema
	{
		public string Name { get; private set; }
		public DbTable[] Tables { get; private set; }

		public DbSchema(string name, DbTable[] tables)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (tables == null) throw new ArgumentNullException("tables");
			if (tables.Length == 0) throw new ArgumentOutOfRangeException("tables");

			this.Name = name;
			this.Tables = tables;
		}
	}
}