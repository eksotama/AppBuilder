using System;

namespace AppBuilder.Clr
{
	public sealed class ClrParameter
	{
		public ClrType Type { get; private set; }
		public string Name { get; private set; }

		public ClrParameter(ClrType type, string name)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.Type = type;
			this.Name = NameProvider.ToParamterName(name);
		}
	}
}