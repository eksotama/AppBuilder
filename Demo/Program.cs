using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using AppBuilder;
using AppBuilder.Db;

namespace Demo
{
	class Program
	{
	

		static void Main(string[] args)
		{
			var dbBrands = new DbTable(@"Brands", new[]
			                                       {
				                                       new DbColumn(DbColumnType.Integer, @"Id", isPrimaryKey:true),
				                                       new DbColumn(DbColumnType.String, @"Name"),
			                                       });
			var dbFlavours = new DbTable(@"Flavours", new[]
			                                       {
				                                       new DbColumn(DbColumnType.Integer, @"Id", isPrimaryKey:true),
				                                       new DbColumn(DbColumnType.String, @"Name"),
			                                       });

			var dbProducts = new DbTable(@"Articles", new[]
			                                          {
				                                          new DbColumn(DbColumnType.Integer, @"Id", isPrimaryKey:true), 
														  new DbColumn(DbColumnType.String, @"Name"), 
														  new DbColumn(DbColumnType.Integer, @"BrandId") {ForeignKey = new DbForeignKey(@"Brands", @"Id")}, 
														  new DbColumn(DbColumnType.Integer, @"FlavourId") {ForeignKey = new DbForeignKey(@"Flavours", @"Id")}, 
			                                          });

			var code = new StringBuilder(1024);
			var sql = new StringBuilder(1024);

			var nameProvider = new NameProvider();
			nameProvider.AddOverride(@"Categories", @"Category");
			nameProvider.AddOverride(@"Activities", @"Activity");

			foreach (var ddl in new[]
			                    {
				                    dbBrands, dbFlavours, dbProducts
			                    }.Select(DbScriptGenerator.GetCreateTable))
			{
				var table = DbSchemaParser.Parse(ddl);
				sql.AppendLine(ddl);
				sql.AppendLine();
				sql.AppendLine();

				var immutable = true;
				var clrClass = DbTableConverter.ToClrClass(table, nameProvider);

				code.AppendLine(ClrCodeGenerator.GetClass(clrClass, immutable));
				code.AppendLine();
				code.AppendLine();
				code.AppendLine();

				code.AppendLine(ClrCodeGenerator.GetAdapterInterface(clrClass, nameProvider));
				code.AppendLine();
				code.AppendLine();
				code.AppendLine();

				code.AppendLine(ClrCodeGenerator.GetAdapter(clrClass, nameProvider, immutable, table));
				code.AppendLine();
				code.AppendLine();
				code.AppendLine();

				if (table.Columns.All(c => c.ForeignKey != null))
				{
					code.AppendLine(ClrCodeGenerator.GetHelper(clrClass, nameProvider));
					code.AppendLine();
					code.AppendLine();
					code.AppendLine();
				}
			}

			File.WriteAllText(@"C:\temp\obj.cs", code.ToString());
			File.WriteAllText(@"C:\temp\schema.sql", sql.ToString());

		}
	}
}