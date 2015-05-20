using System;
using System.Collections.Generic;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
	public static class DbTableConverter
	{
		public static ClrClass ToClrClass(DbTable table, NameProvider nameProvider, DbTable[] tables)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");
			if (tables == null) throw new ArgumentNullException("tables");

			return new ClrClass(nameProvider.GetClassName(table.Name), GetProperties(table, nameProvider, tables));
		}

		private static ClrProperty[] GetProperties(DbTable table, NameProvider nameProvider, IEnumerable<DbTable> tables)
		{
			var referencedTables = GetReferencedTables(table.Name, tables);

			var columns = table.Columns;
			var properties = new List<ClrProperty>(columns.Length);
			var navigationProperties = new List<ClrProperty>();

			foreach (var column in columns)
			{
				var foreignKey = column.DbForeignKey;

				var name = nameProvider.GetPropertyName(column.Name, foreignKey != null);
				var type = GetClrType(column.Type);
				if (foreignKey != null)
				{
					type = new ClrType(nameProvider.GetClassName(foreignKey.Table), !column.AllowNull);

					if (referencedTables.Count > 0)
					{
						var t = referencedTables[0];
						var navigationType = nameProvider.GetClassName(t.Name);
						navigationProperties.Add(new ClrProperty(new ClrType(string.Format(@"List<{0}>", navigationType), true), t.Name, ClrField.AutoProperty));
					}
				}
				properties.Add(new ClrProperty(type, name, ClrField.AutoProperty));
			}

			properties.AddRange(navigationProperties);

			return properties.ToArray();
		}

		private static List<DbTable> GetReferencedTables(string name, IEnumerable<DbTable> tables)
		{
			var referencedTables = new List<DbTable>();

			foreach (var table in tables)
			{
				if (name != table.Name)
				{
					foreach (var column in table.Columns)
					{
						var foreignKey = column.DbForeignKey;
						if (foreignKey != null && foreignKey.Table == name)
						{
							referencedTables.Add(table);
						}
					}
				}
			}

			return referencedTables;
		}

		private static ClrType GetClrType(DbColumnType type)
		{
			if (type == DbColumnType.Integer)
			{
				return ClrType.Long;
			}
			if (type == DbColumnType.String || type.Name == DbColumnType.String.Name)
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