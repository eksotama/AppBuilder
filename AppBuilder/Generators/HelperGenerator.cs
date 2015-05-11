using System;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder.Generators
{
	public static class HelperGenerator
	{
		public static string Generate(NameProvider nameProvider, ClrClass @class)
		{
			if (@class == null) throw new ArgumentNullException("class");
			if (nameProvider == null) throw new ArgumentNullException("nameProvider");

			var buffer = new StringBuilder(1024);
			var varName = StringUtils.LowerFirst(nameProvider.GetDbName(@class.Name));

			buffer.Append(@"public sealed class ");
			buffer.Append(@class.Name);
			buffer.Append(@"Helper");
			buffer.AppendLine(@"{");

			// Field
			buffer.Append(@"private readonly Dictionary<long, ");
			buffer.Append(@class.Name);
			buffer.Append(@"> _");
			buffer.Append(varName);
			buffer.Append(@" = new Dictionary<long, ");
			buffer.Append(@class.Name);
			buffer.AppendLine(@">();");

			// Property
			buffer.AppendLine();
			buffer.Append(@"public Dictionary<long, ");
			buffer.Append(@class.Name);
			buffer.Append(@"> ");
			buffer.Append(varName);
			StringUtils.UpperFirst(buffer, varName);
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			buffer.Append(@"get { return _");
			buffer.Append(varName);
			buffer.Append(@"; }");
			buffer.AppendLine(@"}");
			buffer.AppendLine();

			// Method
			buffer.Append(@"public void Load(");
			buffer.Append(@class.Name);
			buffer.Append(@"Adapter adapter)");
			buffer.AppendLine(@"{");
			buffer.AppendLine(@"if (adapter == null) throw new ArgumentNullException(""adapter"");");
			buffer.AppendLine();
			buffer.Append(@"adapter.Fill(_");
			buffer.Append(varName);
			buffer.AppendLine(@");");
			buffer.AppendLine(@"}");
			buffer.AppendLine(@"}");

			return buffer.ToString();
		}
	}
}