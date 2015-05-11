using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AppBuilder
{
	public static class StringUtils
	{
		public static string UpperFirst(string input)
		{
			if (input == null) throw new ArgumentNullException("input");

			if (input.Length > 0)
			{
				if (input.Length == 1)
				{
					return new string(char.ToUpperInvariant(input[0]), 1);
				}
				return char.ToUpperInvariant(input[0]) + input.Substring(1);
			}
			return input;
		}

		public static string NormalizeTableSchema(string schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			// Remove NewLines
			var space = @" ";
			var value = schema.Trim().Replace(Environment.NewLine, space);

			// Remove brackets
			var buffer = new StringBuilder(value.Length);
			foreach (var symbol in value)
			{
				if (symbol == '[' || symbol == ']')
				{
					continue;
				}
				buffer.Append(symbol);
			}

			// Unify & normalize empty symbols
			return Regex.Replace(buffer.ToString(), @"[ \t]+", space);
		}

		public static string ExtractBetween(string input, string begin, string end)
		{
			if (input == null) throw new ArgumentNullException("input");

			var index = input.IndexOf(begin, StringComparison.OrdinalIgnoreCase);
			if (index >= 0)
			{
				var start = index + begin.Length;
				var stop = input.IndexOf(end, start, StringComparison.OrdinalIgnoreCase);
				if (stop >= 0)
				{
					return input.Substring(start, stop - start);
				}
				return input.Substring(start);
			}

			return string.Empty;
		}

		public static string ExtractBetweenGreedy(string input, string begin, string end)
		{
			if (input == null) throw new ArgumentNullException("input");

			var index = input.IndexOf(begin, StringComparison.OrdinalIgnoreCase);
			if (index >= 0)
			{
				var start = index + begin.Length;
				var stop = input.LastIndexOf(end, StringComparison.OrdinalIgnoreCase);
				return input.Substring(start, stop - start);
			}

			return string.Empty;
		}
	}
}