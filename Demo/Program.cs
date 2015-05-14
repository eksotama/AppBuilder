using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using AppBuilder;
using AppBuilder.Clr;
using AppBuilder.Db;

namespace Demo
{
	class Program
	{
		public sealed class ChannelGroup
		{
			public long ChannelGroupId { get; private set; }
			public string Description { get; private set; }
			public string LocalDescription { get; private set; }

			public ChannelGroup(long channelGroupId, string description, string localDescription)
			{
				if (description == null) throw new ArgumentNullException("description");
				if (localDescription == null) throw new ArgumentNullException("localDescription");

				this.ChannelGroupId = channelGroupId;
				this.Description = description;
				this.LocalDescription = localDescription;
			}
		}

		public interface IChannelGroupAdapter
		{
			void Fill(Dictionary<long, ChannelGroup> channelGroups);
		}

		public sealed class ChannelGroupAdapter : IChannelGroupAdapter
		{
			public void Fill(Dictionary<long, ChannelGroup> items)
			{
				if (items == null) throw new ArgumentNullException("items");

				var query = "SELECT channel_group_id, description, local_description FROM ChannelGroups";
				QueryHelper.Fill(items, query, this.Creator, this.Selector);
			}

			private ChannelGroup Creator(IDataReader r)
			{
				var channelGroupId = 0L;
				if (!r.IsDBNull(0))
				{
					channelGroupId = r.GetInt64(0);
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
				return new ChannelGroup(channelGroupId, description, localDescription);
			}

			private long Selector(ChannelGroup c) { return c.ChannelGroupId; }
		}

		public sealed class ChannelGroupHelper
		{
			private readonly Dictionary<long, ChannelGroup> _channelGroups = new Dictionary<long, ChannelGroup>();

			public Dictionary<long, ChannelGroup> ChannelGroups { get { return _channelGroups; } }

			public void Load(ChannelGroupAdapter adapter)
			{
				if (adapter == null) throw new ArgumentNullException("adapter");

				adapter.Fill(_channelGroups);
			}
		}

		public sealed class Course
		{

		}

		public sealed class Person
		{
			public long Id { get; private set; }

			public Person(long id, string name, long age, Course course)
			{
				throw new NotImplementedException();
			}
		}

		public interface IPersonAdapter
		{
			void Fill(Dictionary<long, Person> persons);
		}

		public sealed class PersonAdapter : IPersonAdapter
		{
			private readonly Dictionary<long, Course> _courses;

			public PersonAdapter(Dictionary<long, Course> courses)
			{
				if (courses == null) throw new ArgumentNullException("courses");

				_courses = courses;
			}

			public void Fill(Dictionary<long, Person> items)
			{
				if (items == null) throw new ArgumentNullException("items");

				var query = "SELECT * FROM ....";
				QueryHelper.Fill(items, query, this.Creator, this.Selector);
			}

			private Person Creator(IDataReader r)
			{
				var id = 0L;
				if (!r.IsDBNull(0))
				{
					id = r.GetInt64(0);
				}
				var name = string.Empty;
				if (!r.IsDBNull(1))
				{
					name = r.GetString(1);
				}
				var age = 0L;
				if (!r.IsDBNull(2))
				{
					age = r.GetInt64(2);
				}
				var course = default(Course);
				if (!r.IsDBNull(3))
				{
					course = _courses[r.GetInt64(3)];
				}
				return new Person(id, name, age, course);
			}

			private long Selector(Person p) { return p.Id; }
		}


		static void Main(string[] args)
		{
			var cl = new ClrClass(@"Person", new[]
			                                 {
				                                 ClrProperty.Auto(ClrType.Long, @"id"),
				                                 ClrProperty.Auto(ClrType.String, @"name"),
				                                 ClrProperty.Auto(ClrType.Long, @"age"),
				                                 ClrProperty.Auto(new ClrType(@"Course"), @"course"),
			                                 });

			var v = CodeGenerator.GetClass(cl, true);
			v = CodeGenerator.GetAdapter(cl, new NameProvider(), true, @"SELECT * FROM ....");
			v = CodeGenerator.GetAdapterInterface(cl, new NameProvider());
			Console.WriteLine(v);
			return;














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

			//			input = @"
			//			CREATE TABLE [Channels] (
			//				[channel_id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
			//				[description] char(100) NOT NULL, 
			//				[local_description] char(100) NOT NULL, 
			//				[sap_channel_id] char(10) NOT NULL,
			//				[channel_group_id] integer,  
			//				FOREIGN KEY ([channel_group_id])
			//					REFERENCES [ChannelGroups] ([channel_group_id])
			//					ON UPDATE NO ACTION ON DELETE NO ACTION
			//			)";

			//input = File.ReadAllText(@"C:\temp\script.sql");


			var buffer = new StringBuilder();

			var nameProvider = new NameProvider();
			nameProvider.AddOverride(@"Categories", @"Category");
			nameProvider.AddOverride(@"Activities", @"Activity");

			foreach (var table in DbSchemaParser.ParseTables(input))
			{
				var definition = DbTableConverter.ToClassDefinition(table, nameProvider);
				Console.WriteLine(CodeGenerator.GetClass(definition, true));
				return;
			}



			File.WriteAllText(@"C:\temp\obj.cs", buffer.ToString());


			try
			{
				using (var cn = new SQLiteConnection(@"Data Source=C:\Users\bg900343\Desktop\local.sqlite;"))
				{
					cn.Open();
					QueryHelper.Connection = cn;
					var h = new ChannelGroupHelper();

					var adapter = new ChannelGroupAdapter();
					var s = Stopwatch.StartNew();
					for (var i = 0; i < 100; i++)
					{
						h.Load(adapter);
					}
					s.Stop();
					Console.WriteLine(s.ElapsedMilliseconds);

					foreach (var kv in h.ChannelGroups)
					{
						var cg = kv.Value;
						Console.WriteLine(cg.ChannelGroupId);
						Console.WriteLine(cg.Description);
						Console.WriteLine();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

		}
	}


}
