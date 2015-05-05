using System;

namespace AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Integer = new ClrType(@"Integer");
		public static readonly ClrType Decimal = new ClrType(@"Decimal");
		public static readonly ClrType DateTime = new ClrType(@"DateTime");
		public static readonly ClrType String = new ClrType(@"String");
		public static readonly ClrType Bytes = new ClrType(@"Bytes");

		public string Name { get; private set; }

		public ClrType(string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
		}
	}
}