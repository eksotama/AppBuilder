using System;
using System.Text;
using AppBuilder.Db;

namespace AppBuilder.Clr
{
	public sealed class ClrProperty
	{
		public string Name { get; private set; }
		public ClrType Type { get; private set; }
		public bool Nullable { get; private set; }
		public DbColumn Column { get; private set; }

		public ClrProperty(string name, ClrType type, bool nullable, DbColumn column)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (type == null) throw new ArgumentNullException("type");
			if (column == null) throw new ArgumentNullException("column");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");

			this.Name = name;
			this.Type = type;
			this.Nullable = nullable;
			this.Column = column;
		}

		public static bool IsReferenceType(ClrProperty property)
		{
			if (property == null) throw new ArgumentNullException("property");

			var type = property.Type;
			if (type == ClrType.Integer || type == ClrType.Decimal || type == ClrType.DateTime)
			{
				return property.Nullable;
			}
			return true;
		}

		public static void AppendPublicReadOnlyProperty(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendPublicProperty(buffer, property);
			buffer.AppendLine(@" { get; private set; }");
		}

		public static void AppendPublicMutableProperty(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendPublicProperty(buffer, property);
			buffer.AppendLine(@" { get; set; }");
		}

		private static void AppendPublicProperty(StringBuilder buffer, ClrProperty property)
		{
			buffer.Append(@"public ");
			AppendType(buffer, property);
			AppendName(buffer, property);
		}

		public static void AppendType(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(GetPropertyType(property));
			buffer.Append(@" ");
		}

		public static void AppendDefaultValue(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			var type = property.Type;
			if (type == ClrType.Integer)
			{
				buffer.Append(property.Nullable ? @"null" : @"0L");
				return;
			}
			if (type == ClrType.Decimal)
			{
				buffer.Append(property.Nullable ? @"null" : @"0M");
				return;
			}
			if (type == ClrType.DateTime)
			{
				if (property.Name.EndsWith(@"From"))
				{
					buffer.Append(@"DateTime.MinValue");
					return;
				}
				if (property.Name.EndsWith(@"To"))
				{
					buffer.Append(@"DateTime.MaxValue");
					return;
				}
				buffer.Append(property.Nullable ? @"null" : @"DateTime.MinValue");
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

		public static void AppendName(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			var propertyName = property.Name;

			var type = property.Type;
			if (type.IsBuiltIn)
			{
				buffer.Append(propertyName);
			}
			else
			{
				AppendParameterNameForObject(buffer, propertyName);
			}
		}

		private static void AppendParameterNameForObject(StringBuilder buffer, string propertyName)
		{
			// Property to other object. Remove ...Id to obtain object property name
			var length = propertyName.Length - @"Id".Length;
			for (var i = 0; i < length; i++)
			{
				buffer.Append(propertyName[i]);
			}
		}

		private static void AppendNameForObjectParameter(StringBuilder buffer, string propertyName)
		{
			AppendParameterNameForObject(buffer, propertyName);
			buffer[buffer.Length - (propertyName.Length - @"Id".Length)] = char.ToLowerInvariant(propertyName[0]);
		}

		public static void AppendParameterName(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			var propertyName = property.Name;

			// Parameter name - Property name with lowered first letter
			var type = property.Type;
			if (type.IsBuiltIn)
			{
				AppendParameterName(buffer, propertyName);
			}
			else
			{
				AppendNameForObjectParameter(buffer, propertyName);
			}
		}

		private static void AppendParameterName(StringBuilder buffer, string name)
		{
			buffer.Append(name);
			buffer[buffer.Length - name.Length] = char.ToLowerInvariant(name[0]);
		}

		private static string GetPropertyType(ClrProperty property)
		{
			var type = property.Type;
			if (type == ClrType.Integer)
			{
				return property.Nullable ? @"long?" : @"long";
			}
			if (type == ClrType.Decimal)
			{
				return property.Nullable ? @"decimal?" : @"decimal";
			}
			if (type == ClrType.DateTime)
			{
				return property.Nullable ? @"DateTime?" : @"DateTime";
			}
			if (type == ClrType.String)
			{
				return @"string";
			}
			if (type == ClrType.Bytes)
			{
				return @"byte[]";
			}
			return type.Name;
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
				var table = property.Column.ForeignKey.Table;
				foreach (var symbol in table)
				{
					buffer.Append(symbol);
				}
				buffer[buffer.Length - table.Length] = char.ToLowerInvariant(table[0]);
				buffer.Append(@"[r.GetInt64(");
				buffer.Append(index);
				buffer.Append(@")]");
			}
		}

		public static void AppendParameterCheck(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			var propertyName = property.Name;

			// Parameter name - Property name with lowered first letter
			var type = property.Type;
			if (type.IsBuiltIn)
			{
				AppendParameterCheck(buffer, property.Name);
			}
			else
			{
				AppendParameterCheck(buffer, propertyName, AppendNameForObjectParameter);
			}
		}

		public static void AppendParameterCheck(StringBuilder buffer, string name)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (name == null) throw new ArgumentNullException("name");

			AppendParameterCheck(buffer, name, AppendParameterName);
		}

		private static void AppendParameterCheck(StringBuilder buffer, string name, Action<StringBuilder, string> appender)
		{
			buffer.Append(@"if (");
			appender(buffer, name);
			buffer.Append(@" == null ) throw new ArgumentNullException(""");
			appender(buffer, name);
			buffer.AppendLine(@""");");
		}

		public static void AppendDictionaryField(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@"private readonly ");
			buffer.Append(@"Dictionary<long,");
			buffer.Append(GetPropertyType(property));
			buffer.Append(@"> _");
			var value = property.Column.ForeignKey.Table;
			buffer.Append(value);
			buffer[buffer.Length - value.Length] = char.ToLowerInvariant(value[0]);
			buffer.AppendLine(@";");
		}

		public static void AppendDictionaryParameter(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@", ");
			buffer.Append(@"Dictionary<long,");
			buffer.Append(GetPropertyType(property));
			buffer.Append(@"> ");
			var value = property.Column.ForeignKey.Table;
			buffer.Append(value);
			buffer[buffer.Length - value.Length] = char.ToLowerInvariant(value[0]);
		}

		public static void AppendFieldAssignment(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@"_");
			var value = property.Column.ForeignKey.Table;
			buffer.Append(value);
			buffer[buffer.Length - value.Length] = char.ToLowerInvariant(value[0]);
			buffer.Append(@" = ");
			buffer.Append(value);
			buffer[buffer.Length - value.Length] = char.ToLowerInvariant(value[0]);
			buffer.AppendLine(@";");
		}
	}
}