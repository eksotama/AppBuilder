using System;
using System.Collections.Generic;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
	public static class DbTableConverter
	{
		private static readonly TypeDefinition[] Types =
		{
			TypeDefinition.Long,
			TypeDefinition.Decimal,
			TypeDefinition.String,
			TypeDefinition.DateTime,
			TypeDefinition.Bytes
		};

		public static ClassDefinition ToClassDefinition(DbTable table, NameProvider nameProvider)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");

			return new ClassDefinition(nameProvider.GetClassName(table.Name), GetProperties(table.Columns, nameProvider));
		}

		private static PropertyDefinition[] GetProperties(IList<DbColumn> columns, NameProvider nameProvider)
		{
			var properties = new PropertyDefinition[columns.Count];

			for (var index = 0; index < columns.Count; index++)
			{
				var column = columns[index];
				var foreignKey = column.ForeignKey;

				var name = nameProvider.GetPropertyName(column.Name, foreignKey != null);
				var type = Types[(int)column.Type];
				if (foreignKey != null)
				{
					type = new TypeDefinition(nameProvider.GetClassName(foreignKey.Table));
				}
				properties[index] = new PropertyDefinition(type, name, FieldDefinition.AutoProperty);
			}

			return properties;
		}
	}
}