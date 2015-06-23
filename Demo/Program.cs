using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using AppBuilder;
using AppBuilder.Clr;
using AppBuilder.Db.DDL;
using MailReportUI;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.Style;

namespace Demo
{
	public static class QueryGenerator
	{
		private static readonly string Template = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$filter=StartDate%20eq%20datetime'{0}'%20and%20EndDate%20eq%20datetime'{1}'%20&$select=SenderAddress,RecipientAddress,Size%20&$format=json";
		private static readonly string OffsetTemplate = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$filter=StartDate%20eq%20datetime'{0}'%20and%20EndDate%20eq%20datetime'{1}'%20&$select=SenderAddress,RecipientAddress,Size%20&$skiptoken={2}&$format=json";

		public static IEnumerable<string> GenerateUrls(DateTime date)
		{
			var fromDate = date.ToString(@"s");
			var toDate = date.AddDays(1).AddSeconds(-1).ToString(@"s");

			yield return string.Format(Template, fromDate, toDate);

			var offset = 1999;

			while (true)
			{
				yield return string.Format(OffsetTemplate, fromDate, toDate, offset);
				offset += 2000;
			}
		}
	}

	public class Program
	{
		private static int _index = -1;
		private static readonly object Sync = new object();

		private static string GetEmail(string[] values)
		{
			lock (Sync)
			{
				_index++;
				if (_index >= 0 && _index < values.Length)
				{
					return values[_index];
				}
				return null;
			}
		}

		//private static void GenerateExcel(IEnumerable<ReportEntry> entries)
		//{
		//	var excelFile = new FileInfo(@"./MailReport.xlsx");
		//	if (excelFile.Exists)
		//	{
		//		excelFile.Delete();
		//	}
		//	using (var package = new ExcelPackage(excelFile))
		//	{
		//		var worksheet = package.Workbook.Worksheets.Add(@"Mail Traffic");

		//		var index = 1;
		//		foreach (var name in new[] { @"Name", @"Inbound", @"Inbound Size", @"Outbound", @"Outbound Size" })
		//		{
		//			worksheet.Cells[1, index++].Value = name;
		//		}

		//		var rowIndex = 2;
		//		foreach (var entry in entries)
		//		{
		//			var colIndex = 1;
		//			foreach (var value in entry.Values)
		//			{
		//				worksheet.Cells[rowIndex, colIndex++].Value = value;
		//			}
		//			rowIndex++;
		//		}

		//		var headerFont = worksheet.Row(1).Style.Font;
		//		headerFont.Bold = true;
		//		headerFont.Size = 12;

		//		for (var i = 1; i < index; i++)
		//		{
		//			worksheet.Column(i).AutoFit();
		//		}

		//		package.Save();
		//	}
		//}

		static void Main(string[] args)
		{
			try
			{
				//var webLogin = new WebLogin(@"O365.Reporting.SA@CCHellenic.onmicrosoft.com", @"Kafo7315");
				//var reportDate = DateTime.Today.AddDays(-1);

				//var batchCounter = new ConcurrentQueue<int>();
				//var reportData = new ConcurrentDictionary<string, ReportEntry>(8, 24 * 1024);

				//var workers = 8;
				//using (var bc = new BlockingCollection<string>(workers))
				//{
				//	using (var completedEvent = new CountdownEvent(workers))
				//	{
				//		for (var i = 0; i < workers; i++)
				//		{
				//			ThreadPool.QueueUserWorkItem(_ =>
				//			{
				//				var parameters = _ as object[];
				//				var e = parameters[0] as CountdownEvent;
				//				var urls = parameters[1] as BlockingCollection<string>;
				//				var login = parameters[2] as WebLogin;
				//				var lookup = parameters[3] as ConcurrentDictionary<string, ReportEntry>;
				//				var counter = parameters[4] as ConcurrentQueue<int>;

				//				try
				//				{
				//					var buffer = new byte[16 * 1024];
				//					foreach (var url in urls.GetConsumingEnumerable())
				//					{
				//						foreach (var entry in ReportGenerator.Parse(WebQuery.DownloadContents(login, url, buffer)).Item1)
				//						{
				//							ReportEntry reportEntry;

				//							var name = entry.Sender.ToLowerInvariant();
				//							if (name.EndsWith(@"@cchellenic.com"))
				//							{
				//								if (!lookup.TryGetValue(name, out reportEntry))
				//								{
				//									reportEntry = new ReportEntry(name);
				//									lookup.TryAdd(name, reportEntry);

				//									// Query one more time to get the unique entry
				//									lookup.TryGetValue(name, out reportEntry);
				//								}
				//								Interlocked.Increment(ref reportEntry.Outbound);
				//								Interlocked.Add(ref reportEntry.OutboundSize, entry.Size);
				//							}
				//							name = entry.Recipient.ToLowerInvariant();
				//							if (name.EndsWith(@"@cchellenic.com"))
				//							{
				//								if (!lookup.TryGetValue(name, out reportEntry))
				//								{
				//									reportEntry = new ReportEntry(name);
				//									lookup.TryAdd(name, reportEntry);

				//									// Query one more time to get the unique entry
				//									lookup.TryGetValue(name, out reportEntry);
				//								}
				//								Interlocked.Increment(ref reportEntry.Inbound);
				//								Interlocked.Add(ref reportEntry.InboundSize, entry.Size);
				//							}
				//						}

				//						counter.Enqueue(0);
				//						Console.WriteLine(counter.Count);

				//						lock (lookup)
				//						{
				//							GenerateExcel(lookup.Values);
				//						}
				//					}
				//				}
				//				catch (Exception ex)
				//				{
				//					// TODO : Log exception
				//					Debug.WriteLine(ex);
				//				}
				//				finally
				//				{
				//					e.Signal();
				//					urls.CompleteAdding();
				//				}
				//			}, new object[] { completedEvent, bc, webLogin, reportData, batchCounter });
				//		}
				//		try
				//		{
				//			foreach (var url in QueryGenerator.GenerateUrls(reportDate))
				//			{
				//				// Check if all workers completed
				//				if (completedEvent.IsSet)
				//				{
				//					break;
				//				}
				//				bc.Add(url);
				//			}
				//		}
				//		catch (Exception ex)
				//		{
				//			// TODO : Log excetpion
				//			Debug.WriteLine(ex);
				//		}
				//		finally
				//		{
				//			bc.CompleteAdding();
				//		}
				//		completedEvent.Wait();
				//	}
				//}

				//GenerateExcel(reportData.Values);

				//Console.WriteLine(@"Done");




				//var filename = @"C:\temp\cchbcMails.txt";

				//var hs = new HashSet<string>();
				//using (var sw = new StreamWriter(filename))
				//{
				//	using (var fs = File.OpenRead(@"C:\temp\emails.txt"))
				//	{
				//		using (var sr = new StreamReader(fs))
				//		{
				//			string line;
				//			while ((line = sr.ReadLine()) != null)
				//			{
				//				line = line.Trim();
				//				if (line.IndexOf(@"cchellenic", StringComparison.OrdinalIgnoreCase) >= 0)
				//				{
				//					//sw.WriteLine(line);
				//					hs.Add(line);
				//				}
				//			}
				//		}
				//	}
				//	var copy = hs.ToArray();
				//	Array.Sort(copy);
				//	foreach (var value in copy)
				//	{
				//		sw.WriteLine(value);
				//	}
				//}

				//var f = new FileInfo(@"C:\temp\input.json");

				//var contents = File.ReadAllText(f.FullName);

				//var s = Stopwatch.StartNew();
				//var entries = ReportGenerator.Parse(contents);
				//s.Stop();
				//Console.WriteLine(s.ElapsedMilliseconds);
				//Console.WriteLine(entries.Count);
				//foreach (var entry in entries)
				//{
				//	Console.WriteLine(entry);
				//}

				//foreach (var range in WebQuery.GetDayRanges())
				//{

				//}


				//var reportEntries = new Dictionary<string, ReportEntry>(1024);

				//var buffer = new byte[4 * 1024];
				//var login = new WebLogin(@"O365.Reporting.SA@CCHellenic.onmicrosoft.com", @"Kafo7315");
				//var startTime = DateTime.Today;
				//var limitTime = startTime.AddDays(1);
				//while (startTime < limitTime)
				//{
				//	Console.WriteLine(startTime);
				//	var endTime = startTime.AddMinutes(1);
				//	var url = WebQuery.GetQueryUrl(startTime, endTime);
				//	var contents = WebQuery.DownloadContents(login, url, buffer);
				//	var entries = ReportGenerator.Parse(contents);

				//	foreach (var entry in entries)
				//	{
				//		ReportEntry reportEntry;

				//		var name = entry.Sender;
				//		if (name.IndexOf(@"cchellenic.com", StringComparison.Ordinal) >= 0)
				//		{
				//			if (!reportEntries.TryGetValue(name, out reportEntry))
				//			{
				//				reportEntry = new ReportEntry(name);
				//				reportEntries.Add(name, reportEntry);
				//			}
				//			reportEntry.Outbound++;
				//			reportEntry.OutboundSize += entry.Size;
				//		}
				//		name = entry.Recipient;
				//		if (name.IndexOf(@"cchellenic.com", StringComparison.Ordinal) >= 0)
				//		{
				//			if (!reportEntries.TryGetValue(name, out reportEntry))
				//			{
				//				reportEntry = new ReportEntry(name);
				//				reportEntries.Add(name, reportEntry);
				//			}
				//			reportEntry.Inbound++;
				//			reportEntry.InboundSize += entry.Size;
				//		}
				//	}

				//	Console.WriteLine(entries.Count);
				//	File.WriteAllText(f.FullName, contents);
				//	startTime = endTime;
				//}

				//var emails = File.ReadAllLines(filename).Skip(2317).Take(100).ToArray();

				//emails = new[]
				//		 {
				//			 @"ivan.andonov@cchellenic.com",
				//			 @"Petar.P.Petrov@cchellenic.com"
				//		 };

				//var results = new ConcurrentQueue<ReportEntry>();

				//using (var ce = new CountdownEvent(Math.Min(32, emails.Length)))
				//{
				//	for (var i = 0; i < ce.InitialCount; i++)
				//	{
				//		ThreadPool.QueueUserWorkItem(_ =>
				//									 {
				//										 var e = _ as CountdownEvent;
				//										 try
				//										 {
				//											 while (true)
				//											 {
				//												 var name = GetEmail(emails);
				//												 if (name == null)
				//												 {
				//													 break;
				//												 }
				//												 try
				//												 {
				//													 var buffer = new byte[4 * 1024];
				//													 var inbound = ReportGenerator.ParseJson(WebQuery.DownloadContents(login, WebQuery.GetMessageTraceRecipientQuery(date, name), buffer));
				//													 var outbound = ReportGenerator.ParseJson(WebQuery.DownloadContents(login, WebQuery.GetMessageTraceSenderQuery(date, name), buffer));

				//													 results.Enqueue(new ReportEntry(name, inbound.Item1, inbound.Item2, outbound.Item1, outbound.Item2));
				//												 }
				//												 catch (Exception ex)
				//												 {
				//													 Console.WriteLine(string.Format(@"Error with: {0} {1}", name, ex.Message));
				//												 }
				//											 }
				//										 }
				//										 catch (Exception ex)
				//										 {
				//											 Console.WriteLine(ex);
				//										 }
				//										 finally
				//										 {
				//											 e.Signal();
				//										 }
				//									 }, ce);
				//	}
				//	ce.Wait();
				//}

				//// Parse the user entries
				//var mailUserEntries = ReportGenerator.ParseMailUserEntries(document);

				//// Create report entries
				//var reportEntries = ReportGenerator.CreateReportEntries(mailUserEntries);

				//foreach (var reportEntry in reportEntries)
				//{
				//	var detailDataXml = WebQuery.DownloadContentsAsync(login, WebQuery.GetMessageTraceSenderQuery(DateTime.Today, reportEntry.Name)).Result;

				//	Console.WriteLine(reportEntry.Name);
				//	Console.WriteLine(reportEntry.Inbound);
				//	Console.WriteLine(reportEntry.Outbound);
				//	Console.WriteLine();
				//}

				//Console.WriteLine(mailUserEntries.Count);
				//Console.WriteLine(mailUserEntries.Select(e => e.Name.ToLowerInvariant()).Distinct().Count());

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