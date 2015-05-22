using System;
using System.Text;

namespace AppBuilder
{
	public static class NameProvider
	{
		public static readonly string IdName = @"Id";

		public static string GetDbForeignKeyName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return name + IdName;
		}

		public static string GetPropertyName(string name, bool isForeignKey)
		{
			if (name == null) throw new ArgumentNullException("name");

			var buffer = new StringBuilder(name);

			buffer[0] = char.ToUpperInvariant(buffer[0]);

			// SomeValueId => SomeValue
			if (isForeignKey)
			{
				var length = IdName.Length;
				buffer.Remove(buffer.Length - length, length);
			}

			return buffer.ToString();
		}

		public static string ToClassName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return ToUpperFirst(name);
		}

		public static string ToPropertyName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return ToUpperFirst(name);
		}

		public static string ToParamterName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			var lower = name[0];
			var upper = char.ToLowerInvariant(lower);
			if (lower == upper)
			{
				return name;
			}
			return upper + name.Substring(1);
		}

		public static string ToTableName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return ToUpperFirst(name);
		}

		public static string ToColumnName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return ToUpperFirst(name);
		}

		private static string ToUpperFirst(string name)
		{
			var first = name[0];
			var upper = char.ToUpperInvariant(first);
			if (first == upper)
			{
				return name;
			}
			return upper + name.Substring(1);
		}
	}
}