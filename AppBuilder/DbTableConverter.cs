using System;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
	public static class DbTableConverter
	{
		public static ClrClass ToClrObject(DbTable table, NameProvider nameProvider)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");

			var name = nameProvider.GetClassName(table.Name);
			var properties = GetClrProperties(table.Columns, nameProvider);
			return new ClrClass(name, properties);
		}

		private static ClrProperty[] GetClrProperties(DbColumn[] columns, NameProvider nameProvider)
		{
			var properties = new ClrProperty[columns.Length];

			for (var index = 0; index < columns.Length; index++)
			{
				properties[index] = GetClrProperty(columns[index], nameProvider);
			}

			return properties;
		}

		private static ClrProperty GetClrProperty(DbColumn column, NameProvider nameProvider)
		{
			var name = nameProvider.GetPropertyName(column);
			var type = GetClrType(column, nameProvider);
			return new ClrProperty(name, type, column.AllowNull, column);
		}

		private static ClrType GetClrType(DbColumn column, NameProvider nameProvider)
		{
			var foreignKey = column.ForeignKey;
			if (foreignKey == null)
			{
				switch (column.Type)
				{
					case DbColumnType.Integer:
						return ClrType.Integer;
					case DbColumnType.Decimal:
						return ClrType.Decimal;
					case DbColumnType.String:
						return ClrType.String;
					case DbColumnType.DateTime:
						return ClrType.DateTime;
					case DbColumnType.Bytes:
						return ClrType.Bytes;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			return new ClrType(nameProvider.GetClassName(foreignKey.Table));
		}
	}
}