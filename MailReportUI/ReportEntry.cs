using System;

namespace MailReportUI
{
	public sealed class ReportEntry
	{
		public string Name;
		public long Inbound;
		public long InboundSize;
		public long Outbound;
		public long OutboundSize;

		public object[] Values
		{
			get
			{
				return new object[]
				       {
					       this.Name,
					       this.Inbound,
					       this.InboundSize,
					       this.Outbound,
					       this.OutboundSize
				       };
			}
		}

		public ReportEntry(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
		}

		public ReportEntry(string name, long inbound, long inboundSize, long outbound, long outboundSize)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.Inbound = inbound;
			this.InboundSize = inboundSize;
			this.Outbound = outbound;
			this.OutboundSize = outboundSize;
		}
	}
}