using System;

namespace AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Long = new ClrType(@"long", false, @"0L");
		public static readonly ClrType Decimal = new ClrType(@"decimal", false, @"0M");
		public static readonly ClrType String = new ClrType(@"string", true, @"string.Empty");
		public static readonly ClrType DateTime = new ClrType(@"DateTime", false, @"DateTime.MinValue");
		public static readonly ClrType Bytes = new ClrType(@"byte[]", false, @"default(byte[])");

		public string Name { get; private set; }
		public bool IsReference { get; private set; }
		public string DefaultValue { get; private set; }

		public ClrType(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.IsReference = true;
			this.DefaultValue = string.Format(@"default({0})", name);
		}

		private ClrType(string name, bool isReference, string defaultValue)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (defaultValue == null) throw new ArgumentNullException("defaultValue");

			this.Name = name;
			this.IsReference = isReference;
			this.DefaultValue = defaultValue;
		}
	}
}