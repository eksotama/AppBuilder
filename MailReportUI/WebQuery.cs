using System;
using System.IO;
using System.Net;
using System.Text;

namespace MailReportUI
{
	public static class WebQuery
	{
		private static readonly string MessageTraceSenderTemplate = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$select=Size & $filter=StartDate eq datetime'{0}T00:00:00' and EndDate eq datetime'{0}T23:59:59' and SenderAddress eq '{1}' &$format=json";
		private static readonly string MessageTraceRecipientTemplate = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$select=Size & $filter=StartDate eq datetime'{0}T00:00:00' and EndDate eq datetime'{0}T23:59:59' and RecipientAddress eq '{1}' &$format=json";
		private static readonly string DateFormat = @"yyyy-MM-dd";

		public static string GetMessageTraceSenderQuery(DateTime date, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return GetMessageTrace(date, name, MessageTraceSenderTemplate);
		}

		public static string GetMessageTraceRecipientQuery(DateTime date, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return GetMessageTrace(date, name, MessageTraceRecipientTemplate);
		}

		private static string GetMessageTrace(DateTime date, string name, string template)
		{
			return string.Format(template, date.ToString(DateFormat), name);
		}

		public static string DownloadContents(WebLogin login, string url, byte[] buffer)
		{
			if (login == null) throw new ArgumentNullException("login");
			if (url == null) throw new ArgumentNullException("url");
			if (buffer == null) throw new ArgumentNullException("buffer");

			var request = WebRequest.Create(url);

			request.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
			request.Credentials = new NetworkCredential(login.Username, login.Password);

			using (var response = request.GetResponse())
			{
				using (var dataStream = response.GetResponseStream())
				{
					using (var ms = new MemoryStream())
					{
						int readedBytes;
						while ((readedBytes = dataStream.Read(buffer, 0, buffer.Length)) != 0)
						{
							ms.Write(buffer, 0, readedBytes);
						}
						return Encoding.ASCII.GetString(ms.ToArray());
					}
				}
			}
		}
	}
}