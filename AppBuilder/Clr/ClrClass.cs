using System;
using System.Text;
using AppBuilder.Db;

namespace AppBuilder.Clr
{
	public sealed class ClrClass
	{
		public string Name { get; private set; }
		public ClrProperty[] Properties { get; private set; }
		public DbTable Table { get; private set; }

		public ClrClass(string name, ClrProperty[] properties, DbTable table)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (properties == null) throw new ArgumentNullException("properties");
			if (table == null) throw new ArgumentNullException("table");
			if (name == string.Empty) throw new ArgumentOutOfRangeException("name");
			if (properties.Length == 0) throw new ArgumentOutOfRangeException("properties");

			this.Name = name;
			this.Properties = properties;
			this.Table = table;
		}

		public static void AppendParameterDefinitions(StringBuilder buffer, ClrClass @class, bool readOnly)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			if (readOnly)
			{
				AppendParameters(buffer, @class, DefinitionAppender);
			}
		}

		public static void AppendParameters(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, ParameterNameAppender);
		}

		public static void AppendParametersAssignments(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, NameValueAppender);
		}

		private static void ParameterNameAppender(StringBuilder buffer, ClrProperty property)
		{
			ClrProperty.AppendParameterName(buffer, property);
		}

		private static void DefinitionAppender(StringBuilder buffer, ClrProperty property)
		{
			ClrProperty.AppendType(buffer, property);
			ParameterNameAppender(buffer, property);
		}

		private static void NameValueAppender(StringBuilder buffer, ClrProperty property)
		{
			ClrProperty.AppendName(buffer, property);
			buffer.Append(@" = ");
			ParameterNameAppender(buffer, property);
		}

		private static void AppendParameters(StringBuilder buffer, ClrClass @class, Action<StringBuilder, ClrProperty> appender)
		{
			var separator = @", ";
			foreach (var property in @class.Properties)
			{
				appender(buffer, property);
				buffer.Append(separator);
			}

			buffer.Remove(buffer.Length - separator.Length, separator.Length);
		}
	}
}