using System;
using System.Collections.Generic;
using System.IO;
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

			input = @"

CREATE TABLE [Channels] (
	[channel_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[description] char(100) NOT NULL, 
	[local_description] char(100) NOT NULL, 
	[sap_channel_id] char(10) NOT NULL,
	[channel_group_id] integer,  
	FOREIGN KEY ([channel_group_id])
		REFERENCES [ChannelGroups] ([channel_group_id])
		ON UPDATE NO ACTION ON DELETE NO ACTION
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

				var nameProvider = new NameProvider();
				var obj = DbTableConverter.ToClrObject(table, nameProvider);
				Console.WriteLine(obj.Name);
				Console.WriteLine(obj.Properties.Length);

				foreach (var p in obj.Properties)
				{
					Console.WriteLine(p.Name);
					Console.WriteLine(p.Type.Name);
					Console.WriteLine(p.Nullable);
					Console.WriteLine();
				}

				var mut = ClassGenerator.GenerateObjectClass(obj, false);
				Console.WriteLine(mut);

				var immut = ClassGenerator.GenerateObjectClass(obj, true);
				Console.WriteLine(immut);

				var total = mut + Environment.NewLine + immut;
				File.WriteAllText(@"C:\temp\obj.cs", total);
			}
		}
	}
}
