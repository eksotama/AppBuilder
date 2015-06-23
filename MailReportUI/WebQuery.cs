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

		public static string GetMessageTraceQuery(DateTime date, string name)
		{
			return
				string.Format(
					@"https://reports.office365.com/ecp/reportingwebservice/reporting.svc/MessageTrace?$select=SenderAddress,RecipientAddress,Size & $filter=StartDate eq datetime'{0}T00:00:00' and EndDate eq datetime'{0}T23:59:59' &$format=json",
					date.ToString(DateFormat),
					name
					);
		}
	}
}