using System;
using System.Collections.Generic;
using System.Text;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public sealed class AppGenerator
	{
		public static string GenerateEventArgsClass()
		{
			return @"

public sealed class HelperLoadedEventArgs : EventArgs
	{
		public string Name { get; private set; }

		public HelperLoadedEventArgs(string name)
		{
			if (name == null) throw new ArgumentNullException(""name"");

			this.Name = name;
		}
	}";
		}

		public static string GenerateCode(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			var buffer = new StringBuilder(1024);
			buffer.AppendLine(string.Format(@"public sealed class {0}App", schema.Name));
			buffer.AppendLine(@"{");

			buffer.AppendLine(@"public event EventHandler<HelperLoadedEventArgs> HelperLoaded;");
			buffer.AppendLine();

			buffer.AppendLine(@"private void OnHelperLoaded(HelperLoadedEventArgs e)");
			buffer.AppendLine(@"{");
			buffer.AppendLine(@"var handler = HelperLoaded;");
			buffer.AppendLine(@"if (handler != null) handler(this, e);");
			buffer.AppendLine(@"}");
			buffer.AppendLine();

			var tables = new List<DbTable>(schema.Tables);
			tables.Sort((x, y) =>
			{
				var cmp = HasForeignKey(x).CompareTo(HasForeignKey(y));
				if (cmp == 0)
				{
					cmp = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
				}
				return cmp;
			});

			foreach (var table in tables)
			{
				if (table.IsReadOnly)
				{
					var name = NameProvider.ToParameterName(table.Name);

					buffer.AppendLine(string.Format(@"private readonly {0}Helper _{1}Helper = new {0}Helper();", table.Name, name));
					buffer.AppendLine(string.Format(@"public {0}Helper {0}Helper", table.Name));
					buffer.AppendLine(@"{");
					buffer.AppendLine(string.Format(@"get {{ return _{0}Helper; }}", name));
					buffer.AppendLine(@"}");
					buffer.AppendLine();
				}
			}

			buffer.AppendLine(@"public void Load()");
			buffer.AppendLine(@"{");
			foreach (var table in tables)
			{
				if (table.IsReadOnly)
				{
					var parameters = new StringBuilder();
					foreach (var column in table.Columns)
					{
						var foreignKey = column.DbForeignKey;
						if (foreignKey != null)
						{
							if (parameters.Length > 0)
							{
								parameters.Append(@", ");
							}
							parameters.Append(@"this.");
							parameters.Append(foreignKey.Table);
							parameters.Append(@"Helper.");
							parameters.Append(foreignKey.Table);
						}
					}
					buffer.AppendLine(string.Format(@"this.{0}Helper.Load(new {0}Adapter({1}));", table.Name, parameters));
					buffer.AppendLine(string.Format(@"this.OnHelperLoaded(new HelperLoadedEventArgs(@""{0}""));", table.Name));
					buffer.AppendLine();
				}
			}
			buffer.AppendLine(@"}");

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static bool HasForeignKey(DbTable x)
		{
			foreach (var column in x.Columns)
			{
				if (column.DbForeignKey != null)
				{
					return true;
				}
			}
			return false;
		}
	}
}