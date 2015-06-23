using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core;
using OfficeOpenXml;

namespace MailReportUI
{
	public sealed class ReportGeneratorViewModel : BindableObject
	{
		private static int _index = -1;
		private static readonly object Sync = new object();
		private static readonly object ProgressSync = new object();

		//private readonly List<ReportEntry> _results = new List<ReportEntry>();
		//private readonly Dictionary<string, ReportEntry> _reportEntries = new Dictionary<string, ReportEntry>(1024);
		private readonly Timer _timer;
		private DateTime _startTime;

		private int _processed;

		private static string GetBatch(string[] values)
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

		//private static DateRange GetBatch(List<DateRange> values)
		//{
		//	lock (Sync)
		//	{
		//		_index++;
		//		if (_index >= 0 && _index < values.Count)
		//		{
		//			return values[_index];
		//		}
		//		return null;
		//	}
		//}

		public ReportSettings Settings { get; private set; }

		private bool _isIdle = true;
		public bool IsIdle
		{
			get { return _isIdle; }
			set
			{
				this.SetField(ref _isIdle, value);
				if (value)
				{
					this.IsBusy = Visibility.Collapsed;
				}
				else
				{
					this.IsBusy = Visibility.Visible;
				}
			}
		}

		private double _minValue = -1;
		public double MinValue
		{
			get { return _minValue; }
			set { this.SetField(ref _minValue, value); }
		}

		private double _maxValue = -1;
		public double MaxValue
		{
			get { return _maxValue; }
			set { this.SetField(ref _maxValue, value); }
		}

		private double _currentValue = -1;
		public double CurrentValue
		{
			get { return _currentValue; }
			set
			{
				this.SetField(ref _currentValue, value);
				this.CurrentValuePercent = string.Format(@"{0} %", (this.CurrentValue / (this.MaxValue / 100)).ToString(@"F2"));
			}
		}

		private string _currentValuePercent = string.Empty;
		public string CurrentValuePercent
		{
			get { return _currentValuePercent; }
			set { this.SetField(ref _currentValuePercent, value); }
		}

		private string _progressStep = string.Empty;
		public string ProgressStep
		{
			get { return _progressStep; }
			set { this.SetField(ref _progressStep, value); }
		}

		private string _elapsedTime = string.Empty;
		public string ElapsedTime
		{
			get { return _elapsedTime; }
			set { this.SetField(ref _elapsedTime, value); }
		}

		private Visibility _isBusy = Visibility.Collapsed;
		public Visibility IsBusy
		{
			get { return _isBusy; }
			set { this.SetField(ref _isBusy, value); }
		}

		public DelegateCommand<ReportSettings> GenerateReportCommand { get; private set; }

		public ReportGeneratorViewModel()
		{
			this.Settings = new ReportSettings();
			this.GenerateReportCommand = new DelegateCommand<ReportSettings>(this.GenerateReport, this.CanGenerateReport);
			this.Settings.PropertyChanged += (sender, args) => this.GenerateReportCommand.RaiseCanExecuteChanged();

			_timer = new Timer(UpdateTime);
		}

		private void UpdateTime(object state)
		{
			var elapsed = (DateTime.Now - _startTime);

			var a = elapsed.TotalMilliseconds;
			var ai = _processed;
			if (ai == 0)
			{
				ai = 1;
			}
			var bi = (int)this.MaxValue;
			var eta = (a * bi) / ai;

			this.ElapsedTime = @"Elapsed:" + elapsed.ToString(@"hh\:mm\:ss") + @", ETA:" + TimeSpan.FromMilliseconds(eta).ToString(@"hh\:mm\:ss");
		}

		public void PasswordChanged(string password)
		{
			// Set the password. Settings will notify for Property changed.
			this.Settings.Password = password;
		}

		private bool CanGenerateReport(ReportSettings arg)
		{
			if (arg == null) throw new ArgumentNullException("arg");

			return !string.IsNullOrWhiteSpace(arg.Username) &&
				   !string.IsNullOrWhiteSpace(arg.Password);
		}

		private Task GenerateReport(ReportSettings settings)
		{
			try
			{
				this.IsIdle = false;
				this.MinValue = 0;
				this.CurrentValue = 0;
				this.ProgressStep = @"Collecting data ...";

				var emails = new[]
							 {
								 @"ivan.andonov@cchellenic.com",
							 };
				ProcessMails(settings, emails);

				//ProcessMails(settings, WebQuery.GetDayRanges(settings.Date, settings.Date.AddDays(1)));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			return Task.FromResult(true);
		}

		//private void ProcessMails(ReportSettings settings, List<DateRange> ranges)
		//{
		//	this.MaxValue = ranges.Count;
		//	_startTime = DateTime.Now;
		//	_timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));

		//	ThreadPool.QueueUserWorkItem(_ =>
		//	{
		//		using (var ce = new CountdownEvent(Math.Min(8, ranges.Count)))
		//		{
		//			for (var i = 0; i < ce.InitialCount; i++)
		//			{
		//				ThreadPool.QueueUserWorkItem(ProcessBatchRanges, new object[] { ce, settings, ranges });
		//			}
		//			ce.Wait();
		//		}

		//		_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

		//		this.CurrentValue = 0;
		//		this.ProgressStep = @"Generating Excel file ...";

		//		// Sort
		//		_results.Clear();
		//		_results.AddRange(_reportEntries.Values);
		//		_results.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

		//		//GenerateExcel(_results);
		//	});
		//}

		private void ProcessMails(ReportSettings settings, string[] emails)
		{
			this.MaxValue = emails.Length;

			_startTime = DateTime.Now;
			_timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));

			ThreadPool.QueueUserWorkItem(ignore =>
			{
				using (var ce = new CountdownEvent(Math.Min(8, emails.Length)))
				{
					for (var i = 0; i < ce.InitialCount; i++)
					{
						//ThreadPool.QueueUserWorkItem(ProcessBatchMails, new object[] { ce, settings, emails });
					}
					ce.Wait();
				}

				_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

				this.CurrentValue = 0;
				this.ProgressStep = @"Generating Excel file ...";

				// Sort
				//_results.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

				//GenerateExcel(_results);
			});
		}

		//		private void ProcessBatchMails(object arg)
		//		{
		//			var pms = arg as object[];
		//			var e = pms[0] as CountdownEvent;
		//			var settings = pms[1] as ReportSettings;
		//			var emails = pms[2] as string[];
		//			try
		//			{
		//				var buffer = new byte[4 * 1024];
		//				var login = new WebLogin(settings.Username, settings.Password);
		//				while (true)
		//				{
		//					var name = GetBatch(emails);
		//					if (name == null)
		//					{
		//						break;
		//					}
		//					try
		//					{
		//						var s = Stopwatch.StartNew();
		//						//var tmp = WebQuery.DownloadContents(login, WebQuery.GetMessageTraceQuery(settings.Date, name), buffer);

		//						//var result = ReportGenerator.Parse(WebQuery.DownloadContents(login, WebQuery.GetMessageTraceQuery(settings.Date, name), buffer));
		//						var result = ReportGenerator.Parse(WebQuery.DownloadContents(login,
		//@"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$filter=StartDate%20eq%20datetime'2015-06-18T00%3A00%3A00'%20and%20EndDate%20eq%20datetime'2015-06-18T23%3A59%3A59'%20&$select=SenderAddress,RecipientAddress,Size%20&$skiptoken=1999&$format=json", buffer));

		//						var entries = result.Item1;
		//						while (result.Item2 != string.Empty)
		//						{
		//							result = ReportGenerator.Parse(WebQuery.DownloadContents(login, result.Item2, buffer));
		//							entries.AddRange(result.Item1);

		//							Debug.WriteLine(s.ElapsedMilliseconds);
		//						}

		//						s.Stop();
		//						Debug.WriteLine(s.ElapsedMilliseconds);

		//						var inbound = ReportGenerator.ParseJson(
		//							WebQuery.DownloadContents(login, WebQuery.GetMessageTraceRecipientQuery(settings.Date, name), buffer));
		//						var outbound = ReportGenerator.ParseJson(
		//							WebQuery.DownloadContents(login, WebQuery.GetMessageTraceSenderQuery(settings.Date, name), buffer));

		//						lock (ProgressSync)
		//						{
		//							_results.Add(new ReportEntry(name, inbound.Item1,
		//								inbound.Item2, outbound.Item1, outbound.Item2));
		//							_processed++;
		//							this.CurrentValue = _processed;
		//						}
		//					}
		//					catch
		//					{
		//					}
		//				}
		//			}
		//			catch
		//			{
		//			}
		//			finally
		//			{
		//				e.Signal();
		//			}
		//		}

		//private void GenerateExcel(IEnumerable<ReportEntry> entries)
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

		//			this.CurrentValue += 1;
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

		//	this.ProgressStep = @"Complete";
		//	this.IsIdle = true;

		//	Process.Start(new ProcessStartInfo(@"excel", excelFile.FullName));
		//}
	}
}