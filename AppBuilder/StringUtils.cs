﻿using System;
using System.Text;

namespace AppBuilder
{
	public static class StringUtils
	{
		public static string UpperFirst(string input)
		{
			if (input == null) throw new ArgumentNullException("input");

			return ApplyFirst(input, char.ToUpperInvariant);
		}

		public static string LowerFirst(string input)
		{
			if (input == null) throw new ArgumentNullException("input");

			return ApplyFirst(input, char.ToLowerInvariant);
		}

		public static void UpperFirst(StringBuilder buffer, string name)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (name == null) throw new ArgumentNullException("name");
			if (name.Length == 0) throw new ArgumentOutOfRangeException("name");

			ApplyFirst(buffer, name, char.ToUpperInvariant);
		}

		private static string ApplyFirst(string input, Func<char, char> f)
		{
			if (input.Length > 0)
			{
				if (input.Length == 1)
				{
					return new string(f(input[0]), 1);
				}
				return f(input[0]) + input.Substring(1);
			}
			return input;
		}

		private static void ApplyFirst(StringBuilder buffer, string name, Func<char, char> f)
		{
			buffer[buffer.Length - name.Length] = f(name[0]);
		}

		public static string RemoveBrackets(string schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			var buffer = new StringBuilder(schema.Length);

			foreach (var symbol in schema)
			{
				if (symbol == '[' || symbol == ']')
				{
					continue;
				}
				buffer.Append(symbol);
			}

			return buffer.ToString();
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
	}
}