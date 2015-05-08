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
		//public sealed class Brand
		//{
		//	public long BrandId { get; set; }
		//	public string Description { get; set; }
		//	public string LocalDescription { get; set; }

		//	public Brand()
		//	{
		//		this.BrandId = 0L;
		//		this.Description = string.Empty;
		//		this.LocalDescription = string.Empty;
		//	}
		//}

		public sealed class Brand
		{
			public long BrandId { get; private set; }
			public string Description { get; private set; }
			public string LocalDescription { get; private set; }

			public Brand(long brandId, string description, string localDescription)
			{
				if (description == null) throw new ArgumentNullException("description");
				if (localDescription == null) throw new ArgumentNullException("localDescription");

				this.BrandId = brandId;
				this.Description = description;
				this.LocalDescription = localDescription;
			}
		}

		public sealed class ChannelGroup
		{
			public long ChannelGroupId { get; set; }
			public string Description { get; set; }
			public string LocalDescription { get; set; }

			public ChannelGroup()
			{
				this.ChannelGroupId = 0L;
				this.Description = string.Empty;
				this.LocalDescription = string.Empty;
			}
		}

		public sealed class Channel
		{
			public long ChannelId { get; set; }
			public string Description { get; set; }
			public string LocalDescription { get; set; }
			public string SapChannelId { get; set; }
			public ChannelGroup ChannelGroup { get; set; }

			public Channel()
			{
				this.ChannelId = 0L;
				this.Description = string.Empty;
				this.LocalDescription = string.Empty;
				this.SapChannelId = string.Empty;
				this.ChannelGroup = default(ChannelGroup);
			}
		}












		static void Main(string[] args)
		{
			var input = @"
			CREATE TABLE [Brands] (
				[brand_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
				[description] char(100) NOT NULL, 
				[local_description] char(100) NOT NULL
			)";

			input = @"
			CREATE TABLE [ChannelGroups] (
				[channel_group_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
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

			input = File.ReadAllText(@"C:\temp\dbscript.sql");


			var buffer = new StringBuilder();

			var nameProvider = new NameProvider();
			nameProvider.AddOverride(@"Categories", @"Category");
			nameProvider.AddOverride(@"Activities", @"Activity");

			foreach (var table in DbSchemaParser.ParseTables(input))
			{
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

				//var mut = ClassGenerator.GenerateObject(obj, false);
				var immut = ClassGenerator.GenerateObject(obj, true);

				//var mut = ClassGenerator.GenerateAdapter(obj, false, AdapterResultType.Dictionary);
				var rt = AdapterResultType.Dictionary;
				if (table.Columns.Any(c => c.ForeignKey != null))
				{
					rt= AdapterResultType.List;
				}
				//var immut = ClassGenerator.GenerateAdapter(obj, true, AdapterResultType.Dictionary);

				//buffer.AppendLine(mut);
				//buffer.AppendLine();
				//buffer.AppendLine();
				buffer.AppendLine(immut);
				buffer.AppendLine();
			}

			File.WriteAllText(@"C:\temp\obj.cs", buffer.ToString());
		}
	}



}
