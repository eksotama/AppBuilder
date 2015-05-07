using System;
using System.Collections.Generic;
using System.Data;
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
		//public sealed class BrandAdapter : AdapterBase
		//{
		//	public BrandAdapter(QueryHelper queryHelper) : base(queryHelper) { }

		//	public void Fill(List<Brand> items)
		//	{
		//		if (items == null) throw new ArgumentNullException("items");

		//		var query = "SELECT brand_id, description, local_description FROM Brands";
		//		this.QueryHelper.Fill(items, query, this.BrandCreator);
		//	}

		//	private Brand BrandCreator(IDataReader r)
		//	{
		//		var brandId = 0L;

		//		var description = string.Empty;

		//		var localDescription = string.Empty;

		//		return new Brand(brandId, description, localDescription);
		//	}

		//	private long BrandSelector(Brand b) { return b.BrandId; }
		//}


		static void Main(string[] args)
		{
			var input = @"
			CREATE TABLE [Brands] (
				[brand_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
				[description] char(100) NOT NULL, 
				[local_description] char(100) NOT NULL
			)";

//			input = @"
//
//CREATE TABLE [Channels] (
//	[channel_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[description] char(100) NOT NULL, 
//	[local_description] char(100) NOT NULL, 
//	[sap_channel_id] char(10) NOT NULL,
//	[channel_group_id] integer,  
//	FOREIGN KEY ([channel_group_id])
//		REFERENCES [ChannelGroups] ([channel_group_id])
//		ON UPDATE NO ACTION ON DELETE NO ACTION
//)";


			foreach (var table in DbSchemaParser.ParseTables(input))
			{
				//Console.WriteLine(table.Name);
				//Console.WriteLine(table.Columns.Length);
				//Console.WriteLine();
				//foreach (var c in table.Columns)
				//{
				//	Console.WriteLine(c.Name + ", " + c.Type);
				//}

				var nameProvider = new NameProvider();
				var obj = DbTableConverter.ToClrObject(table, nameProvider);
				//Console.WriteLine(obj.Name);
				//Console.WriteLine(obj.Properties.Length);

				//foreach (var p in obj.Properties)
				//{
				//	Console.WriteLine(p.Name);
				//	Console.WriteLine(p.Type.Name);
				//	Console.WriteLine(p.Nullable);
				//	Console.WriteLine();
				//}

				//var mut = ObjectClassGenerator.Generate(obj, false);
				//var immut = ObjectClassGenerator.Generate(obj, true);

				var mut = AdapterClassGenerator.Generate(obj, false, AdapterResultType.Dictionary);
				var immut = AdapterClassGenerator.Generate(obj, true, AdapterResultType.Dictionary);

				var total = mut + Environment.NewLine + Environment.NewLine + Environment.NewLine + immut;
				File.WriteAllText(@"C:\temp\obj.cs", total);
			}
		}
	}



}
