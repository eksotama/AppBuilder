using System;
using System.Collections.Generic;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
	public static class DbTableConverter
	{
		public static ClrClass ToClassDefinition(DbTable table, NameProvider nameProvider)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");

			return new ClrClass(nameProvider.GetClassName(table.Name), GetProperties(table.Columns, nameProvider));
		}

		private static ClrProperty[] GetProperties(IList<DbColumn> columns, NameProvider nameProvider)
		{
			var properties = new ClrProperty[columns.Count];

			for (var index = 0; index < columns.Count; index++)
			{
				var column = columns[index];
				var foreignKey = column.ForeignKey;

				var name = nameProvider.GetPropertyName(column.Name, foreignKey != null);
				var type = GetClrType(column.Type);
				if (foreignKey != null)
				{
					type = new ClrType(nameProvider.GetClassName(foreignKey.Table));
				}
				properties[index] = new ClrProperty(type, name, ClrField.AutoProperty);
			}

			return properties;
		}

		private static ClrType GetClrType(DbColumnType type)
		{
			if (type == DbColumnType.Integer)
			{
				return ClrType.Long;
			}
			if (type == DbColumnType.String)
			{
				return ClrType.String;
			}
			if (type == DbColumnType.Decimal)
			{
				return ClrType.Decimal;
			}
			if (type == DbColumnType.DateTime)
			{
				return ClrType.DateTime;
			}
			if (type == DbColumnType.Bytes)
			{
				return ClrType.Bytes;
			}
			return ClrType.Long;
		}
	}
}