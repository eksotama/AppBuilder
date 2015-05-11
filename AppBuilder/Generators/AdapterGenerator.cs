using System;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Clr.Providers;

namespace AppBuilder.Generators
{
	public static class AdapterGenerator
	{
		public static string Generate(ClrClass @class, bool readOnly, AdapterResultType resultType)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(2 * 1024);
			buffer.Append(@"public sealed class ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter");
			buffer.AppendLine(@"{");
			AppendConstructor(buffer, @class);
			//AppendFillAllMethod(buffer, @class, resultType, DbQuery.GetSelect(table));
			//AppendCreatorMethod(buffer, @class, readOnly);
			//AppendSelectorMethod(buffer, @class, resultType);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class)
		{
			AppendDictionaryFields(buffer, @class);
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter(");
			AppendDictionaryParameters(buffer, @class);
			buffer.Append(@")");
			buffer.AppendLine(@"{");
			AppendDictionaryChecks(buffer, @class);
			AppendDictionaryAssignments(buffer, @class);
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendDictionaryFields(StringBuilder buffer, ClrClass @class)
		{
			var oldLength = buffer.Length;
			AppendDictionary(buffer, @class, AppendDictionaryField);
			AppendLineIfChanged(buffer, oldLength);
		}





		public static void AppendDataReaderValue(StringBuilder buffer, ClrProperty property, int index)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			var methodCall = string.Empty;
			var type = property.Type;
			if (type == ClrType.Integer)
			{
				methodCall = @"r.GetInt64(";
			}
			if (type == ClrType.Decimal)
			{
				methodCall = @"r.GetDecimal(";
			}
			if (type == ClrType.DateTime)
			{
				methodCall = @"r.GetDateTime(";
			}
			if (type == ClrType.String)
			{
				methodCall = @"r.GetString(";
			}
			if (type == ClrType.Bytes)
			{
				methodCall = @"r.GetBytes(";
			}
			if (methodCall != string.Empty)
			{
				buffer.Append(methodCall);
				buffer.Append(index);
				buffer.Append(@")");
			}
			else
			{
				buffer.Append(@"_");
				// TODO : !!!
				//var name = property.Column.ForeignKey.Table;
				//buffer.Append(name);
				//StringUtils.LowerFirst(buffer, name);
				buffer.Append(@"[r.GetInt64(");
				buffer.Append(index);
				buffer.Append(@")]");
			}
		}

		public static void AppendDictionaryField(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@"private readonly ");
			AppendDictionaryDefinition(buffer, property);
			buffer.Append(@"_");
			AppendForeignKeyVariableName(buffer, property);
			buffer.AppendLine(@";");
		}

		private static void AppendForeignKeyVariableName(StringBuilder buffer, ClrProperty property)
		{
			//var name = property.Column.ForeignKey.Table;
			//buffer.Append(name);
			//StringUtils.LowerFirst(buffer, name);
		}

		public static void AppendDictionaryParameter(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@", ");
			AppendDictionaryDefinition(buffer, property);
			AppendForeignKeyVariableName(buffer, property);
		}

		private static void AppendDictionaryDefinition(StringBuilder buffer, ClrProperty property)
		{
			buffer.Append(@"Dictionary<long,");
			buffer.Append(PropertyTypeProvider.GetPropertyType(property));
			buffer.Append(@"> ");
		}

		public static void AppendDictionaryParameterCheck(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			//AppendParameterCheck(buffer, property.Column.ForeignKey.Table);
		}

		public static void AppendDictionaryAssignment(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@"_");
			//var name = property.Column.ForeignKey.Table;
			//buffer.Append(name);
			//StringUtils.LowerFirst(buffer, name);
			//buffer.Append(@" = ");
			//buffer.Append(name);
			//StringUtils.LowerFirst(buffer, name);
			buffer.AppendLine(@";");
		}



		private static void AppendDictionaryAssignments(StringBuilder buffer, ClrClass @class)
		{
			AppendDictionary(buffer, @class, AppendDictionaryAssignment);
		}

		private static void AppendDictionaryChecks(StringBuilder buffer, ClrClass @class)
		{
			var oldLength = buffer.Length;
			AppendDictionary(buffer, @class, AppendDictionaryParameterCheck);
			AppendLineIfChanged(buffer, oldLength);
		}



		private static void AppendLineIfChanged(StringBuilder buffer, int oldLength)
		{
			var hasFields = (oldLength != buffer.Length);
			if (hasFields)
			{
				buffer.AppendLine();
			}
		}

		private static void AppendDictionaryParameters(StringBuilder buffer, ClrClass @class)
		{
			AppendDictionary(buffer, @class, AppendDictionaryParameter);
		}

		private static void AppendDictionary(StringBuilder buffer, ClrClass @class, Action<StringBuilder, ClrProperty> appender)
		{
			foreach (var property in @class.Properties)
			{
				//var foreignKey = property.Column.ForeignKey;
				//if (foreignKey != null)
				//{
				//	appender(buffer, property);
				//}
			}
		}

		private static void AppendFillAllMethod(StringBuilder buffer, ClrClass @class, AdapterResultType resultType, string query)
		{
			string call;
			buffer.Append(@"public void FillAll(");
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
			//ClrProperty.AppendParameterCheck(buffer, @"items");
			buffer.AppendLine();
			buffer.Append(@"var query = """);
			buffer.Append(query);
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
				//buffer.Append(property.ParameterName);
				buffer.Append(@" = ");
				//buffer.Append(property.DefaultValue);
				buffer.AppendLine(@";");
				buffer.Append(@"if (!r.IsDBNull(");
				buffer.Append(index);
				buffer.AppendLine(@")){");
				//buffer.Append(property.ParameterName);
				buffer.Append(@" = ");
				AppendDataReaderValue(buffer, property, index);
				buffer.AppendLine(@";}");
			}

			buffer.Append(@"return new ");
			buffer.Append(name);
			if (readOnly)
			{
				buffer.Append(@"(");
				//ClrClass.AppendParameterNames(buffer, @class);
				buffer.Append(@")");
			}
			else
			{
				buffer.Append(@"{");
				//ClrClass.AppendParametersAssignments(buffer, @class);
				buffer.Append(@"}");
			}
			buffer.AppendLine(@";");
			buffer.AppendLine(@"}");
		}

		private static void AppendSelectorMethod(StringBuilder buffer, ClrClass @class, AdapterResultType resultType)
		{
			if (resultType == AdapterResultType.Dictionary)
			{
				buffer.AppendLine();

				var name = @class.Name;
				var varName = char.ToLowerInvariant(name[0]);
				var primaryKeyProperty = default(ClrProperty);
				foreach (var property in @class.Properties)
				{
					//if (property.Column.IsPrimaryKey)
					//{
					//	primaryKeyProperty = property;
					//	break;
					//}
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

		private static void AppendClassDefinition(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendPublicModifier(buffer);
			AppendSealedClass(buffer);
			AppendClassName(buffer, @class);
		}

		private static void AppendPublicModifier(StringBuilder buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			buffer.Append(@"public ");
		}

		private static void AppendSealedClass(StringBuilder buffer)
		{
			buffer.Append(@"sealed class ");
		}

		private static void AppendClassName(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@class.Name);
		}
	}
}