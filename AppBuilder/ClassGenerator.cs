using System;
using System.Linq;
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

			var buffer = new StringBuilder(1024 * 2);
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
			buffer.AppendLine(@"{");
			AppendConstructor(buffer, @class);
			AppendGetAllMethod(buffer, @class, resultType);
			AppendCreatorMethod(buffer, @class, readOnly);
			AppendSelectorMethod(buffer, @class, resultType);
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		//private static void AppendClassDefinition(StringBuilder buffer, ClrClass @class)
		//{
		//	ClassGenerator.AppendClassDefinition(buffer, @class);
		//	buffer.AppendLine(@"Adapter : AdapterBase");
		//}

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

		public static void AppendClassDefinition(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendPublicModifier(buffer);
			AppendSealedClass(buffer);
			AppendClassName(buffer, @class);
		}

		public static void AppendContructorName(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendPublicModifier(buffer);
			AppendClassName(buffer, @class);
		}

		public static void AppendPublicModifier(StringBuilder buffer)
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