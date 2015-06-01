using System;
using AppBuilder.Db.DDL;

namespace AppBuilder
{
	public static class HelperGenerator
	{
		public static string GenerateCode(DbTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			var template = @"	public sealed class {0}Helper
	{{
		private readonly Dictionary<long, {1}> _{2} = new Dictionary<long, {1}>();

		public Dictionary<long, {1}> {0} {{ get {{ return _{2}; }} }}

		public void Load({0}Adapter adapter)
		{{
			if (adapter == null) throw new ArgumentNullException(""adapter"");

			adapter.Fill(this.{0});
		}}
	}}";
			return string.Format(template, table.Name, table.ClassName, NameProvider.ToParameterName(table.Name));
		}
	}
}