using System;

namespace AppBuilder.Clr
{
	public sealed class ClrProperty
	{
		public ClrType Type { get; private set; }
		public string Name { get; private set; }

		public ClrProperty(ClrType type, string name)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.Type = type;
			this.Name = NameProvider.ToPropertyName(name);
		}
	}
}