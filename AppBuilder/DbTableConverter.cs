using System;
using System.Collections.Generic;
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

		private static ClrProperty[] GetProperties(DbTable inputTable, DbTable[] tables)
		{
			var columns = inputTable.Columns;
			var properties = new List<ClrProperty>(columns.Length + 1);

			// Table properties
			foreach (var column in columns)
			{
				var foreignKey = column.DbForeignKey;

				var type = Types[column.Type.Sequence];
				var name = NameProvider.GetPropertyName(column.Name, foreignKey != null);

				if (foreignKey != null)
				{
					var classType = string.Empty;
					var tableName = foreignKey.Table;
					foreach (var table in tables)
					{
						if (table.Name == tableName)
						{
							classType = table.ClassName;
							break;
						}
					}
					type = ClrType.UserType(classType, !column.AllowNull);
				}

				properties.Add(new ClrProperty(type, name));
			}

			// Collection properties for Normal table
			if (inputTable.IsReadOnly.HasValue && !inputTable.IsReadOnly.Value)
			{
				var inputTableName = inputTable.Name;

				foreach (var table in tables)
				{
					var currentTableName = table.Name;
					if (inputTableName != currentTableName)
					{
						var isCollectionAdded = false;
						foreach (var column in table.Columns)
						{
							var foreignKey = column.DbForeignKey;
							if (foreignKey != null && foreignKey.Table == inputTableName)
							{
								properties.Add(ClrProperty.UserCollection(currentTableName, table.ClassName));
								isCollectionAdded = true;
								break;
							}
						}
						if (isCollectionAdded)
						{
							break;
						}
					}
				}
			}

			return properties.ToArray();
		}
	}
}