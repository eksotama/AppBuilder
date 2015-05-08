using System;
using System.Text;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace AppBuilder
{
	public static class ClassGenerator
	{
		public static string GenerateObject(ClrClass @class, bool readOnly)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(2 * 1024);
			AppendClassDefinition(buffer, @class);
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			AppendProperties(buffer, @class, readOnly);
			buffer.AppendLine();
			AppendContructor(buffer, @class, readOnly);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string GenerateAdapter(ClrClass @class, bool readOnly, AdapterResultType resultType)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var buffer = new StringBuilder(2 * 1024);
			AppendClassDefinition(buffer, @class);
			buffer.Append(@"Adapter : AdapterBase");
			buffer.AppendLine(@"{");
			AppendConstructor(buffer, @class);
			AppendFillAllMethod(buffer, @class, resultType);
			AppendCreatorMethod(buffer, @class, readOnly);
			AppendSelectorMethod(buffer, @class, resultType);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class)
		{
			AppendDictionaryFields(buffer, @class);
			buffer.Append(@"public ");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter(QueryHelper queryHelper");
			AppendDictionaryParameters(buffer, @class);
			buffer.Append(@") : base(queryHelper)");
			buffer.AppendLine(@"{");
			AppendDictionaryChecks(buffer, @class);
			AppendDictionaryAssignments(buffer, @class);
			buffer.AppendLine(@"}");
			buffer.AppendLine();
		}

		private static void AppendDictionaryAssignments(StringBuilder buffer, ClrClass @class)
		{
			AppendDictionary(buffer, @class, ClrProperty.AppendDictionaryAssignment);
		}

		private static void AppendDictionaryChecks(StringBuilder buffer, ClrClass @class)
		{
			var oldLength = buffer.Length;
			AppendDictionary(buffer, @class, ClrProperty.AppendDictionaryParameterCheck);
			AppendLineIfChanged(buffer, oldLength);
		}

		private static void AppendDictionaryFields(StringBuilder buffer, ClrClass @class)
		{
			var oldLength = buffer.Length;
			AppendDictionary(buffer, @class, ClrProperty.AppendDictionaryField);
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
			AppendDictionary(buffer, @class, ClrProperty.AppendDictionaryParameter);
		}

		private static void AppendDictionary(StringBuilder buffer, ClrClass @class, Action<StringBuilder, ClrProperty> appender)
		{
			foreach (var property in @class.Properties)
			{
				var foreignKey = property.Column.ForeignKey;
				if (foreignKey != null)
				{
					appender(buffer, property);
				}
			}
		}

		private static void AppendFillAllMethod(StringBuilder buffer, ClrClass @class, AdapterResultType resultType)
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

		private static void AppendProperties(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			Action<StringBuilder, ClrProperty> appender = ClrProperty.AppendMutableProperty;
			if (readOnly)
			{
				appender = ClrProperty.AppendReadOnlyProperty;
			}
			foreach (var property in @class.Properties)
			{
				appender(buffer, property);
				buffer.AppendLine();
			}
		}

		private static void AppendContructor(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			Action<StringBuilder, ClrClass> appender = ClrClass.AppendEmptyConstructor;
			if (readOnly)
			{
				appender = ClrClass.AppendConstructorWithParameters;
			}
			appender(buffer, @class);
		}
	}
}