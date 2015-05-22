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

		public static ClrProperty Long(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new ClrProperty(ClrType.Long, name);
		}

		public static ClrProperty Decimal(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new ClrProperty(ClrType.Decimal, name);
		}

		public static ClrProperty DateTime(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new ClrProperty(ClrType.DateTime, name);
		}

		public static ClrProperty String(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new ClrProperty(ClrType.String, name);
		}

		public static ClrProperty Bytes(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new ClrProperty(ClrType.Bytes, name);
		}
	}
}