using System;

namespace MailReportUI
{
	public sealed class WebLogin
	{
		public string Username { get; private set; }
		public string Password { get; private set; }

		public WebLogin(string username, string password)
		{
			if (username == null) throw new ArgumentNullException("username");
			if (password == null) throw new ArgumentNullException("password");

			this.Username = username;
			this.Password = password;
		}
	}
}