using System;
using AppBuilder.Db;

namespace AppBuilder.Clr
{
	public sealed class ClrProperty
	{
		public string Name { get; private set; }
		public ClrType Type { get; private set; }
		public bool Nullable { get; private set; }
		public DbColumn Column { get; private set; }

		public ClrProperty(string name, ClrType type, bool nullable, DbColumn column)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (type == null) throw new ArgumentNullException("type");
			if (column == null) throw new ArgumentNullException("column");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
			this.Type = type;
			this.Nullable = nullable;
			Column = column;
		}
	}
}