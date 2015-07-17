using System;

namespace Core.Logging
{
	public sealed class LogMessage
	{
		public DateTime Date { get; private set; }
		public LogLevel Level { get; private set; }
		public string Content { get; private set; }

		public LogMessage(DateTime date, LogLevel level, string content)
		{
			if (content == null) throw new ArgumentNullException("content");

			this.Date = date;
			this.Level = level;
			this.Content = content;
		}
	}
}