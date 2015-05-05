using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBuilder;
using AppBuilder.Db;

namespace Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			var input = @"
			CREATE TABLE [Brands] (
				[brand_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
				[description] char(100) NOT NULL, 
				[local_description] char(100) NOT NULL
			)";

			foreach (var table in DbSchemaParser.ParseTables(input))
			{
				Console.WriteLine(table.Name);
				Console.WriteLine(table.Columns.Length);
				Console.WriteLine();
				foreach (var c in table.Columns)
				{
					Console.WriteLine(c.Name + ", " + c.Type);
				}

				var obj = DbTableConverter.ToClrObject(table, new NameProvider());
				Console.WriteLine(obj.Name);
				Console.WriteLine(obj.Properties.Length);

				foreach (var p in obj.Properties)
				{
					Console.WriteLine(p.Name);
					Console.WriteLine(p.Type.Name);
					Console.WriteLine(p.Nullable);
					Console.WriteLine();
				}
			}
		}
	}
}
