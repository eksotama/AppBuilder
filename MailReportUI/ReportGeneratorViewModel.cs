using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using OfficeOpenXml;

namespace MailReportUI
{


	public sealed class ReportGeneratorViewModel : BindableObject
	{
		public ReportSettings Settings { get; private set; }

		private string _progressStep = string.Empty;
		public string ProgressStep
		{
			get { return _progressStep; }
			set { this.SetField(ref _progressStep, value); }
		}

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
		}

		public void PasswordChanged(string password)
		{
			// Set the password. Settings will notify for Property changed.
			this.Settings.Password = password;
		}

		private bool CanGenerateReport(ReportSettings arg)
		{
			if (arg == null) throw new ArgumentNullException("arg");

			return !string.IsNullOrWhiteSpace(arg.Url) &&
				   !string.IsNullOrWhiteSpace(arg.Username) &&
				   !string.IsNullOrWhiteSpace(arg.Password);
		}

		private async Task GenerateReport(ReportSettings settings)
		{
			try
			{
				this.IsIdle = false;
				this.ProgressStep = @"Connecting to the server ...";
#if DEBUG
				settings.Password = @"Kafo7315";
#endif
				var url = (settings.Url ?? string.Empty).Trim();
				if (!url.EndsWith(@"/"))
				{
					url += @"/";
				}
				url += @"MessageTrace?$select=StartDate,EndDate,SenderAddress,RecipientAddress,Size";

				var hard = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MailTrafficTop?$filter=AggregateBy eq 'Day' and StartDate eq datetime'2015-06-16T00:00:00' and EndDate eq datetime'2015-06-16T23:59:59'";
				//url = WebQuery.GetMailTraficQuery(DateTime.Today);



				this.ProgressStep = @"Generating report ...";
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				this.IsIdle = true;
			}
		}
	}
}