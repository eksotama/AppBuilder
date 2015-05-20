using System;
using System.Collections.Generic;

namespace AppBuilder.Clr
{
	public sealed class ClrContructor
	{
		public string Name { get; private set; }
		public ICollection<ClrParameter> Parameters { get; private set; }

		public ClrContructor(string name, ICollection<ClrParameter> parameters)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (parameters == null) throw new ArgumentNullException("parameters");

			this.Name = name;
			this.Parameters = parameters;
		}
	}
}