using System;
using System.IO;
using System.Linq;
using AppBuilder;
using AppBuilder.Clr;
using AppBuilder.Db;
using AppBuilder.Db.DDL;
using AppBuilder.Db.DML;

namespace Demo
{
	class Program
	{
		public sealed class Person
		{
			public long Id { get; private set; }
			public string FirstName { get; private set; }
			public string LastName { get; private set; }
			public long Age { get; private set; }
			public DateTime BirthDate { get; private set; }

			public Person(long id, string firstName, string lastName, long age, DateTime birthDate)
			{
				if (firstName == null) throw new ArgumentNullException("firstName");
				if (lastName == null) throw new ArgumentNullException("lastName");

				this.Id = id;
				this.FirstName = firstName;
				this.LastName = lastName;
				this.Age = age;
				this.BirthDate = birthDate;
			}
		}







		static void Main(string[] args)
		{
			var person = new ClrClass(@"Person", new[]
			                                     {
				                                     ClrProperty.Long(@"Id"),
													 ClrProperty.String(@"FirstName"),
													 ClrProperty.String(@"LastName"),
													 ClrProperty.Long(@"Age"),
													 ClrProperty.DateTime(@"BirthDate"),
			                                     });
			var code = ClrClassGenerator.GetCode(person);
			Console.WriteLine(code);

			File.WriteAllText(@"C:\temp\obj.cs", code);


			//var brands = new DbTable(@"Brands", new[]
			//									{
			//										DbColumn.PrimaryKey(@"Id"),
			//										DbColumn.String(@"Name"),
			//									});
			//var flavours = new DbTable(@"Flavours", new[]
			//									{
			//										DbColumn.PrimaryKey(@"Id"),
			//										DbColumn.String(@"Name"),
			//									});
			//var dbArticles = new DbTable(@"Articles", new[]
			//											  {
			//												  DbColumn.PrimaryKey(@"Id"),
			//												  DbColumn.String(@"Name"),
			//												  DbColumn.ForeignKey(@"BrandId", brands),
			//												  DbColumn.ForeignKey(@"FlavourId", flavours),
			//											  });
			//var dbOrderHeader = new DbTable(@"Headers", new[]
			//											  {
			//												  DbColumn.PrimaryKey(@"Id"),
			//												  DbColumn.String(@"Name")
			//											  });
			//var dbOrderDetails = new DbTable(@"Details", new[]
			//											  {
			//												  DbColumn.PrimaryKey(@"Id"),
			//												  DbColumn.ForeignKey(@"HeaderId", dbOrderHeader),
			//												  DbColumn.ForeignKey(@"ArticleId", dbArticles),
			//												  DbColumn.Integer(@"Quantity"),
			//											  });



			//var dbTables = new[]
			//			   {
			//				   dbArticles,
			//				   dbOrderHeader,
			//				   dbOrderDetails,
			//				   brands,
			//				   flavours,
			//			   };

			//var script = DbSchemaParser.GenerateScript(new DbSchema(@"iFSA", dbTables));
			//File.WriteAllText(@"C:\temp\schema.sql", script);

			//var schema = DbSchemaParser.Parse(script);

			//for (int i = 0; i < schema.Tables.Length; i++)
			//{
			//	var t1 = dbTables[i];
			//	var t2 = schema.Tables[i];
			//	Console.WriteLine(IsEqual(t1, t2));
			//}
			//Console.WriteLine(schema.Name);


			//var query = QueryCreator.GetSelect(dbOrderTypes, dbOrderTypes.Columns.Take(1).ToArray());
			//Display(query);
			//query = QueryCreator.GetInsert(dbOrderTypes);
			//Display(query);
			//query = QueryCreator.GetUpdate(dbOrderTypes);
			//Display(query);
			//query = QueryCreator.GetDelete(dbOrderTypes);
			//Display(query);


			//var dbOrderTypes = new DbTable(@"OrderTypes", new[]
			//											  {
			//												  DbColumn.PrimaryKey(@"Id"),
			//												  DbColumn.String(@"Name")
			//											  });
			//var dbOrders = new DbTable(@"Orders", new[]
			//									  {
			//										  DbColumn.PrimaryKey(@"Id"),
			//										  DbColumn.DateTime(@"OrderDate"),
			//										  DbColumn.ForeignKey(@"OrderTypeId", dbOrderTypes)
			//									  });
			//var dbProducts = new DbTable(@"Products", new[]
			//										  {
			//											  DbColumn.PrimaryKey(@"Id"),
			//											  DbColumn.String(@"Name"),
			//											  DbColumn.Decimal(@"Price"),
			//										  });
			//var dbOrderDetails = new DbTable(@"OrderDetails", new[]
			//												  {
			//													  DbColumn.PrimaryKey(@"Id"),
			//													  DbColumn.Integer(@"Quantity"),
			//													  DbColumn.ForeignKey(@"ProductId", dbProducts),																  
			//													  DbColumn.ForeignKey(@"OrderId", dbOrders),
			//												  });

			//var schema = new DbSchema(@"iFSA", new[]
			//								   {
			//									   dbOrderTypes,
			//									   dbOrders,
			//									   dbProducts,
			//									   dbOrderDetails
			//								   });
			//var nameProvider = new NameProvider();

			//var code = new StringBuilder(2048);
			//foreach (var table in schema.Tables)
			//{
			//	if (table.Name == dbOrders.Name)
			//	{
			//		var @class = DbTableConverter.ToClrClass(table, nameProvider, schema.Tables);
			//		var adapter = ClrCodeGenerator.GetAdapter(@class, nameProvider, table);
			//		Console.WriteLine(adapter);

			//		code.AppendLine(adapter);
			//		code.AppendLine();
			//		code.AppendLine();
			//		break;
			//	}
			//}

			//File.WriteAllText(@"C:\temp\obj.cs", code.ToString());


			//Builder.Generate(schema, nameProvider, new DirectoryInfo(@"C:\temp\app\"));
			////File.WriteAllText(@"C:\temp\schema.sql", DbSchemaGenerator.Generate(schema));
		}

		private static void Display(DbQuery query)
		{
			Console.WriteLine(query.Statement);
			Console.WriteLine(query.Parameters.Length);
			foreach (var p in query.Parameters)
			{
				Console.WriteLine(p.Name);
			}
			Console.WriteLine();
			Console.WriteLine();
		}

		public static bool IsEqual(DbTable a, DbTable b)
		{
			if (a == null) throw new ArgumentNullException("a");
			if (b == null) throw new ArgumentNullException("b");

			if (a.Name.Equals(b.Name))
			{
				var colsA = a.Columns;
				var colsB = b.Columns;
				if (colsA.Length == colsB.Length)
				{
					for (var i = 0; i < colsA.Length; i++)
					{
						var ca = colsA[i];
						var cb = colsB[i];
						if (!IsEqual(ca, cb))
						{
							return false;
						}
					}
					return true;
				}
			}

			return false;
		}

		private static bool IsEqual(DbColumn a, DbColumn b)
		{
			var x = GetColumnKey(a);
			var y = GetColumnKey(b);
			return x.Equals(y);
		}

		private static string GetColumnKey(DbColumn a)
		{
			return string.Join(@"|", new[]
			                         {
				                         a.AllowNull.ToString(),
				                         a.Name,
				                         GetForeignKeyKey(a),
				                         a.IsPrimaryKey.ToString(),
				                         GetTypeKey(a)
			                         });
		}

		private static string GetForeignKeyKey(DbColumn a)
		{
			var fk = a.DbForeignKey;
			if (fk != null)
			{
				return fk.Table + @"|" + fk.Column;
			}
			return string.Empty;
		}

		private static string GetTypeKey(DbColumn a)
		{
			return a.Type.Name;
		}
	}
}