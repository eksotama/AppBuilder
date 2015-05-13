using System;

namespace AppBuilder.Clr
{
	public sealed class MethodDefinition
	{
		public string Name { get; private set; }
		public string ReturnType { get; private set; }
		public ParameterDefinition[] Parameters { get; private set; }

		public MethodDefinition(string name)
			: this(name, string.Empty, new ParameterDefinition[0])
		{
		}

		public MethodDefinition(string name, string returnType)
			: this(name, returnType, new ParameterDefinition[0])
		{
		}

		public MethodDefinition(string name, string returnType, ParameterDefinition[] parameters)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (returnType == null) throw new ArgumentNullException("returnType");
			if (parameters == null) throw new ArgumentNullException("parameters");

			this.Name = name;
			this.ReturnType = returnType;
			this.Parameters = parameters;
		}
	}
}