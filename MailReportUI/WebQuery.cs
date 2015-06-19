using System;
using System.Collections.Generic;
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

		public static List<DateRange> GetDayRanges(DateTime? begin = null, DateTime? end = null)
		{
			var ranges = new List<DateRange>(1440);

			var startTime = begin ?? DateTime.Today;
			var limitTime = end ?? startTime.Add(DateTime.Now.TimeOfDay);

			if (startTime == DateTime.Today)
			{
				limitTime = startTime.Add(DateTime.Now.TimeOfDay);
			}

			while (startTime < limitTime)
			{
				var endTime = startTime.AddMinutes(1);

				var dr = new DateRange();

				dr.StartTime = startTime;
				dr.EndTime = endTime;

				ranges.Add(dr);

				startTime = endTime;
			}

			return ranges;
		}

		public static string GetQueryUrl(DateTime startTime, DateTime endTime)
		{
			var template = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$filter=StartDate%20eq%20datetime'{0}'%20and%20EndDate%20eq%20datetime'{1}'%20&$select=RecipientAddress,SenderAddress,Size&$format=Json";

			return string.Format(template, startTime.ToString(@"s"), endTime.ToString(@"s"));
		}

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

			request.Timeout = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
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

	public sealed class MsgTraceEntry
	{
		public string Sender = string.Empty;
		public string Recipient = string.Empty;
		public long Size = 0;
	}

	public sealed class DateRange
	{
		public DateTime StartTime;
		public DateTime EndTime;
	}
}