using System;

namespace AppBuilder.Clr
{
	public sealed class ClrField
	{
		public static readonly ClrField AutoProperty = new ClrField(ClrType.String, @"N/A");
		public static readonly ClrField ImmutableAutoProperty = new ClrField(ClrType.String, @"N/A");

		public ClrType Type { get; private set; }
		public string Name { get; private set; }
		public string InitialValue { get; private set; }
		public bool IsReadOnly { get; private set; }
		public ClrProperty Property { get; private set; }

		public ClrField(ClrType type, string name, string initialValue = "", bool isReadOnly = true, ClrProperty property = null)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");
			if (initialValue == null) throw new ArgumentNullException("initialValue");

			this.Type = type;
			this.Name = NameProvider.ToFieldName(name);
			this.InitialValue = initialValue;
			this.IsReadOnly = isReadOnly;
			this.Property = property;
		}
	}
}