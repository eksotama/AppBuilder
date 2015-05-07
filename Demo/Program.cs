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
			public long ChannelId { get; private set; }
			public string Description { get; private set; }
			public string LocalDescription { get; private set; }
			public string SapChannelId { get; private set; }
			public ChannelGroup ChannelGroup { get; private set; }

			public Channel(long channelId, string description, string localDescription, string sapChannelId, ChannelGroup channelGroup)
			{
				if (description == null) throw new ArgumentNullException("description");
				if (localDescription == null) throw new ArgumentNullException("localDescription");
				if (sapChannelId == null) throw new ArgumentNullException("sapChannelId");
				if (channelGroup == null) throw new ArgumentNullException("channelGroup");

				this.ChannelId = channelId;
				this.Description = description;
				this.LocalDescription = localDescription;
				this.SapChannelId = sapChannelId;
				this.ChannelGroup = channelGroup;
			}
		}


		public sealed class ChannelAdapter : AdapterBase
		{
			private readonly Dictionary<long, ChannelGroup> _channelGroups;

			public ChannelAdapter(QueryHelper queryHelper, Dictionary<long, ChannelGroup> channelGroups)
				: base(queryHelper)
			{
				if (channelGroups == null) throw new ArgumentNullException("channelGroups");
				_channelGroups = channelGroups;
			}

			public void Fill(List<Channel> items)
			{
				if (items == null) throw new ArgumentNullException("items");

				var query = "SELECT channel_id, description, local_description, sap_channel_id, channel_group_id FROM Channels";
				this.QueryHelper.Fill(items, query, this.Creator);
			}

			private Channel Creator(IDataReader r)
			{
				var channelId = 0L;
				if (!r.IsDBNull(0))
				{
					channelId = r.GetInt64(0);
				}
				var description = string.Empty;
				if (!r.IsDBNull(1))
				{
					description = r.GetString(1);
				}
				var localDescription = string.Empty;
				if (!r.IsDBNull(2))
				{
					localDescription = r.GetString(2);
				}
				var sapChannelId = string.Empty;
				if (!r.IsDBNull(3))
				{
					sapChannelId = r.GetString(3);
				}
				var channelGroup = default(ChannelGroup);
				if (!r.IsDBNull(4))
				{
					channelGroup = _channelGroups[r.GetInt64(4)];
				}
				return new Channel(channelId, description, localDescription, sapChannelId, channelGroup);
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

				var mut = AdapterClassGenerator.Generate(obj, false, AdapterResultType.List);
				var immut = AdapterClassGenerator.Generate(obj, true, AdapterResultType.List);

				var total = mut + Environment.NewLine + Environment.NewLine + Environment.NewLine + immut;
				File.WriteAllText(@"C:\temp\obj.cs", total);
			}
		}
	}



}
