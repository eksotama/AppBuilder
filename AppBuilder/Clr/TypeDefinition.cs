using System;

namespace AppBuilder.Clr
{
	public sealed class TypeDefinition
	{
		public static readonly TypeDefinition Long = new TypeDefinition(@"long", false, @"0L");
		public static readonly TypeDefinition Decimal = new TypeDefinition(@"decimal", false, @"0M");
		public static readonly TypeDefinition String = new TypeDefinition(@"string", true, @"string.Empty");
		public static readonly TypeDefinition DateTime = new TypeDefinition(@"DateTime", false, @"DateTime.MinValue");
		public static readonly TypeDefinition Bytes = new TypeDefinition(@"byte[]", false, @"default(byte[])");

		public string Name { get; private set; }
		public bool IsReference { get; private set; }
		public string DefaultValue { get; private set; }

		public TypeDefinition(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.IsReference = true;
			this.DefaultValue = string.Format(@"default({0})", name);
		}

		private TypeDefinition(string name, bool isReference, string defaultValue)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (defaultValue == null) throw new ArgumentNullException("defaultValue");

			this.Name = name;
			this.IsReference = isReference;
			this.DefaultValue = defaultValue;
		}
	}
}