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

		public static void AppendParameters(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, ClrProperty.AppendParameter);
		}

		public static void AppendParameterNames(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, ClrProperty.AppendParameterName);
		}

		public static void AppendParametersAssignments(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendParameters(buffer, @class, ClrProperty.AppendPropertyNameParameterName);
		}

		private static void AppendParameters(StringBuilder buffer, ClrClass @class, Action<StringBuilder, ClrProperty> appender)
		{
			var addSeparator = false;
			foreach (var property in @class.Properties)
			{
				if (addSeparator)
				{
					buffer.Append(@", ");
				}
				appender(buffer, property);
				addSeparator = true;
			}
		}

		public static void AppendEmptyConstructor(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			ClassGenerator.AppendContructorName(buffer, @class);
			buffer.Append(@"()");
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			foreach (var property in @class.Properties)
			{
				ClrProperty.AppendInitToDefaultValue(buffer, property);
				buffer.AppendLine();
			}
			buffer.AppendLine(@"}");
		}

		public static void AppendConstructorWithParameters(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			ClassGenerator.AppendContructorName(buffer, @class);
			buffer.Append(@"(");
			AppendParameters(buffer, @class);
			buffer.Append(@")");
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			var current = buffer.Length;
			foreach (var property in @class.Properties)
			{
				if (property.IsReferenceType)
				{
					ClrProperty.AppendParameterCheck(buffer, property);
					buffer.AppendLine();
				}
			}
			// Separate argument checks with properies assignments
			if (current != buffer.Length)
			{
				buffer.AppendLine();
			}
			foreach (var property in @class.Properties)
			{
				ClrProperty.AppendInitToParameterName(buffer, property);
				buffer.AppendLine();
			}
			buffer.AppendLine(@"}");
		}
	}
}