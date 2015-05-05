using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	//public sealed class Brand
	//{
	//	public long BrandId { get; set; }
	//	public string Description { get; set; }
	//	public string LocalDescription { get; set; }

	//	public Brand()
	//	{
	//		this.Description = string.Empty;
	//		this.LocalDescription = string.Empty;
	//	}
	//}

	public sealed class Brand
	{
		public long BrandId { get; private set; }
		public string Description { get; private set; }
		public string LocalDescription { get; private set; }

		public Brand(long brandId, string description, string localDescription)
		{
			if (description == null) throw new ArgumentNullException("description");
			if (localDescription == null) throw new ArgumentNullException("localDescription");

			this.BrandId = brandId;
			this.Description = description;
			this.LocalDescription = localDescription;
		}
	}

	public static class AdapterClassGenerator
	{
		public static string Generate(ClrClass @class, bool readOnly, AdapterResultType resultType)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(2 * 1024);

			buffer.Append(@"public sealed class ");
			buffer.Append(@class.Name);
			buffer.AppendLine(@"Adapter : AdapterBase");
			buffer.AppendLine(@"{");

			AppendConstructor(buffer, @class);
			AppendGetAllMethod(buffer, @class, resultType);
			AppendCreatorMethod(buffer, @class, readOnly);
			//if (resultType == AdapterResultType.Dictionary)
			{
				AppendSelectorMethod(buffer, @class);
			}

			buffer.AppendLine(@"}");

			//public List<Zone> GetAll()
			//{
			//	var query = @"
			//	SELECT ZONE_ID    ,
			//		   DESCRIPTION,
			//		   LOCAL_DESCRIPTION
			//	FROM   ZONES
			//	ORDER BY LOCAL_DESCRIPTION";

			//	return this.QueryHelper.ExecuteReader(query, this.ZoneCreator, 24);
			//}

			//private Zone ZoneCreator(IDataReader r)
			//{
			//	var id = r.GetInt32(0);
			//	var description = string.Empty;
			//	if (!r.IsDBNull(1))
			//	{
			//		description = r.GetString(1).Trim(TrimSymbols);
			//	}
			//	var localDescription = string.Empty;
			//	if (!r.IsDBNull(2))
			//	{
			//		localDescription = r.GetString(2).Trim(TrimSymbols);
			//	}


			//	return new Zone(id, this.GetLocal(localDescription, description));
			//}

			return buffer.ToString();
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class)
		{
			// TODO : !!! For adapters with classes with other object we need Dictionaries eagerly load objects
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.AppendLine(@"Adapter(QueryHelper queryHelper) : base(queryHelper) { } ");
			buffer.AppendLine();
		}

		private static void AppendGetAllMethod(StringBuilder buffer, ClrClass @class, AdapterResultType resultType)
		{
			buffer.Append(@"public void Fill(");
			switch (resultType)
			{
				case AdapterResultType.List:
					buffer.Append(@"List<");
					break;
				case AdapterResultType.Dictionary:
					buffer.Append(@"Dictionary<long,");
					break;
				default:
					throw new ArgumentOutOfRangeException("resultType");
			}
			buffer.Append(@class.Name);
			buffer.AppendLine(@"> items) {");
			buffer.AppendLine(@"if (items == null) throw new ArgumentNullException(""items"");");
			buffer.AppendLine();
			buffer.Append(@"var query = """);
			AppendSelectQuery(buffer, @class);
			buffer.AppendLine(@""";");
			buffer.Append(@"this.QueryHelper.Fill(items, query, this.");
			buffer.Append(@class.Name);
			buffer.AppendLine(@"Creator);");
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendSelectQuery(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@"SELECT ");

			var table = @class.Table;
			var addSeparator = false;
			foreach (var column in table.Columns)
			{
				if (addSeparator)
				{
					buffer.Append(@", ");
				}
				buffer.Append(column.Name);
				addSeparator = true;
			}

			buffer.Append(@" FROM ");
			buffer.Append(table.Name);
		}

		private static void AppendCreatorMethod(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			buffer.Append(@"private ");
			buffer.Append(@class.Name);
			buffer.Append(@" ");
			buffer.Append(@class.Name);
			buffer.AppendLine(@"Creator(IDataReader r){");

			var properties = @class.Properties;
			for (var index = 0; index < properties.Length; index++)
			{
				var property = properties[index];
				var name = property.Name;

				buffer.Append(@"var ");
				buffer.Append(name);
				buffer[buffer.Length - name.Length] = char.ToLowerInvariant(name[0]);
				buffer.Append(@" = ");
				AppendDefaultPropertyValue(buffer, property);

				// TODO : !!! Read value from the reader if not dbNULL

				buffer.AppendLine(@";");
				buffer.AppendLine();
			}

			if (readOnly)
			{
				// TODO : !!!
				buffer.Append(@"return new Brand(");

				var addSeparator = false;
				foreach (var property in properties)
				{
					if (addSeparator)
					{
						buffer.Append(@", ");
					}
					var name = property.Name;
					buffer.Append(name);
					buffer[buffer.Length - name.Length] = char.ToLowerInvariant(name[0]);
					addSeparator = true;
				}

				buffer.AppendLine(@");");
			}
			else
			{
				// TODO : !!!
			}
			//buffer.AppendLine(@"return new Brand();");
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendDefaultPropertyValue(StringBuilder buffer, ClrProperty property)
		{
			var type = property.Type;
			if (type == ClrType.Integer)
			{
				buffer.Append(property.Nullable ? @"default(long?)" : @"0L");
				return;
			}
			if (type == ClrType.Decimal)
			{
				buffer.Append(property.Nullable ? @"default(decimal?)" : @"0M");
				return;
			}
			if (type == ClrType.DateTime)
			{
				buffer.Append(property.Nullable ? @"default(DateTime?)" : @"DateTime.MinValue");
				return;
			}
			if (type == ClrType.String)
			{
				buffer.Append(@"string.Empty");
				return;
			}
			if (type == ClrType.Bytes)
			{
				buffer.Append(@"default(byte[])");
				return;
			}

			buffer.Append(@"default(");
			buffer.Append(type.Name);
			buffer.Append(@")");
		}

		private static void AppendSelectorMethod(StringBuilder buffer, ClrClass @class)
		{
			var name = char.ToLowerInvariant(@class.Name[0]);
			var baseProperty = default(ClrProperty);
			foreach (var property in @class.Properties)
			{
				if (property.Column.IsPrimaryKey)
				{
					baseProperty = property;
					break;
				}
			}

			buffer.Append(@"private long ");
			buffer.Append(@class.Name);
			buffer.Append(@"Selector(");
			buffer.Append(@class.Name);
			buffer.Append(@" ");
			buffer.Append(name);
			buffer.Append(@")");
			buffer.Append(@"{");
			buffer.Append(@"return ");
			buffer.Append(name);
			buffer.Append(@".");
			buffer.Append(baseProperty.Name);
			buffer.Append(@";");
			buffer.AppendLine(@"}");
		}
	}
}