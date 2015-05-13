using System;
using System.Collections.Generic;

namespace AppBuilder.Clr
{
	public sealed class ContructorDefinition
	{
		public AccessModifier AccessModifier { get { return AccessModifier.Public; } }
		public string Name { get; private set; }
		public ICollection<ParameterDefinition> Parameters { get; private set; }
		public ICollection<PropertyDefinition> Properties { get; private set; }

		public ContructorDefinition(string name, ICollection<ParameterDefinition> parameters)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (parameters == null) throw new ArgumentNullException("parameters");

			this.Name = name;
			this.Parameters = parameters;
			this.Properties = new PropertyDefinition[0];
		}

		public ContructorDefinition(string name, ICollection<PropertyDefinition> properties)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (properties == null) throw new ArgumentNullException("properties");

			this.Name = name;
			this.Parameters = new ParameterDefinition[0];
			this.Properties = properties;
		}
	}
}