using System;

namespace AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Integer = new ClrType(@"long", @"long?");
		public static readonly ClrType Decimal = new ClrType(@"decimal", @"decimal?");
		public static readonly ClrType DateTime = new ClrType(@"DateTime", @"DateTime?");
		public static readonly ClrType String = new ClrType(@"string", true);
		public static readonly ClrType Bytes = new ClrType(@"byte[]", true);

		public string Name { get; private set; }
		public string NullableName { get; private set; }
		public bool IsBuiltIn { get; private set; }

		public ClrType(string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
		}

		private ClrType(string name, bool isBuiltIn)
			: this(name)
		{
			this.IsBuiltIn = isBuiltIn;
		}

		private ClrType(string name, string nullableName)
			: this(name)
		{
			if (nullableName == null) throw new ArgumentNullException("nullableName");
			if (nullableName == string.Empty) throw new ArgumentOutOfRangeException("nullableName");

			this.Name = name;
			this.NullableName = nullableName;
			this.IsBuiltIn = true;
		}
	}
}