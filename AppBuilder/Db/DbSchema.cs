using System;

namespace AppBuilder.Db
{
	public sealed class DbSchema
	{
		public string Name { get; private set; }
		public DbTable[] Tables { get; private set; }

		public DbSchema(string name, DbTable[] tables)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (tables == null) throw new ArgumentNullException("tables");

			this.Name = name;
			this.Tables = tables;
		}
	}
}