using System;

namespace AppBuilder.Clr
{
	public sealed class ClrProperty
	{
		public ClrType Type { get; private set; }
		public string Name { get; private set; }
		public ClrField BackingField { get; private set; }

		public ClrProperty(ClrType type, string name, ClrField backingField = null)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.Type = type;
			this.Name = NameProvider.ToPropertyName(name);
			this.BackingField = backingField;
		}

		public static ClrProperty Auto(ClrType type, string name)
		{
			return Create(type, name, ClrField.AutoProperty);
		}

		private static ClrProperty Create(ClrType type, string name, ClrField field)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			return new ClrProperty(type, name, field);
		}
	}
}