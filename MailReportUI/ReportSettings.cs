namespace MailReportUI
{
	public sealed class ReportSettings : BindableObject
	{
		private string _username = string.Empty;
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