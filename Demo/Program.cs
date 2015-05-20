using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using AppBuilder;
using AppBuilder.Db;

namespace Demo
{
	class Program
	{
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
			                                          });
			var dbOrderDetails = new DbTable(@"OrderDetails", new[]
															  {
																  DbColumn.PrimaryKey(@"Id"),
																  //DbColumn.String(@"Name"),
																  DbColumn.ForeignKey(@"ProductId", dbProducts),
																  DbColumn.Decimal(@"Price"),
																  DbColumn.ForeignKey(@"OrderId", dbOrders),
															  });




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

			var code = new StringBuilder(1024);
			var sql = new StringBuilder(1024);

			var nameProvider = new NameProvider();
			nameProvider.AddOverride(@"Categories", @"Category");
			nameProvider.AddOverride(@"Activities", @"Activity");

			var tables = new[]
			             {
							 dbOrderTypes, dbOrders, dbOrderDetails, dbProducts
							 //dbBrands, dbFlavours, dbProducts
			             };
			foreach (var ddl in tables)
			{
				var schema = DbScriptGenerator.GetCreateTable(ddl);
				var table = DbSchemaParser.Parse(schema);
				sql.AppendLine(schema);
				sql.AppendLine();
				sql.AppendLine();

				
				var immutable = true;
				var clrClass = DbTableConverter.ToClrClass(table, nameProvider, tables);

				//if (table.Name.Equals(dbOrders.Name))
				//{
				//	var newProperties = new List<ClrProperty>(clrClass.Properties);
				//	newProperties.Add(ClrProperty.Auto((new ClrType(@"List<OrderDetails>", true)), @"Details"));
				//	clrClass.Properties = newProperties.ToArray();
				//}

				code.AppendLine(ClrCodeGenerator.GetClass(clrClass, immutable));
				code.AppendLine();
				code.AppendLine();
				code.AppendLine();

				//code.AppendLine(ClrCodeGenerator.GetAdapterInterface(clrClass, nameProvider));
				//code.AppendLine();
				//code.AppendLine();
				//code.AppendLine();

				//code.AppendLine(ClrCodeGenerator.GetAdapter(clrClass, nameProvider, immutable, table));
				//code.AppendLine();
				//code.AppendLine();
				//code.AppendLine();

				//if (table.Columns.All(c => c.DbForeignKey != null))
				//{
				//	code.AppendLine(ClrCodeGenerator.GetHelper(clrClass, nameProvider));
				//	code.AppendLine();
				//	code.AppendLine();
				//	code.AppendLine();
				//}
			}

			File.WriteAllText(@"C:\temp\obj.cs", code.ToString());
			File.WriteAllText(@"C:\temp\schema.sql", sql.ToString());
		}

	}

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
		public Product Product { get; private set; }
		public decimal Price { get; private set; }
		public Order Order { get; private set; }

		public OrderDetail(long id, Product product, decimal price, Order order)
		{
			if (product == null) throw new ArgumentNullException("product");
			if (order == null) throw new ArgumentNullException("order");

			this.Id = id;
			this.Product = product;
			this.Price = price;
			this.Order = order;
		}
	}




	public sealed class Product
	{
		public long Id { get; private set; }
		public string Name { get; private set; }

		public Product(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

















}