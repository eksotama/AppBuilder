using System;

namespace AppBuilder.Clr
{
	public sealed class PropertyDefinition
	{
		public AccessModifier AccessModifier { get; private set; }
		public TypeDefinition Type { get; private set; }
		public string Name { get; private set; }
		public FieldDefinition BackingField { get; private set; }

		public PropertyDefinition(TypeDefinition type, string name, FieldDefinition backingField = null, AccessModifier accessModifier = AccessModifier.Public)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.AccessModifier = accessModifier;
			this.Type = type;
			this.Name = NameProvider.ToPropertyName(name);
			this.BackingField = backingField;
		}

		public static PropertyDefinition CreateMutable(TypeDefinition type, string name)
		{
			return Create(type, name, FieldDefinition.AutoProperty);
		}

		public static PropertyDefinition CreateImmutable(TypeDefinition type, string name)
		{
			return Create(type, name, FieldDefinition.ImmutableAutoProperty);
		}

		private static PropertyDefinition Create(TypeDefinition type, string name, FieldDefinition field)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			return new PropertyDefinition(type, name, field);
		}
	}
}