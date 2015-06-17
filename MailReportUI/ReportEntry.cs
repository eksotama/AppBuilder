using System;

namespace MailReportUI
{
	public sealed class ReportEntry
	{
		public string Name { get; private set; }
		public int Inbound { get; private set; }
		public int InboundSize { get; private set; }
		public int Outbound { get; private set; }
		public int OutboundSize { get; private set; }
		public object[] Values { get; private set; }

		public ReportEntry(string name, int inbound, int inboundSize, int outbound, int outboundSize)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Name = name;
			this.Inbound = inbound;
			this.InboundSize = inboundSize;
			this.Outbound = outbound;
			this.OutboundSize = outboundSize;
			this.Values = new object[]
			              {
				              name,
							  inbound,
							  inboundSize,
							  outbound,
							  outboundSize
			              };
		}
	}
}