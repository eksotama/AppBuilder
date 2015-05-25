using System;
using AppBuilder.Clr;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class DbTableConverter
	{
		private static readonly ClrType[] Types =
		{
			ClrType.Long,
			ClrType.String,
			ClrType.Decimal,
			ClrType.DateTime,
			ClrType.Bytes,
		};

		public static ClrClass ToClrClass(DbTable table, DbTable[] tables)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (tables == null) throw new ArgumentNullException("tables");

			return new ClrClass(table.ClassName, GetProperties(table, tables));
		}

		private static ClrProperty[] GetProperties(DbTable table, DbTable[] tables)
		{
			var columns = table.Columns;
			var collectionProperty = FindCollectionProperty(table, tables);

			ClrProperty[] properties;
			if (collectionProperty == null)
			{
				properties = new ClrProperty[columns.Length];
			}
			else
			{
				properties = new ClrProperty[columns.Length + 1];
				properties[properties.Length - 1] = collectionProperty;
			}

			// Table properties
			for (var i = 0; i < columns.Length; i++)
			{
				var column = columns[i];
				var foreignKey = column.DbForeignKey;

				var type = Types[column.Type.Sequence];
				var name = NameProvider.GetPropertyName(column.Name, foreignKey != null);

				if (foreignKey != null)
				{
					var classType = string.Empty;
					var tableName = foreignKey.Table;
					foreach (var t in tables)
					{
						if (t.Name == tableName)
						{
							classType = t.ClassName;
							break;
						}
					}
					type = ClrType.UserType(classType, !column.AllowNull);
				}

				properties[i] = new ClrProperty(type, name);
			}

			return properties;
		}

		private static ClrProperty FindCollectionProperty(DbTable table, DbTable[] tables)
		{
			// Collection property for Normal table
			if (!table.IsReadOnly)
			{
				foreach (var current in tables)
				{
					// Exclude self
					if (table.Name == current.Name)
					{
						continue;
					}
					if (IsCollectionTable(current, table.Name))
					{
						return ClrProperty.UserCollection(current.Name, current.ClassName);
					}
				}
			}

			return null;
		}

		private static bool IsCollectionTable(DbTable table, string matchName)
		{
			foreach (var column in table.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null && foreignKey.Table == matchName)
				{
					return true;
				}
			}

			return false;
		}
	}
}