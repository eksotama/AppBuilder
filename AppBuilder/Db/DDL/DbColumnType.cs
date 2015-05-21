using System;

namespace AppBuilder.Db.DDL
{
	public sealed class DbColumnType
	{
		public readonly static DbColumnType Integer = new DbColumnType(@"INTEGER");
		public readonly static DbColumnType String = new DbColumnType(@"TEXT");
		public readonly static DbColumnType Decimal = new DbColumnType(@"DECIMAL");
		public readonly static DbColumnType DateTime = new DbColumnType(@"DATETIME");
		public readonly static DbColumnType Bytes = new DbColumnType(@"BLOB");

		private static readonly DbColumnType[] Types =
		{
			Integer,
			String,
			Decimal,
			DateTime,
			Bytes
		};

		public string Name { get; private set; }

		private DbColumnType(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
		}

		public static DbColumnType Parse(string input)
		{
			if (input == null) throw new ArgumentNullException("input");

			foreach (var type in Types)
			{
				if (input.Equals(type.Name, StringComparison.OrdinalIgnoreCase))
				{
					return type;
				}
			}

			throw new ArgumentOutOfRangeException(@"input");
		}
	}
}