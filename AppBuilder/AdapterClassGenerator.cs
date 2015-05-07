using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
	public sealed class Brand
	{
		public long BrandId { get; set; }
		public string Description { get; set; }
		public string LocalDescription { get; set; }

		public Brand()
		{
			this.BrandId = 0L;
			this.Description = string.Empty;
			this.LocalDescription = string.Empty;
		}
	}



	//public sealed class Brand
	//{
	//	public long BrandId { get; private set; }
	//	public string Description { get; private set; }
	//	public string LocalDescription { get; private set; }

	//	public Brand(long brandId, string description, string localDescription)
	//	{
	//		if (description == null) throw new ArgumentNullException("description");
	//		if (localDescription == null) throw new ArgumentNullException("localDescription");

	//		this.BrandId = brandId;
	//		this.Description = description;
	//		this.LocalDescription = localDescription;
	//	}
	//}






	public static class AdapterClassGenerator
	{
		public static string Generate(ClrClass @class, bool readOnly, AdapterResultType resultType)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(2 * 1024);

			AppendClassDefinition(buffer, @class);
			buffer.AppendLine(@"{");
			AppendConstructor(buffer, @class);
			AppendGetAllMethod(buffer, @class, resultType);
			AppendCreatorMethod(buffer, @class, readOnly);
			AppendSelectorMethod(buffer, @class, resultType);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendClassDefinition(StringBuilder buffer, ClrClass @class)
		{
			ClassGenerator.AppendClassDefinition(buffer, @class);
			buffer.AppendLine(@"Adapter : AdapterBase");
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class)
		{
			// TODO : !!! For adapters with classes with other object we need Dictionaries eagerly load objects 
			// and assigned to fields of the appropriate type
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.AppendLine(@"Adapter(QueryHelper queryHelper) : base(queryHelper) { } ");
			buffer.AppendLine();
		}

		private static void AppendGetAllMethod(StringBuilder buffer, ClrClass @class, AdapterResultType resultType)
		{
			string call;
			buffer.Append(@"public void Fill(");
			switch (resultType)
			{
				case AdapterResultType.List:
					buffer.Append(@"List<");
					call = @");";
					break;
				case AdapterResultType.Dictionary:
					buffer.Append(@"Dictionary<long,");
					call = @", this.Selector);";
					break;
				default:
					throw new ArgumentOutOfRangeException("resultType");
			}
			buffer.Append(@class.Name);
			buffer.AppendLine(@"> items) {");
			ClrProperty.AppendParameterCheck(buffer, @"items");
			buffer.AppendLine();
			buffer.Append(@"var query = """);
			DbTable.AppendSelectQuery(buffer, @class.Table);
			buffer.AppendLine(@""";");
			buffer.Append(@"this.QueryHelper.Fill(items, query, this.Creator");
			buffer.AppendLine(call);
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendCreatorMethod(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			var name = @class.Name;
			buffer.Append(@"private ");
			buffer.Append(name);
			buffer.AppendLine(@" Creator(IDataReader r)");
			buffer.AppendLine(@"{");

			var properties = @class.Properties;
			for (var index = 0; index < properties.Length; index++)
			{
				var property = properties[index];

				buffer.Append(@"var ");
				ClrProperty.AppendParameterName(buffer, property);
				buffer.Append(@" = ");
				ClrProperty.AppendDefaultValue(buffer, property);
				buffer.AppendLine(@";");
				buffer.Append(@"if (!r.IsDBNull(");
				buffer.Append(index);
				buffer.AppendLine(@")){");
				ClrProperty.AppendParameterName(buffer, property);
				buffer.Append(@" = ");
				ClrProperty.AppendDataReaderValue(buffer, property, index);
				buffer.AppendLine(@";}");
			}

			buffer.Append(@"return new ");
			buffer.Append(name);
			if (readOnly)
			{
				buffer.Append(@"(");
				ClrClass.AppendParameters(buffer, @class);
				buffer.Append(@")");
			}
			else
			{
				buffer.Append(@"{");
				ClrClass.AppendParametersAssignments(buffer, @class);
				buffer.Append(@"}");
			}
			buffer.AppendLine(@";");
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendSelectorMethod(StringBuilder buffer, ClrClass @class, AdapterResultType resultType)
		{
			if (resultType == AdapterResultType.Dictionary)
			{
				var name = @class.Name;
				var varName = char.ToLowerInvariant(name[0]);
				var primaryKeyProperty = default(ClrProperty);
				foreach (var property in @class.Properties)
				{
					if (property.Column.IsPrimaryKey)
					{
						primaryKeyProperty = property;
						break;
					}
				}

				buffer.Append(@"private long Selector(");
				buffer.Append(name);
				buffer.Append(@" ");
				buffer.Append(varName);
				buffer.Append(@") { return ");
				buffer.Append(varName);
				buffer.Append(@".");
				buffer.Append(primaryKeyProperty.Name);
				buffer.Append(@";}");
				buffer.AppendLine();
			}
		}
	}
}