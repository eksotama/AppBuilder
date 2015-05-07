using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
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
			var hasObjectProperties = @class.Properties.Any(p => p.Column.ForeignKey != null);
			if (hasObjectProperties)
			{
				foreach (var property in @class.Properties)
				{
					if (property.Column.ForeignKey != null)
					{
						ClrProperty.AppendDictionaryField(buffer, property);
					}
				}
				buffer.AppendLine();
			}
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter(QueryHelper queryHelper");
			if (hasObjectProperties)
			{
				foreach (var property in @class.Properties)
				{
					if (property.Column.ForeignKey != null)
					{
						ClrProperty.AppendDictionaryParameter(buffer, property);
					}
				}
			}
			buffer.AppendLine(@") : base(queryHelper)");
			buffer.AppendLine(@"{");
			if (hasObjectProperties)
			{
				foreach (var property in @class.Properties)
				{
					var foreignKey = property.Column.ForeignKey;
					if (foreignKey != null)
					{
						ClrProperty.AppendParameterCheck(buffer, foreignKey.Table);
					}
				}
				foreach (var property in @class.Properties)
				{
					var foreignKey = property.Column.ForeignKey;
					if (foreignKey != null)
					{
						ClrProperty.AppendFieldAssignment(buffer, property);
					}
				}
			}
			buffer.AppendLine(@"}");
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
				buffer.Append(property.ParameterName);
				buffer.Append(@" = ");
				buffer.Append(property.DefaultValue);
				buffer.AppendLine(@";");
				buffer.Append(@"if (!r.IsDBNull(");
				buffer.Append(index);
				buffer.AppendLine(@")){");
				buffer.Append(property.ParameterName);
				buffer.Append(@" = ");
				ClrProperty.AppendDataReaderValue(buffer, property, index);
				buffer.AppendLine(@";}");
			}

			buffer.Append(@"return new ");
			buffer.Append(name);
			if (readOnly)
			{
				buffer.Append(@"(");
				ClrClass.AppendParameterNames(buffer, @class);
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