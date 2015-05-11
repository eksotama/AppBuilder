using System;

namespace AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Integer = new ClrType(@"long", @"long?");
		public static readonly ClrType Decimal = new ClrType(@"decimal", @"decimal?");
		public static readonly ClrType DateTime = new ClrType(@"DateTime", @"DateTime?");
		public static readonly ClrType String = new ClrType(@"string");
		public static readonly ClrType Bytes = new ClrType(@"byte[]");

		public string Name { get; private set; }
		public string NullableName { get; private set; }

		public ClrType(string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
		}

		public ClrType(string name, string nullableName)
			: this(name)
		{
			if (nullableName == null) throw new ArgumentNullException("nullableName");
			if (nullableName == string.Empty) throw new ArgumentOutOfRangeException("nullableName");

			this.Name = name;
			this.NullableName = nullableName;
		}
	}
}