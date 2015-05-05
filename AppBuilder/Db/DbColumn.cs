using System;

namespace AppBuilder.Db
{
	public sealed class DbColumn
	{
		public string Name { get; private set; }
		public DbColumnType Type { get; private set; }
		public int? Length { get; private set; }
		public bool AllowNull { get; private set; }
		public bool IsPrimaryKey { get; private set; }
		public DbForeignKey ForeignKey { get; set; }

		public DbColumn(string name, DbColumnType type, int? length, bool allowNull, bool isPrimaryKey)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (name.Length == 0) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
			this.Type = type;
			this.AllowNull = allowNull;
			this.Length = length;
			this.IsPrimaryKey = isPrimaryKey;
		}
	}
}