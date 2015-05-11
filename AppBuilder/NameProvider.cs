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
			return name.Substring(0, name.Length - 1);
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

			StringUtils.UpperFirst(buffer, name);

			// SomeValueId => SomeValue
			if (isForeignKey)
			{
				var length = @"Id".Length;
				buffer.Remove(buffer.Length - length, length);
			}

			return buffer.ToString();
		}
	}
}