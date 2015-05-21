using System;

namespace AppBuilder.Db.DDL
{
	public sealed class DbColumn
	{
		public DbColumnType Type { get; private set; }
		public string Name { get; private set; }
		public bool AllowNull { get; private set; }
		public bool IsPrimaryKey { get; private set; }
		public DbForeignKey DbForeignKey { get; private set; }

		public DbColumn(DbColumnType type, string name, bool allowNull = false, bool isPrimaryKey = false)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			this.Name = NameProvider.ToColumnName(name);
			this.Type = type;
			this.AllowNull = allowNull;
			this.IsPrimaryKey = isPrimaryKey;
			this.DbForeignKey = null;
		}

		public DbColumn(DbColumnType type, string name, DbForeignKey dbForeignKey, bool allowNull = false, bool isPrimaryKey = false)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");
			if (dbForeignKey == null) throw new ArgumentNullException("dbForeignKey");

			this.Name = NameProvider.ToColumnName(name);
			this.Type = type;
			this.AllowNull = allowNull;
			this.IsPrimaryKey = isPrimaryKey;
			this.DbForeignKey = dbForeignKey;
		}

		public static DbColumn PrimaryKey(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new DbColumn(DbColumnType.Integer, name, isPrimaryKey: true);
		}

		public static DbColumn ForeignKey(string name, DbTable table, bool allowNull = false)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (table == null) throw new ArgumentNullException("table");

			foreach (var column in table.Columns)
			{
				if (column.IsPrimaryKey)
				{
					var foreignKey = new DbForeignKey(table.Name, column.Name);
					return new DbColumn(DbColumnType.Integer, name, foreignKey, allowNull);
				}
			}
			throw new Exception(@"No Primary Key column.");
		}

		public static DbColumn Integer(string name, bool allowNull = false)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new DbColumn(DbColumnType.Integer, name, allowNull);
		}

		public static DbColumn String(string name, bool allowNull = false)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new DbColumn(DbColumnType.String, name, allowNull);
		}

		public static DbColumn DateTime(string name, bool allowNull = false)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new DbColumn(DbColumnType.DateTime, name, allowNull);
		}

		public static DbColumn Decimal(string name, bool allowNull = false)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new DbColumn(DbColumnType.Decimal, name, allowNull);
		}
	}
}