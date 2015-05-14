using System;
using System.Collections.Generic;
using System.Text;

namespace AppBuilder
{
	public sealed class NameProvider
	{
		private readonly Dictionary<string, string> _overrides = new Dictionary<string, string>();

		public void AddOverride(string table, string @override)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (@override == null) throw new ArgumentNullException("override");

			_overrides.Add(table, @override);
		}

		public string GetClassName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			// Search in overrides for something like (Activities => Activity)
			string value;
			if (_overrides.TryGetValue(name, out value))
			{
				return value;
			}

			// Remove the last symbol, Probably it will be 's' (Users => User)
			return ToClassName(name.Substring(0, name.Length - 1));
		}

		public string GetDbName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			foreach (var kv in _overrides)
			{
				if (kv.Value.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return kv.Value;
				}
			}

			return name + @"s";
		}

		public string GetPropertyName(string name, bool isForeignKey)
		{
			if (name == null) throw new ArgumentNullException("name");

			//var name = column.Name;
			var buffer = new StringBuilder(name.Length);

			//some_value_id => SomeValueId
			var upperSymbol = false;
			foreach (var symbol in name)
			{
				if (symbol == '_')
				{
					upperSymbol = true;
					continue;
				}
				var current = symbol;
				if (upperSymbol)
				{
					current = char.ToUpperInvariant(symbol);
				}
				buffer.Append(current);
				upperSymbol = false;
			}

			buffer[0] = char.ToUpperInvariant(buffer[0]);

			// SomeValueId => SomeValue
			if (isForeignKey)
			{
				var length = @"Id".Length;
				buffer.Remove(buffer.Length - length, length);
			}

			return buffer.ToString();
		}

		public static string ToClassName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return ToUpperFirst(name);
		}

		public static string ToFieldName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			var flag = @"_";
			if (name.StartsWith(flag))
			{
				return name;
			}

			var buffer = new StringBuilder(name.Length + 1);

			buffer.Append('_');
			buffer.Append(name);
			buffer[1] = char.ToLowerInvariant(buffer[1]);

			return buffer.ToString();
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