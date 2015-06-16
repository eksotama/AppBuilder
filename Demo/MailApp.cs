using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Demo
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

	public static class WebQuery
	{
		private static readonly string MailTrafficTemplate = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MailTrafficTop?$select=Name,Direction,MessageCount&$filter=AggregateBy eq 'Day' and StartDate eq datetime'{0}T00:00:00' and EndDate eq datetime'{0}T23:59:59'";
		private static readonly string MessageTraceSenderTemplate = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$filter=StartDate eq datetime'{0}T00:00:00' and EndDate eq datetime'{0}T23:59:59' and SenderAddress eq '{1}'";
		private static readonly string MessageTraceRecipientTemplate = @"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$filter=StartDate eq datetime'{0}T00:00:00' and EndDate eq datetime'{0}T23:59:59' and RecipientAddress eq '{1}'";
		private static readonly string DateFormat = @"yyyy-MM-dd";

		public static string GetMailTrafficQuery(DateTime date)
		{
			return string.Format(MailTrafficTemplate, date.ToString(DateFormat));
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

		public static async Task<string> DownloadContentsAsync(WebLogin login, string url)
		{
			if (login == null) throw new ArgumentNullException("login");
			if (url == null) throw new ArgumentNullException("url");

			var request = WebRequest.Create(url);

			request.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
			request.Credentials = new NetworkCredential(login.Username, login.Password);

			using (var response = await request.GetResponseAsync())
			{
				using (var dataStream = response.GetResponseStream())
				{
					using (var ms = new MemoryStream())
					{
						var buffer = new byte[8 * 1024];

						int readedBytes;
						while ((readedBytes = await dataStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
						{
							ms.Write(buffer, 0, readedBytes);
						}
						return Encoding.ASCII.GetString(ms.ToArray());
					}
				}
			}
		}
	}

	public static class ReportGenerator
	{
		//public static ReportEntry[] CreateReportEntries(XDocument document)
		//{
		//	if (document == null) throw new ArgumentNullException("document");

		//	// Parse all mail entries
		//	var mailEntries = ParseMailEntries(document);

		//	// Extract unique senders
		//	var uniqueSenders = mailEntries
		//		.Select(e => e.SenderAddress.ToLowerInvariant())
		//		.Distinct()
		//		.ToArray();

		//	Array.Sort(uniqueSenders);

		//	// Create report entries
		//	var reportEntries = new ReportEntry[uniqueSenders.Length];

		//	var index = 0;
		//	foreach (var sender in uniqueSenders)
		//	{
		//		var report = new ReportEntry(sender);

		//		foreach (var entry in mailEntries)
		//		{
		//			if (entry.SenderAddress.Equals(sender, StringComparison.OrdinalIgnoreCase))
		//			{
		//				report.Outbound++;
		//				report.OutboundSize += entry.Size;
		//			}
		//			if (entry.RecipientAddress.Equals(sender, StringComparison.OrdinalIgnoreCase))
		//			{
		//				report.Inbound++;
		//				report.InboundSize += entry.Size;
		//			}
		//		}

		//		reportEntries[index++] = report;
		//	}
		//	return reportEntries;
		//}

		public static List<MailUserEntry> ParseMailUserEntries(XDocument document)
		{
			var rootNs = @"http://www.w3.org/2005/Atom";
			var propertiesNs = @"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
			var valuesNs = @"http://schemas.microsoft.com/ado/2007/08/dataservices";

			var userEntries = new List<MailUserEntry>(128);

			var propertiesName = XName.Get(@"properties", propertiesNs);
			var nameName = XName.Get(@"Name", valuesNs);
			var directionName = XName.Get(@"Direction", valuesNs);
			var messageCountName = XName.Get(@"MessageCount", valuesNs);

			foreach (var entry in document
				.Elements(XName.Get(@"feed", rootNs))
				.Elements(XName.Get(@"entry", rootNs))
				.Elements(XName.Get(@"content", rootNs)))
			{
				var properties = entry.Element(propertiesName);

				var name = properties.Element(nameName);
				var direction = properties.Element(directionName);
				var messageCount = properties.Element(messageCountName);

				userEntries.Add(new MailUserEntry(name.Value, GetMailDirection(direction), int.Parse(messageCount.Value)));
			}

			return userEntries;
		}

		public static ReportEntry[] CreateReportEntries(List<MailUserEntry> entries)
		{
			if (entries == null) throw new ArgumentNullException("entries");

			var names = GetUniqueNames(entries);
			var reportEntries = new ReportEntry[names.Count];

			var index = 0;
			foreach (var name in names)
			{
				var reportName = string.Empty;
				var inbound = 0;
				var outbound = 0;

				foreach (var entry in entries)
				{
					if (entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						reportName = entry.Name;
						switch (entry.Direction)
						{
							case MailDirection.Inbound:
								inbound += entry.MessageCount;
								break;
							case MailDirection.Outbound:
								outbound += entry.MessageCount;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
				}

				reportEntries[index++] = new ReportEntry(reportName, inbound, outbound);
			}

			return reportEntries;
		}

		private static HashSet<string> GetUniqueNames(List<MailUserEntry> entries)
		{
			var names = new HashSet<string>();

			foreach (var entry in entries)
			{
				names.Add(entry.Name.ToLowerInvariant());
			}

			return names;
		}

		private static List<MailEntry> ParseMailEntries(XDocument document)
		{
			var rootNs = @"http://www.w3.org/2005/Atom";
			var propertiesNs = @"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
			var valuesNs = @"http://schemas.microsoft.com/ado/2007/08/dataservices";

			var mailEntries = new List<MailEntry>();

			var propertiesName = XName.Get(@"properties", propertiesNs);
			var startDateName = XName.Get(@"StartDate", valuesNs);
			var endDateName = XName.Get(@"EndDate", valuesNs);
			var senderAddressName = XName.Get(@"SenderAddress", valuesNs);
			var recipientAddressName = XName.Get(@"RecipientAddress", valuesNs);
			var sizeName = XName.Get(@"Size", valuesNs);

			foreach (var entry in document
				.Elements(XName.Get(@"feed", rootNs))
				.Elements(XName.Get(@"entry", rootNs))
				.Elements(XName.Get(@"content", rootNs)))
			{
				var properties = entry.Element(propertiesName);

				var startDate = properties.Element(startDateName);
				var endDate = properties.Element(endDateName);
				var senderAddress = properties.Element(senderAddressName);
				var recipientAddress = properties.Element(recipientAddressName);
				var size = properties.Element(sizeName);

				mailEntries.Add(new MailEntry(DateTime.Parse(startDate.Value), DateTime.Parse(endDate.Value), senderAddress.Value, recipientAddress.Value, int.Parse(size.Value)));
			}

			return mailEntries;
		}

		private static MailDirection GetMailDirection(XElement element)
		{
			var value = element.Value;
			if (value.Equals(@"Inbound", StringComparison.OrdinalIgnoreCase))
			{
				return MailDirection.Inbound;
			}
			if (value.Equals(@"Outbound", StringComparison.OrdinalIgnoreCase))
			{
				return MailDirection.Outbound;
			}
			throw new ArgumentOutOfRangeException("element");
		}
	}

	public sealed class ReportEntry
	{
		public string Name { get; private set; }
		public int Inbound { get; private set; }
		public int Outbound { get; private set; }

		public int InboundSize { get; set; }
		public int OutboundSize { get; set; }

		public ReportEntry(string name, int inbound, int outbound)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.Inbound = inbound;
			this.Outbound = outbound;
		}
	}

	public sealed class MailUserEntry
	{
		public string Name { get; private set; }
		public MailDirection Direction { get; private set; }
		public int MessageCount { get; private set; }

		public MailUserEntry(string name, MailDirection direction, int messageCount)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.Direction = direction;
			this.MessageCount = messageCount;
		}
	}

	public enum MailDirection
	{
		Inbound,
		Outbound
	}

	public sealed class MailEntry
	{
		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }
		public string RecipientAddress { get; private set; }
		public string SenderAddress { get; private set; }
		public int Size { get; private set; }

		public MailEntry(DateTime startDate, DateTime endDate, string recipientAddress, string senderAddress, int size)
		{
			if (recipientAddress == null) throw new ArgumentNullException("recipientAddress");
			if (senderAddress == null) throw new ArgumentNullException("senderAddress");

			this.StartDate = startDate;
			this.EndDate = endDate;
			this.RecipientAddress = recipientAddress;
			this.SenderAddress = senderAddress;
			this.Size = size;
		}
	}
}