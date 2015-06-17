using System;
using System.Xml.Linq;

namespace MailReportUI
{
	public static class ReportGenerator
	{
		private static readonly string RootNs = @"http://www.w3.org/2005/Atom";
		private static readonly string PropertiesNs = @"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
		private static readonly string ValuesNs = @"http://schemas.microsoft.com/ado/2007/08/dataservices";

		private static readonly XName PropertiesName = XName.Get(@"properties", PropertiesNs);
		private static readonly XName SizeName = XName.Get(@"Size", ValuesNs);
		private static readonly XName FeedName = XName.Get(@"feed", RootNs);
		private static readonly XName EntryName = XName.Get(@"entry", RootNs);
		private static readonly XName ContentName = XName.Get(@"content", RootNs);

		private static readonly string StartXmlFlag = @"<d:Size m:type=""Edm.Int32"">";
		private static readonly string EndXmlFlag = @"</d:Size>";

		private static readonly string StartJsonFlag = @"""Size"":";
		private static readonly string EndJsonFlag = @"}";

		public static Tuple<int, int> ParseXml(string contents)
		{
			if (contents == null) throw new ArgumentNullException("contents");

			return Parse(contents, StartXmlFlag, EndXmlFlag);
		}

		public static Tuple<int, int> ParseJson(string contents)
		{
			if (contents == null) throw new ArgumentNullException("contents");

			return Parse(contents, StartJsonFlag, EndJsonFlag);
		}

		private static Tuple<int, int> Parse(string contents, string startFlag, string endFlag)
		{
			var count = 0;
			var size = 0;

			var offset = 0;

			while (true)
			{
				var start = contents.IndexOf(startFlag, offset, StringComparison.OrdinalIgnoreCase);
				if (start < 0)
				{
					break;
				}
				start += startFlag.Length;

				var end = contents.IndexOf(endFlag, start, StringComparison.OrdinalIgnoreCase);

				size += int.Parse(contents.Substring(start, end - start));
				count++;

				offset = end;
			}

			return Tuple.Create(count, size);
		}

		public static Tuple<int, int> ParseXml(XDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");

			var count = 0;
			var size = 0;

			foreach (var entry in document
				.Elements(FeedName)
				.Elements(EntryName)
				.Elements(ContentName))
			{
				size += int.Parse(entry.Element(PropertiesName).Element(SizeName).Value);
				count++;
			}

			return Tuple.Create(count, size);
		}
	}
}