using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Organizer
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private readonly PersonItemViewModel _viewModel = new PersonItemViewModel(new WinRtDialog());

		public MainPage()
		{
			this.InitializeComponent();
			this.DataContext = _viewModel;
		}
	}

	public sealed class DelegateCommand<T> : ICommand where T : class
	{
		private readonly Func<T, bool> _canExecuteMethod;
		private readonly Action<T> _executeMethod;

		#region Constructors

		public DelegateCommand(Action<T> executeMethod)
			: this(executeMethod, null)
		{
		}

		public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
		{
			_executeMethod = executeMethod;
			_canExecuteMethod = canExecuteMethod;
		}

		#endregion Constructors

		#region ICommand Members

		public event EventHandler CanExecuteChanged;

		bool ICommand.CanExecute(object parameter)
		{
			var value = parameter as T;
			if (value != null)
			{
				return this.CanExecute(value);
			}
			return false;
		}

		void ICommand.Execute(object parameter)
		{
			Execute((T)parameter);
		}

		#endregion ICommand Members

		#region Public Methods

		public bool CanExecute(T parameter)
		{
			return ((_canExecuteMethod == null) || _canExecuteMethod(parameter));
		}

		public void Execute(T parameter)
		{
			if (_executeMethod != null)
			{
				_executeMethod(parameter);
			}
		}

		public void RaiseCanExecuteChanged()
		{
			OnCanExecuteChanged(EventArgs.Empty);
		}

		#endregion Public Methods

		#region Protected Methods

		private void OnCanExecuteChanged(EventArgs e)
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion Protected Methods
	}

	public sealed class iOsDialog
	{
		private EventHandler Accept;
		private EventHandler Cancel;

		public string Message { get; set; }

		public void DisplayMessage(string message)
		{
			var dialog = this;
			dialog.Message = message;
			dialog.Accept += (sender, args) =>
							 {

							 };
			dialog.Cancel += (sender, args) =>
							 {

							 };
			dialog.Show();
		}

		private void Show()
		{
			throw new NotImplementedException();
		}
	}

	public sealed class WinRtDialog : IDialogInterface
	{
		public async Task<bool?> DisplayAsync(string message)
		{
			var dialog = new MessageDialog(message);
			dialog.Commands.Add(new UICommand(@"OK"));
			await dialog.ShowAsync();
			return false;
		}

		public async Task<bool?> ConfirmAsync(string message)
		{
			var dialog = new MessageDialog(message);
			dialog.Commands.Add(new UICommand(@"Yes", null, true));
			dialog.Commands.Add(new UICommand(@"No", null, false));
			var cmd = await dialog.ShowAsync();
			return (bool)cmd.Id;
		}
	}

	public abstract class BindableObject : INotifyPropertyChanged
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

	public sealed class PersonItem : BindableObject
	{
		private string _name = string.Empty;
		public string Name
		{
			get { return _name; }
			set { this.SetField(ref _name, value); }
		}

		private string _email = string.Empty;
		public string Email
		{
			get { return _email; }
			set { this.SetField(ref _email, value); }
		}
	}

	public interface IDialogInterface
	{
		Task<bool?> DisplayAsync(string message);
		Task<bool?> ConfirmAsync(string message);
	}

	public sealed class PersonItemViewModel : BindableObject
	{
		public IDialogInterface Dialog { get; set; }

		private bool _isIdle = true;
		public bool IsIdle
		{
			get { return _isIdle; }
			set { this.SetField(ref _isIdle, value); }
		}

		public ObservableCollection<PersonItem> Items { get; private set; }

		public DelegateCommand<PersonItem> AddCommand { get; private set; }
		public DelegateCommand<PersonItem> DeleteCommand { get; private set; }

		public PersonItem NewItem { get; set; }

		public PersonItemViewModel(IDialogInterface dialog)
		{
			if (dialog == null) throw new ArgumentNullException("dialog");

			this.Dialog = dialog;
			this.Items = new ObservableCollection<PersonItem>();
			this.AddCommand = new DelegateCommand<PersonItem>(this.Add, this.CanExecuteAdd);
			this.DeleteCommand = new DelegateCommand<PersonItem>(this.Delete, this.CanExecuteDelete);
			this.NewItem = new PersonItem();
			this.NewItem.PropertyChanged += (sender, args) => this.AddCommand.RaiseCanExecuteChanged();
		}

		private bool CanExecuteAdd(PersonItem input)
		{
			return !string.IsNullOrWhiteSpace(input.Name) &&
				   !string.IsNullOrWhiteSpace(input.Email);
		}

		private bool CanExecuteDelete(PersonItem input)
		{
			return input != null;
		}

		public async void Add(PersonItem input)
		{
			try
			{
				this.IsIdle = false;

				if (input.Name == @"a")
				{
					await this.Dialog.DisplayAsync(@"Cannot insert person with this 'reserved' name.");
					return;
				}

				this.Items.Add(new PersonItem { Name = input.Name, Email = input.Email });

				this.NewItem.Name = string.Empty;
				this.NewItem.Email = string.Empty;
			}
			finally
			{
				this.IsIdle = true;
			}
		}

		private async void Delete(PersonItem input)
		{
			var confirmed = await this.Dialog.ConfirmAsync(@"Are you sure you want to delete this item?") ?? false;
			if (confirmed)
			{
				try
				{
					this.IsIdle = false;

					await Task.Delay(TimeSpan.FromSeconds(3));
					this.Items.Remove(input);
				}
				finally
				{
					this.IsIdle = true;
				}
			}
		}
	}

	public sealed class ObjectToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
