using System;

namespace AppBuilder.Clr
{
	public sealed class FieldDefinition
	{
		public static readonly FieldDefinition AutoProperty = new FieldDefinition(TypeDefinition.String, @"N/A");
		public static readonly FieldDefinition ImmutableAutoProperty = new FieldDefinition(TypeDefinition.String, @"N/A");

		public AccessModifier AccessModifier { get { return AccessModifier.Private; } }
		public TypeDefinition Type { get; private set; }
		public string Name { get; private set; }
		public string InitialValue { get; private set; }
		public bool IsReadOnly { get; private set; }

		public FieldDefinition(TypeDefinition type, string name, string initialValue = "", bool isReadOnly = true)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");
			if (initialValue == null) throw new ArgumentNullException("initialValue");

			this.Type = type;
			this.Name = NameProvider.ToFieldName(name);
			this.InitialValue = initialValue;
			this.IsReadOnly = isReadOnly;
		}
	}
}