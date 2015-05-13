using System;

namespace AppBuilder.Clr
{
	public sealed class ParameterDefinition
	{
		public TypeDefinition Type { get; private set; }
		public string Name { get; private set; }

		public ParameterDefinition(TypeDefinition type, string name)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.Type = type;
			this.Name = NameProvider.ToParamterName(name);
		}
	}
}