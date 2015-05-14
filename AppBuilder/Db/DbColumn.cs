using System;

namespace AppBuilder.Db
{
	public sealed class DbColumn
	{
		public string Name { get; private set; }
		public DbColumnType Type { get; private set; }
		public bool AllowNull { get; private set; }
		public bool IsPrimaryKey { get; private set; }
		public DbForeignKey ForeignKey { get; set; }

		public DbColumn(DbColumnType type, string name, bool allowNull = false, bool isPrimaryKey = false)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");
			if (name.Length == 0) throw new ArgumentOutOfRangeException("name");

			this.Name = NameProvider.ToColumnName(name);
			this.Type = type;
			this.AllowNull = allowNull;
			this.IsPrimaryKey = isPrimaryKey;
		}
	}
}