using System;
using System.Linq;
using AppBuilder.Db;
using AppBuilder.Db.DDL;
using AppBuilder.Db.DML;

namespace Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			var dbProducts = new DbTable(@"Products", new[]
														  {
															  DbColumn.PrimaryKey(@"Id"),
															  DbColumn.String(@"Name"),
														  });
			var dbOrderHeader = new DbTable(@"Headers", new[]
														  {
															  DbColumn.PrimaryKey(@"Id"),
															  DbColumn.String(@"Name")
														  });
			var dbOrderDetails = new DbTable(@"Details", new[]
														  {
															  DbColumn.PrimaryKey(@"Id"),
															  DbColumn.ForeignKey(@"Product", dbProducts),
															  DbColumn.Integer(@"Quantity"),
															  DbColumn.ForeignKey(@"HeaderId", dbOrderHeader)
														  });

			var query = QueryCreator.GetSelect(dbOrderHeader, dbOrderDetails);
			Display(query);

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
	}
}