namespace MailReportUI
{
	public sealed class ReportSettings : BindableObject
	{
		private string _url = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc";
		private string _username = @"O365.Reporting.SA@CCHellenic.onmicrosoft.com";
		private string _password = @"Kafo7315";

		public string Url
		{
			get { return _url; }
			set { this.SetField(ref _url, value); }
		}

		public string Username
		{
			get { return _username; }
			set { this.SetField(ref _username, value); }
		}

		public string Password
		{
			get { return _password; }
			set { this.SetField(ref _password, value); }
		}

		public ReportSettings()
		{
#if !DEBUG
			this.Url = string.Empty;
			this.Username = string.Empty;
			this.Password = string.Empty;
#endif
		}
	}
}