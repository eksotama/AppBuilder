using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Architecture.Data;
using Architecture.Dialog;
using Architecture.Helpers;
using Architecture.Objects;
using Architecture.Validation;

namespace Demo
{
	//
	// Read Only scenario
	//
	public sealed class Brand : IReadOnlyObject
	{
		public long Id { get; private set; }
		public string Name { get; private set; }

		public Brand(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		public void Fill(Dictionary<long, Brand> items)
		{
			if (items == null) throw new ArgumentNullException("items");

			// SQL easy with my tool
			throw new NotImplementedException();
		}
	}

	public sealed class BrandHelper : Helper<Brand>
	{
	}



	public sealed class Article : IReadOnlyObject
	{
		public long Id { get; private set; }
		public string Name { get; private set; }
		public Brand Brand { get; private set; }

		public Article(long id, string name, Brand brand)
		{
			this.Id = id;
			this.Name = name;
			this.Brand = brand;
		}
	}

	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private readonly Dictionary<long, Brand> _brands;

		public ArticleAdapter(Dictionary<long, Brand> brands)
		{
			if (brands == null) throw new ArgumentNullException("brands");

			_brands = brands;
		}

		public void Fill(Dictionary<long, Article> items)
		{
			if (items == null) throw new ArgumentNullException("items");

			// SQL : Easy with the tool
			throw new NotImplementedException();
		}
	}

	public sealed class ArticleHelper : Helper<Article>
	{
	}


	//
	// Read Only scenario with dependencies
	//
	public static class Code
	{
		public static async void Run()
		{
			var bh = new BrandHelper();
			var ah = new ArticleHelper();

			bh.Load(new BrandAdapter());
			ah.Load(new ArticleAdapter(bh.Items));

			//var m = new LogMessageManager(new LogMessageAdapter(), new LogMessageValidator(), new LogMessageSettings());
			//m.Load(DateTime.Today);

			var item = default(LogMessage);
			var dialog = default(IDialog);
			//var results = await m.InsertAsync(item, dialog);

			var viewManager = new LogMessageViewManager(new LogMessageManager(new LogMessageAdapter()), new LogMessageSettings());


		}
	}

	public abstract class BindableBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;

			field = value;
			OnPropertyChanged(propertyName);
		}
	}



	public sealed class LogMessageViewManager : BindableBase
	{
		// For the logic !!!
		public LogMessageManager Manager { get; private set; }
		public LogMessageSettings Settings { get; private set; }
		public ObservableCollection<LogMessageViewItem> ViewItems { get; private set; }

		public LogMessageViewManager(LogMessageManager manager, LogMessageSettings settings)
		{
			if (manager == null) throw new ArgumentNullException("manager");
			if (settings == null) throw new ArgumentNullException("settings");

			this.Manager = manager;
			this.Settings = settings;
			this.ViewItems = new ObservableCollection<LogMessageViewItem>();
		}

		public void Load(DateTime date)
		{
			this.ViewItems.Clear();
			foreach (var item in this.Manager.GetAll(date))
			{
				this.ViewItems.Add(new LogMessageViewItem(item));
			}

			// TODO : Sort & Filter
		}

		public async Task AddNew(LogMessageViewItem viewItem, IDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException("viewItem");
			if (dialog == null) throw new ArgumentNullException("dialog");

			// Check Contents
			var validationResult = Validator.ValidateNotEmpty(viewItem.Content, @"Contents cannot be empty");
			if (validationResult != ValidationResult.Success)
			{
				await dialog.DisplayAsync(this.Settings.ItemAlreadyHandledMsg);
				return;
			}

			// Call the manager to add the new message
			this.Manager.AddNew(viewItem.LogMessage);

			// Add the item to the list to the right place if sorter != null
			this.ViewItems.Add(viewItem);
		}

		public async Task MarkHandled(LogMessageViewItem viewItem, IDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException("viewItem");
			if (dialog == null) throw new ArgumentNullException("dialog");

			// Check if the item isn't already handled
			var isAlreadyHandled = viewItem.IsHandled;
			if (isAlreadyHandled)
			{
				await dialog.DisplayAsync(this.Settings.ItemAlreadyHandledMsg);
				return;
			}

			// Check if we have other message which is critical and it isn't handled
			var messages = this.ViewItems.Select(v => v.LogMessage);
			var currentMessage = viewItem.LogMessage;
			var hasUnhandledCriticalMessage = this.Manager.HasUnhandledCriticalMessage(messages, currentMessage);
			if (hasUnhandledCriticalMessage)
			{
				await dialog.DisplayAsync(this.Settings.HasOtherUnhandledCriticalMessage);
				return;
			}

			// Request confirmation for marking the item as handled
			var confirmation = await dialog.ConfirmAsync(this.Settings.ConfirmMarkHandledMsg);
			if (confirmation != ConfirmationResult.Accept) return;

			// Mark the viewItem as Handled
			viewItem.IsHandled = true;

			// Call the manager to update the message
			this.Manager.MarkHandled(currentMessage);
		}
	}

	public sealed class LogMessageManager
	{
		public LogMessageAdapter Adapter { get; private set; }

		public LogMessageManager(LogMessageAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			this.Adapter = adapter;
		}

		public bool HasUnhandledCriticalMessage(IEnumerable<LogMessage> messages, LogMessage excludeMessage)
		{
			if (messages == null) throw new ArgumentNullException("messages");
			if (excludeMessage == null) throw new ArgumentNullException("excludeMessage");

			foreach (var message in messages)
			{
				if (message == excludeMessage)
				{
					continue;
				}
				if (this.IsUnhandledCriticalItem(message))
				{
					return false;
				}
			}

			return true;
		}

		public bool IsUnhandledCriticalItem(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			return !logMessage.IsHandled && logMessage.Content.IndexOf(@"Critical", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public List<LogMessage> GetAll(DateTime date)
		{
			return this.Adapter.GetAll(date);
		}

		public void AddNew(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			// Add the item to the db
			this.Adapter.Insert(logMessage);
		}

		public void MarkHandled(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			// Mask the message as handled
			logMessage.IsHandled = true;

			// Update the message in the db
			this.Adapter.Update(logMessage);
		}


	}

	public sealed class LogMessageViewItem : BindableBase
	{
		private readonly LogMessage _logMessage;

		public LogMessage LogMessage
		{
			get { return _logMessage; }
		}

		public string Content
		{
			get { return LogMessage.Content; }
			set
			{
				LogMessage.Content = value;
				this.OnPropertyChanged();
			}
		}

		public bool IsHandled
		{
			get { return LogMessage.IsHandled; }
			set
			{
				LogMessage.IsHandled = value;
				this.OnPropertyChanged();
			}
		}

		public LogMessageViewItem(LogMessage logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			_logMessage = logMessage;
		}
	}

	public sealed class LogMessageSettings
	{
		public string CannotDeleteCriticalMsg { get; private set; }
		public string HasOtherUnhandledCriticalMessage { get; private set; }
		public string MaximumLimitExceededMsg { get; private set; }
		public int MaxItems { get; private set; }
		public string ConfirmDeleteMsg { get; private set; }
		public string ConfirmMarkHandledMsg { get; private set; }
		public string ItemAlreadyHandledMsg { get; private set; }
	}

	public sealed class LogMessageAdapter : IModifiableAdapter<LogMessage>
	{
		public List<LogMessage> GetAll(DateTime date)
		{
			throw new NotImplementedException();
		}

		public void Insert(LogMessage item)
		{
			throw new NotImplementedException();
		}

		public void Update(LogMessage item)
		{
			throw new NotImplementedException();
		}

		public void Delete(LogMessage item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class LogMessage : IModifiableObject
	{
		public long Id { get; set; }
		public string Content { get; set; }
		public bool IsHandled { get; set; }

		public LogMessage(long id, string content, bool isHandled)
		{
			this.Id = id;
			this.Content = content;
			this.IsHandled = isHandled;
		}
	}
}