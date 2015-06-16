using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using AppBuilder;
using AppBuilder.Clr;
using AppBuilder.Db.DDL;
using MailReportUI;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Demo
{
	public class Program
	{
		static void Main(string[] args)
		{

			try
			{
				var login = new WebLogin(@"O365.Reporting.SA@CCHellenic.onmicrosoft.com", @"Kafo7315");
				var masterUrl = WebQuery.GetMailTrafficQuery(DateTime.Today);

				XDocument document;
				var bufferFile = new FileInfo(@"C:\temp\raw.xml");
				if (bufferFile.Exists)
				{
					// Load from the disk
					document = XDocument.Load(bufferFile.FullName);
				}
				else
				{
					// Download Master Data XML
					var masterDataXml = WebQuery.DownloadContentsAsync(login, masterUrl).Result;

					// Load into memory				
					document = XDocument.Parse(masterDataXml);

					// Save to buffer file
					document.Save(bufferFile.FullName);
				}

				// Parse the user entries
				var mailUserEntries = ReportGenerator.ParseMailUserEntries(document);

				// Create report entries
				var reportEntries = ReportGenerator.CreateReportEntries(mailUserEntries);

				foreach (var reportEntry in reportEntries)
				{
					var detailDataXml = WebQuery.DownloadContentsAsync(login, WebQuery.GetMessageTraceSenderQuery(DateTime.Today, reportEntry.Name)).Result;

					Console.WriteLine(reportEntry.Name);
					Console.WriteLine(reportEntry.Inbound);
					Console.WriteLine(reportEntry.Outbound);
					Console.WriteLine();
				}

				Console.WriteLine(mailUserEntries.Count);
				Console.WriteLine(mailUserEntries.Select(e => e.Name.ToLowerInvariant()).Distinct().Count());

				//foreach (var entry in entries)
				//{
				//	//Console.WriteLine(entry.Recipient);
				//	Console.WriteLine(entry.Name);
				//	Console.WriteLine(entry.Direction);
				//	Console.WriteLine(entry.MessageCount);
				//	Console.WriteLine();
				//}


				//var excelFile = new FileInfo(@"C:\temp\MailReportOutput.xlsx");
				//if (excelFile.Exists)
				//{
				//	excelFile.Delete();
				//}

				//using (var package = new ExcelPackage(excelFile))
				//{
				//	var worksheet = package.Workbook.Worksheets.Add(@"Mail Traffic");

				//	var index = 1;
				//	foreach (var name in new[] { @"Name", @"Inbound", @"Inbound Size", @"Outbound", @"Outbound Size" })
				//	{
				//		worksheet.Cells[1, index++].Value = name;
				//	}

				//	var rowIndex = 2;
				//	foreach (var entry in entries)
				//	{
				//		var colIndex = 1;
				//		foreach (var value in entry.Values)
				//		{
				//			worksheet.Cells[rowIndex, colIndex++].Value = value;
				//		}
				//		rowIndex++;
				//	}

				//	var headerFont = worksheet.Row(1).Style.Font;
				//	headerFont.Bold = true;
				//	headerFont.Size = 12;

				//	for (var i = 1; i < index; i++)
				//	{
				//		worksheet.Column(i).BestFit = true;
				//		worksheet.Column(i).AutoFit();
				//	}

				//	package.Save();
				//}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}


			//using (var cn = new SQLiteConnection(@"Data Source=C:\Users\bg900343\Desktop\store.sqlite"))
			//{
			//	cn.Open();
			//	QueryHelper.Connection = cn;
			//	QueryHelper.ParameterCreator = (name, v) => new SQLiteParameter(name, v);

			//	var bds = QueryHelper.Get(@"select id, name from brands", r => r.GetInt64(0) + " : " + r.GetString(1));
			//	foreach (var bd in bds)
			//	{
			//		Console.WriteLine(bd);
			//	}



			//	QueryHelper.ExecuteQuery(@"insert into brands(name) values('A')");
			//	var value = QueryHelper.ExecuteScalar(@"SELECT LAST_INSERT_ROWID()");
			//	Console.WriteLine(value);

			//}
			//return;


			//var builder = new Builder(new DirectoryInfo(@"C:\temp\ifsa"));
			//builder.BuilderApp(new IFsa().Create());




			//var tables = new GitHub().Create().Tables;
			//tables = new IFsa().Create().Tables;

			//DumpDDL(tables);
			//DumpClasses(tables);
			//DumpAdapters(tables);
			//DumpHelpers(tables);
			//DumApp(tables);
		}




		private static void DumpDDL(DbTable[] tables)
		{
			var script = DbSchemaParser.GenerateScript(new DbSchema(@"iFSA", tables));
			File.WriteAllText(@"C:\temp\schema.sql", script);

			var schema = DbSchemaParser.Parse(script);
			for (int index = 0; index < schema.Tables.Length; index++)
			{
				var a = schema.Tables[index];
				var b = tables[index];
				Console.WriteLine(IsEqual(a, b));
			}
		}

		private static void DumpClasses(DbTable[] tables)
		{
			var buffer = new StringBuilder();
			foreach (var table in tables)
			{
				var code = ObjectGenerator.GenerateCode(DbTableConverter.ToClrClass(table, tables), table.IsReadOnly);
				buffer.AppendLine(code);
			}
			File.WriteAllText(@"C:\temp\obj.cs", buffer.ToString());
		}

		private static void DumpAdapters(DbTable[] tables)
		{
			var schema = new DbSchema(string.Empty, tables);
			var buffer = new StringBuilder();
			foreach (var table in tables)
			{
				//if ( table.Name != @"Brands")
				//if (table.Name != @"Articles")
				//if (table.IsReadOnly)
				//if (table.Name != @"Activities")
				//if (table.Name != @"CalendarDays")
				if (false)
				//if (table.Name != @"Visits")
				//if (table.Name == @"Visits")
				{
					continue;
				}
				var code = AdapterGenerator.GenerateCode(DbTableConverter.ToClrClass(table, tables), table, schema) + Environment.NewLine;
				buffer.AppendLine(code);
			}
			File.WriteAllText(@"C:\temp\ada.cs", buffer.ToString());
		}

		private static void DumpHelpers(DbTable[] tables)
		{
			var buffer = new StringBuilder();
			foreach (var table in tables)
			{
				if (table.IsReadOnly)
				{
					var code = HelperGenerator.GenerateCode(table) + Environment.NewLine;
					buffer.AppendLine(code);
				}
			}
			File.WriteAllText(@"C:\temp\help.cs", buffer.ToString());
		}

		private static void DumApp(DbTable[] tables)
		{
			var code = AppGenerator.GenerateCode(new DbSchema(string.Empty, tables)) + Environment.NewLine;
			File.WriteAllText(@"C:\temp\app.cs", code);
		}

		//private static void Display(DbQuery query)
		//{
		//	Console.WriteLine(query.Statement);
		//	Console.WriteLine(query.Parameters.Length);
		//	foreach (var p in query.Parameters)
		//	{
		//		Console.WriteLine(p.Name);
		//	}
		//	Console.WriteLine();
		//	Console.WriteLine();
		//}

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

	public sealed class HelperLoadedEventArgs : EventArgs
	{
		public string Name { get; private set; }

		public HelperLoadedEventArgs(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
		}
	}

	//public sealed class App
	//{
	//	public event EventHandler<HelperLoadedEventArgs> HelperLoaded;

	//	private void OnHelperLoaded(HelperLoadedEventArgs e)
	//	{
	//		var handler = HelperLoaded;
	//		if (handler != null) handler(this, e);
	//	}

	//	private readonly ActivityTypesHelper _activityTypesHelper = new ActivityTypesHelper();
	//	public ActivityTypesHelper ActivityTypesHelper
	//	{
	//		get { return _activityTypesHelper; }
	//	}

	//	private readonly ArticleTypesHelper _articleTypesHelper = new ArticleTypesHelper();
	//	public ArticleTypesHelper ArticleTypesHelper
	//	{
	//		get { return _articleTypesHelper; }
	//	}

	//	private readonly BrandsHelper _brandsHelper = new BrandsHelper();
	//	public BrandsHelper BrandsHelper
	//	{
	//		get { return _brandsHelper; }
	//	}

	//	private readonly DeliveryLocationsHelper _deliveryLocationsHelper = new DeliveryLocationsHelper();
	//	public DeliveryLocationsHelper DeliveryLocationsHelper
	//	{
	//		get { return _deliveryLocationsHelper; }
	//	}

	//	private readonly FlavoursHelper _flavoursHelper = new FlavoursHelper();
	//	public FlavoursHelper FlavoursHelper
	//	{
	//		get { return _flavoursHelper; }
	//	}

	//	private readonly UsersHelper _usersHelper = new UsersHelper();
	//	public UsersHelper UsersHelper
	//	{
	//		get { return _usersHelper; }
	//	}

	//	private readonly ArticlesHelper _articlesHelper = new ArticlesHelper();
	//	public ArticlesHelper ArticlesHelper
	//	{
	//		get { return _articlesHelper; }
	//	}

	//	private readonly OutletsHelper _outletsHelper = new OutletsHelper();
	//	public OutletsHelper OutletsHelper
	//	{
	//		get { return _outletsHelper; }
	//	}

	//	public void Load()
	//	{
	//		this.ActivityTypesHelper.Load(new ActivityTypesAdapter());
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"ActivityTypes"));

	//		this.ArticleTypesHelper.Load(new ArticleTypesAdapter());
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"ArticleTypes"));

	//		this.BrandsHelper.Load(new BrandsAdapter());
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"Brands"));

	//		this.DeliveryLocationsHelper.Load(new DeliveryLocationsAdapter());
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"DeliveryLocations"));

	//		this.FlavoursHelper.Load(new FlavoursAdapter());
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"Flavours"));

	//		this.UsersHelper.Load(new UsersAdapter());
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"Users"));

	//		this.ArticlesHelper.Load(new ArticlesAdapter(this.ArticleTypesHelper.ArticleTypes, this.BrandsHelper.Brands, this.FlavoursHelper.Flavours));
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"Articles"));

	//		this.OutletsHelper.Load(new OutletsAdapter(this.DeliveryLocationsHelper.DeliveryLocations));
	//		this.OnHelperLoaded(new HelperLoadedEventArgs(@"Outlets"));

	//	}
	//}
}