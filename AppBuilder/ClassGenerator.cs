using System;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	public static class ClassGenerator
	{
		public static void AppendClassDefinition(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			buffer.Append(@"public sealed class ");
			buffer.Append(@class.Name);
		}

		public static void AppendContructorName(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			buffer.Append(@"public ");
			buffer.Append(@class.Name);
		}
	}
}