using System;

namespace AppBuilder.Db.DDL
{
	public sealed class DbColumnType
	{
		public readonly static DbColumnType Integer = new DbColumnType(0, @"INTEGER");
		public readonly static DbColumnType String = new DbColumnType(1, @"TEXT");
		public readonly static DbColumnType Decimal = new DbColumnType(2, @"DECIMAL");
		public readonly static DbColumnType DateTime = new DbColumnType(3, @"DATETIME");
		public readonly static DbColumnType Bytes = new DbColumnType(4, @"BLOB");

		private static readonly DbColumnType[] Types =
		{
			Integer,
			String,
			Decimal,
			DateTime,
			Bytes
		};

		public long Sequence { get; private set; }
		public string Name { get; private set; }

		private DbColumnType(long sequence, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Sequence = sequence;
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