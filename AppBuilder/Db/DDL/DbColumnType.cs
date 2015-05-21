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

		public string Name { get; private set; }
		public int? Length { get; private set; }

		public DbColumnType(string name, int? length = null)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.Length = length;
		}

		public static DbColumnType GetString(int? length)
		{
			return new DbColumnType(String.Name, length);
		}
	}
}