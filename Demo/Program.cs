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

			foreach (var t in DbSchemaParser.ParseTables(input))
			{
				Console.WriteLine(t.Name);
				Console.WriteLine(t.Columns.Length);
				Console.WriteLine();
				foreach (var c in t.Columns)
				{
					Console.WriteLine(c.Name + ", " + c.Type);
				}
			}
		}
	}
}
