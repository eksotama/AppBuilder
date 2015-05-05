using System;
using System.Collections.Generic;
using System.Text;
using AppBuilder.Db;

namespace AppBuilder
{
	public sealed class NameProvider
	{
		private readonly Dictionary<string, string> _overrides = new Dictionary<string, string>();

		public void AddOverride(DbTable table, string @override)
		{
			_overrides.Add(table.Name, @override);
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

		public string GetPropertyName(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

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

			// Upper first
			buffer[0] = char.ToUpperInvariant(buffer[0]);

			return buffer.ToString();
		}
	}
}