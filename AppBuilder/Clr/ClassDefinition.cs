using System;
using System.Collections.Generic;

namespace AppBuilder.Clr
{
	public sealed class ClassDefinition
	{
		public AccessModifier AccessModifier { get; private set; }
		public bool Sealed { get; private set; }
		public string Name { get; private set; }
		public List<string> Interfaces { get; private set; }
		public List<FieldDefinition> Fields { get; private set; }
		public PropertyDefinition[] Properties { get; private set; }
		public List<MethodDefinition> Methods { get; private set; }

		public ClassDefinition(string name, PropertyDefinition[] properties)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (properties == null) throw new ArgumentNullException("properties");

			this.AccessModifier = AccessModifier.Public;
			this.Sealed = true;
			this.Name = NameProvider.ToClassName(name);
			this.Interfaces = new List<string>();
			this.Fields = new List<FieldDefinition>();
			this.Properties = properties;
			this.Methods = new List<MethodDefinition>();
		}
	}
}