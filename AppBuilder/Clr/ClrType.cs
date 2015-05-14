using System;

namespace AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Long = new ClrType(@"long", false, @"0L", @"GetInt64");
		public static readonly ClrType Decimal = new ClrType(@"decimal", false, @"0M", @"GetDecimal");
		public static readonly ClrType String = new ClrType(@"string", true, @"string.Empty", @"GetString");
		public static readonly ClrType DateTime = new ClrType(@"DateTime", false, @"DateTime.MinValue", @"GetDateTime");
		public static readonly ClrType Bytes = new ClrType(@"byte[]", false, @"default(byte[])", @"GetBytes");

		public string Name { get; private set; }
		public bool IsReference { get; private set; }
		public bool IsBuiltIn { get; private set; }
		public string DefaultValue { get; private set; }
		public string ReaderMethod { get; private set; }

		public ClrType(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.IsReference = true;
			this.DefaultValue = string.Format(@"default({0})", name);
			this.ReaderMethod = @"GetInt64";
		}

		private ClrType(string name, bool isReference, string defaultValue, string readerMethod)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (defaultValue == null) throw new ArgumentNullException("defaultValue");
			if (readerMethod == null) throw new ArgumentNullException("readerMethod");

			this.Name = name;
			this.IsReference = isReference;
			this.IsBuiltIn = true;
			this.DefaultValue = defaultValue;
			this.ReaderMethod = readerMethod;
		}
	}
}