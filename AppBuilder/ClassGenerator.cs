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

			AppendPublicModifier(buffer);
			AppendSealedClass(buffer);
			AppendClassName(buffer, @class);
		}

		public static void AppendContructorName(StringBuilder buffer, ClrClass @class)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (@class == null) throw new ArgumentNullException("class");

			AppendPublicModifier(buffer);
			AppendClassName(buffer, @class);
		}

		public static void AppendPublicModifier(StringBuilder buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			buffer.Append(@"public ");
		}

		private static void AppendSealedClass(StringBuilder buffer)
		{
			buffer.Append(@"sealed class ");
		}

		private static void AppendClassName(StringBuilder buffer, ClrClass @class)
		{
			buffer.Append(@class.Name);
		}
	}
}