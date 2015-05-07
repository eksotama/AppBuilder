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
		public string PropertyType { get; private set; }
		public string DefaultValue { get; private set; }
		public string ParameterName { get; private set; }
		public bool IsReferenceType { get; private set; }

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

			this.SetupPropertyType();
			this.SetupDefaultValue();
			this.ParameterName = char.ToLowerInvariant(name[0]) + name.Substring(1);
			this.IsReferenceType = true;
			if (type == ClrType.Integer || type == ClrType.Decimal || type == ClrType.DateTime)
			{
				this.IsReferenceType = nullable;
			}
		}

		private void SetupPropertyType()
		{
			var type = this.Type;
			this.PropertyType = type.Name;
			if (type == ClrType.Integer)
			{
				this.PropertyType = this.Nullable ? @"long?" : @"long";
				return;
			}
			if (type == ClrType.String)
			{
				this.PropertyType = @"string";
				return;
			}
			if (type == ClrType.Decimal)
			{
				this.PropertyType = this.Nullable ? @"decimal?" : @"decimal";
				return;
			}
			if (type == ClrType.DateTime)
			{
				this.PropertyType = this.Nullable ? @"DateTime?" : @"DateTime";
				return;
			}
			if (type == ClrType.Bytes)
			{
				this.PropertyType = @"byte[]";
			}
		}

		private void SetupDefaultValue()
		{
			if (this.Type == ClrType.Integer)
			{
				this.DefaultValue = this.Nullable ? @"null" : @"0L";
				return;
			}
			if (this.Type == ClrType.String)
			{
				this.DefaultValue = @"string.Empty";
				return;
			}
			if (this.Type == ClrType.Decimal)
			{
				this.DefaultValue = this.Nullable ? @"null" : @"0M";
				return;
			}
			if (this.Type == ClrType.DateTime)
			{
				if (this.Name.EndsWith(@"From"))
				{
					this.DefaultValue = @"DateTime.MinValue";
					return;
				}
				if (this.Name.EndsWith(@"To"))
				{
					this.DefaultValue = @"DateTime.MaxValue";
					return;
				}
				this.DefaultValue = this.Nullable ? @"null" : @"DateTime.MinValue";
				return;
			}
			if (this.Type == ClrType.Bytes)
			{
				this.DefaultValue = @"default(byte[])";
				return;
			}

			this.DefaultValue = @"default(" + this.Type.Name + @")";
		}

		public static void AppendMutableProperty(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendProperty(buffer, property, @" { get; set; }");
		}

		public static void AppendReadOnlyProperty(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendProperty(buffer, property, @" { get; private set; }");
		}

		private static void AppendProperty(StringBuilder buffer, ClrProperty property, string accessModifier)
		{
			buffer.Append(@"public ");
			buffer.Append(property.PropertyType);
			buffer.Append(@" ");
			buffer.Append(property.Name);
			buffer.Append(accessModifier);
		}

		public static void AppendInitToDefaultValue(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendInitializationTo(buffer, property, property.DefaultValue);
		}

		public static void AppendInitToParameterName(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendInitializationTo(buffer, property, property.ParameterName);
		}

		private static void AppendInitializationTo(StringBuilder buffer, ClrProperty property, string value)
		{
			buffer.Append(@"this.");
			buffer.Append(property.Name);
			buffer.Append(@" = ");
			buffer.Append(value);
			buffer.Append(@";");
		}

		public static void AppendParameter(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(property.PropertyType);
			buffer.Append(@" ");
			buffer.Append(property.ParameterName);
		}

		public static void AppendParameterName(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(property.ParameterName);
		}

		public static void AppendPropertyNameParameterName(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(property.Name);
			buffer.Append(@" = ");
			buffer.Append(property.ParameterName);
		}

		public static void AppendParameterCheck(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			AppendParameterCheck(buffer, property.ParameterName);
		}

		public static void AppendParameterCheck(StringBuilder buffer, string name)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (name == null) throw new ArgumentNullException("name");

			buffer.Append(@"if (");
			buffer.Append(name);
			buffer.Append(@" == null ) throw new ArgumentNullException(""");
			buffer.Append(name);
			buffer.Append(@""");");
		}








		//
		// TODO : !!!
		//
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

		public static void AppendDictionaryField(StringBuilder buffer, ClrProperty property)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (property == null) throw new ArgumentNullException("property");

			buffer.Append(@"private readonly ");
			buffer.Append(@"Dictionary<long,");
			throw new Exception(@"currently refactoring");
			//buffer.Append(GetPropertyType(property));
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
			//buffer.Append(GetPropertyType(property));
			throw new Exception(@"currently refactoring");
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