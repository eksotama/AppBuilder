using System;

namespace MailReportUI
{
	public sealed class ReportSettings : BindableObject
	{
		private DateTime _date = DateTime.Today;
		public DateTime Date
		{
			get { return _date; }
			set { this.SetField(ref _date, value); }
		}

		private string _username = @"O365.Reporting.SA@CCHellenic.onmicrosoft.com";
		public string Username
		{
			get { return _username; }
			set { this.SetField(ref _username, value); }
		}

		private string _password = string.Empty;
		public string Password
		{
			get { return _password; }
			set { this.SetField(ref _password, value); }
		}
	}
}