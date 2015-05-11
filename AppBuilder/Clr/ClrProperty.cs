using System;

namespace AppBuilder.Clr
{
	public sealed class ClrProperty
	{
		public string Name { get; private set; }
		public ClrType Type { get; private set; }
		public bool Nullable { get; private set; }
		public bool IsReferenceType { get; private set; }

		public ClrProperty(string name, ClrType type, bool nullable)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (type == null) throw new ArgumentNullException("type");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
			this.Type = type;
			this.Nullable = nullable;
			this.IsReferenceType = true;
			if (type == ClrType.Integer || type == ClrType.Decimal || type == ClrType.DateTime)
			{
				this.IsReferenceType = nullable;
			}
		}
	}
}