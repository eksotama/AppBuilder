using System;
using System.Collections.Generic;

namespace AppBuilder.Clr
{
	public sealed class ClrContructor
	{
		public string Name { get; private set; }
		public ICollection<ClrParameter> Parameters { get; private set; }
		public ICollection<ClrProperty> Properties { get; private set; }

		public ClrContructor(string name, ICollection<ClrParameter> parameters)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (parameters == null) throw new ArgumentNullException("parameters");

			this.Name = name;
			this.Parameters = parameters;
			this.Properties = new ClrProperty[0];
		}

		public ClrContructor(string name, ICollection<ClrProperty> properties)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (properties == null) throw new ArgumentNullException("properties");

			this.Name = name;
			this.Parameters = new ClrParameter[0];
			this.Properties = properties;
		}
	}
}