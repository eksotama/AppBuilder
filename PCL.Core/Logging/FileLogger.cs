using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Core.Logging
{
	public abstract class FileLogger : IDisposable
	{
		private DateTime _currentDate;
		private string _currentFilePath = string.Empty;

		private readonly string _folderPath;
		private readonly TimeSpan _period;
		private readonly Timer _flushTimer;
		private readonly object _sync = new object();
		private readonly List<LogMessage> _messages = new List<LogMessage>();
		private readonly string[] _levelNames;

		protected FileLogger(string folderPath, TimeSpan period)
		{
			if (folderPath == null) throw new ArgumentNullException("folderPath");

			_folderPath = folderPath;
			_period = period;
			_flushTimer = new Timer(this.Flush, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			_levelNames = typeof(LogLevel).GetRuntimeFields()
				.Where(f => f.IsPublic && !f.IsSpecialName)
				.Select(f => f.Name).ToArray();
		}

		protected abstract void WriteToFile(string filePath, string contents);

		public void Start()
		{
			_flushTimer.Change(_period, _period);
		}

		public void Stop()
		{
			_flushTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

			this.Flush(null);
		}

		public void Trace(string message)
		{
			this.AddLogMessage(message, LogLevel.Trace);
		}

		public void Debug(string message)
		{
			this.AddLogMessage(message, LogLevel.Debug);
		}

		public void Info(string message)
		{
			this.AddLogMessage(message, LogLevel.Info);
		}

		public void Warn(string message)
		{
			this.AddLogMessage(message, LogLevel.Warn);
		}

		public void Error(string message)
		{
			this.AddLogMessage(message, LogLevel.Error);
		}

		public void Fatal(string message)
		{
			this.AddLogMessage(message, LogLevel.Fatal);
		}

		public void Flush()
		{
			lock (_sync)
			{
				var buffer = new StringBuilder(_messages.Count * 16);

				if (_messages.Count > 0)
				{
					foreach (var message in _messages)
					{
						buffer.Append(message.Date.ToString(@"HH:mm:ss"));
						buffer.Append('|');
						buffer.Append(_levelNames[(int)message.Level]);
						buffer.Append('|');
						buffer.AppendLine(message.Content);
					}

					_messages.Clear();

					var today = DateTime.Today;
					if (today != _currentDate)
					{
						_currentFilePath = Path.Combine(_folderPath, today.ToString(@"dd-MM-yyyy") + @".txt");
						_currentDate = today;
					}

					WriteToFile(_currentFilePath, buffer.ToString());
				}
			}
		}

		private void Flush(object state)
		{
			try
			{
				this.Flush();
			}
			catch { }
		}

		private void AddLogMessage(string message, LogLevel logLevel)
		{
			lock (_sync)
			{
				_messages.Add(new LogMessage(DateTime.Now, logLevel, message));
			}
		}

		public void Dispose()
		{
			this.Stop();
			_flushTimer.Dispose();
		}
	}
}