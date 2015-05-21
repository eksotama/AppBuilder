using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using AppBuilder;
using AppBuilder.Db;
using AppBuilder.Db.DDL;

namespace Demo
{
	class Program
	{
		public sealed class OrderType
		{
			public long Id { get; private set; }
			public string Name { get; private set; }

			public OrderType(long id, string name)
			{
				if (name == null) throw new ArgumentNullException("name");

				this.Id = id;
				this.Name = name;
			}
		}

		public sealed class Product
		{
			public long Id { get; private set; }
			public string Name { get; private set; }
			public decimal Price { get; private set; }

			public Product(long id, string name, decimal price)
			{
				if (name == null) throw new ArgumentNullException("name");

				this.Id = id;
				this.Name = name;
				this.Price = price;
			}
		}

		public sealed class Order
		{
			public long Id { get; private set; }
			public DateTime OrderDate { get; private set; }
			public OrderType OrderType { get; private set; }
			public List<OrderDetail> OrderDetails { get; private set; }

			public Order(long id, DateTime orderDate, OrderType orderType, List<OrderDetail> orderDetails)
			{
				if (orderType == null) throw new ArgumentNullException("orderType");
				if (orderDetails == null) throw new ArgumentNullException("orderDetails");

				this.Id = id;
				this.OrderDate = orderDate;
				this.OrderType = orderType;
				this.OrderDetails = orderDetails;
			}
		}

		public sealed class OrderDetail
		{
			public long Id { get; private set; }
			public long Quantity { get; private set; }
			public Product Product { get; private set; }
			public Order Order { get; private set; }

			public OrderDetail(long id, long quantity, Product product, Order order)
			{
				if (product == null) throw new ArgumentNullException("product");
				if (order == null) throw new ArgumentNullException("order");

				this.Id = id;
				this.Quantity = quantity;
				this.Product = product;
				this.Order = order;
			}
		}








		public sealed class OrderAdapter
		{
			private readonly Dictionary<long, OrderType> _orderTypes;

			public OrderAdapter(Dictionary<long, OrderType> orderTypes)
			{
				if (orderTypes == null) throw new ArgumentNullException("orderTypes");

				_orderTypes = orderTypes;
			}

			public List<Order> GetAll()
			{
				var query = @"SELECT Id, OrderDate, OrderTypeId FROM Orders";
				return QueryHelper.Get(query, this.Creator);
			}

			private Order Creator(IDataReader r)
			{
				var id = 0L;
				if (!r.IsDBNull(0))
				{
					id = r.GetInt64(0);
				}
				var orderDate = DateTime.MinValue;
				if (!r.IsDBNull(1))
				{
					orderDate = r.GetDateTime(1);
				}
				var orderType = default(OrderType);
				if (!r.IsDBNull(2))
				{
					orderType = _orderTypes[r.GetInt64(2)];
				}
				var orderDetails = default(List<OrderDetail>);
				if (!r.IsDBNull(3))
				{
					orderDetails = null;
				}
				return new Order(id, orderDate, orderType, orderDetails);
			}
		}


























		static void Main(string[] args)
		{
			var dbOrderTypes = new DbTable(@"OrderTypes", new[]
														  {
															  DbColumn.PrimaryKey(@"Id"),
															  DbColumn.String(@"Name")
														  });
			var dbOrders = new DbTable(@"Orders", new[]
												  {
													  DbColumn.PrimaryKey(@"Id"),
													  DbColumn.DateTime(@"OrderDate"),
													  DbColumn.ForeignKey(@"OrderTypeId", dbOrderTypes)
												  });
			var dbProducts = new DbTable(@"Products", new[]
			                                          {
				                                          DbColumn.PrimaryKey(@"Id"),
				                                          DbColumn.String(@"Name"),
														  DbColumn.Decimal(@"Price"),
			                                          });
			var dbOrderDetails = new DbTable(@"OrderDetails", new[]
															  {
																  DbColumn.PrimaryKey(@"Id"),
																  DbColumn.Integer(@"Quantity"),
																  DbColumn.ForeignKey(@"ProductId", dbProducts),																  
																  DbColumn.ForeignKey(@"OrderId", dbOrders),
															  });

			var schema = new DbSchema(@"iFSA", new[]
			                                   {
				                                   dbOrderTypes,
				                                   dbOrders,
				                                   dbProducts,
				                                   dbOrderDetails
			                                   });
			var nameProvider = new NameProvider();

			var code = new StringBuilder(2048);
			foreach (var table in schema.Tables)
			{
				if (table.Name == dbOrders.Name)
				{
					var @class = DbTableConverter.ToClrClass(table, nameProvider, schema.Tables);
					var adapter = ClrCodeGenerator.GetAdapter(@class, nameProvider, table);
					Console.WriteLine(adapter);

					code.AppendLine(adapter);
					code.AppendLine();
					code.AppendLine();
					break;
				}
			}

			File.WriteAllText(@"C:\temp\obj.cs", code.ToString());


			Builder.Generate(schema, nameProvider, new DirectoryInfo(@"C:\temp\app\"));
			//File.WriteAllText(@"C:\temp\schema.sql", DbSchemaGenerator.Generate(schema));



			//var dbBrands = new DbTable(@"Brands", new[]
			//									  {
			//										  DbColumn.PrimaryKey(@"Id"),
			//										  DbColumn.String(@"Name"),
			//									  });
			//var dbFlavours = new DbTable(@"Flavours", new[]
			//										  {
			//											  DbColumn.PrimaryKey(@"Id"),
			//											  DbColumn.String(@"Name"),
			//										  });

			//var dbProducts = new DbTable(@"Articles", new[]
			//										  {
			//											  DbColumn.PrimaryKey(@"Id"),
			//											  DbColumn.String(@"Name"),
			//											  DbColumn.ForeignKey(@"BrandId", dbBrands),
			//											  DbColumn.ForeignKey(@"FlavourId", dbFlavours),
			//										  });

			//var code = new StringBuilder(1024);
			//var sql = new StringBuilder(1024);
			//var nameProvider = new NameProvider();

			//foreach (var ddl in tables)
			//{
			//	var schema = DbScriptGenerator.GetCreateTable(ddl);
			//	var table = DbSchemaParser.Parse(schema);
			//	sql.AppendLine(schema);
			//	sql.AppendLine();
			//	sql.AppendLine();


			//	var clrClass = DbTableConverter.ToClrClass(table, nameProvider, tables);

			//	code.AppendLine(ClrCodeGenerator.GetClass(clrClass));
			//	code.AppendLine();
			//	code.AppendLine();
			//	code.AppendLine();

			//	//code.AppendLine(ClrCodeGenerator.GetAdapterInterface(clrClass, nameProvider));
			//	//code.AppendLine();
			//	//code.AppendLine();
			//	//code.AppendLine();

			//	//code.AppendLine(ClrCodeGenerator.GetAdapter(clrClass, nameProvider, immutable, table));
			//	//code.AppendLine();
			//	//code.AppendLine();
			//	//code.AppendLine();

			//	//if (table.Columns.All(c => c.DbForeignKey != null))
			//	//{
			//	//	code.AppendLine(ClrCodeGenerator.GetHelper(clrClass, nameProvider));
			//	//	code.AppendLine();
			//	//	code.AppendLine();
			//	//	code.AppendLine();
			//	//}
			//}

			//File.WriteAllText(@"C:\temp\obj.cs", code.ToString());
			//File.WriteAllText(@"C:\temp\schema.sql", sql.ToString());
		}
	}
}