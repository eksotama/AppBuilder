using System;
using System.Text;
using AppBuilder.Clr;

namespace AppBuilder
{
	public static class ObjectGenerator
	{
		private static readonly char Space = ' ';
		private static readonly char Semicolon = ';';
		private static readonly char Comma = ',';
		private static readonly char Tab = '\t';

		public static string GenerateCode(ClrClass @class, bool readOnly)
		{
			if (@class == null) throw new ArgumentNullException("class");

			var name = @class.Name;
			var properties = @class.Properties;

			var buffer = new StringBuilder(1024);

			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(@"sealed");
			buffer.Append(Space);
			buffer.Append(@"class");
			buffer.Append(Space);
			buffer.Append(name);
			buffer.AppendLine();

			buffer.AppendLine(@"{");

			AppendProperties(properties, buffer, readOnly);
			AppendContructor(name, properties, buffer);

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		private static void AppendProperties(ClrProperty[] properties, StringBuilder buffer, bool readOnly)
		{
			foreach (var property in properties)
			{
				buffer.Append(Tab);
				buffer.Append(@"public");
				buffer.Append(Space);
				buffer.Append(property.Type.Name);
				buffer.Append(Space);
				var name = property.Name;
				buffer.Append(name);
				buffer.Append(Space);
				var access = @"{ get; private set; }";
				if (!readOnly && name == NameProvider.IdName)
				{
					access = @"{ get; set; }";
				}
				buffer.Append(access);
				buffer.AppendLine();
			}

			buffer.AppendLine();
		}

		private static void AppendContructor(string name, ClrProperty[] properties, StringBuilder buffer)
		{
			buffer.Append(Tab);
			buffer.Append(@"public");
			buffer.Append(Space);
			buffer.Append(name);
			buffer.Append(@"(");

			// Add parameters
			var parameterNames = new string[properties.Length];
			for (var i = 0; i < parameterNames.Length; i++)
			{
				parameterNames[i] = NameProvider.ToParameterName(properties[i].Name);
			}
			for (var i = 0; i < parameterNames.Length; i++)
			{
				var parameterName = parameterNames[i];
				if (i > 0)
				{
					buffer.Append(Comma);
					buffer.Append(Space);
				}
				buffer.Append(properties[i].Type.Name);
				buffer.Append(Space);
				buffer.Append(parameterName);
			}

			buffer.AppendLine(@")");

			buffer.Append(Tab);
			buffer.AppendLine(@"{");

			// Add parameter checks
			var hasChecks = false;
			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				if (property.Type.CheckValue)
				{
					hasChecks = true;
					buffer.Append(Tab);
					buffer.Append(Tab);
					buffer.AppendFormat(@"if ({0} == null) throw new ArgumentNullException(""{0}"");", parameterNames[i]);
					buffer.AppendLine();
				}
			}

			if (hasChecks)
			{
				buffer.AppendLine();
			}

			for (var i = 0; i < properties.Length; i++)
			{
				buffer.Append(Tab);
				buffer.Append(Tab);
				buffer.Append(@"this.");
				buffer.Append(properties[i].Name);
				buffer.Append(@" = ");
				buffer.Append(parameterNames[i]);
				buffer.Append(Semicolon);
				buffer.AppendLine();
			}

			buffer.Append(Tab);
			buffer.AppendLine(@"}");
		}
	}
}