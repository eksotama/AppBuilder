using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Architecture.Data;
using Architecture.Dialog;
using Architecture.Objects;
using Architecture.Validation;



namespace Demo
{
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
		public LoginManager Manager { get; private set; }

		public ObservableCollection<LoginViewItem> ViewItems { get; private set; }

		public LogMessageViewManager(LoginManager manager)
		{
			if (manager == null) throw new ArgumentNullException("manager");

			this.Manager = manager;
			this.ViewItems = new ObservableCollection<LoginViewItem>();
		}

		public void Load(DateTime date)
		{
			this.Manager.Load(date);

			this.ViewItems.Clear();
			foreach (var item in this.Manager.Logins)
			{
				this.ViewItems.Add(new LoginViewItem(item));
			}

			// TODO : Sort & Filter
		}

		public async Task AddNew(LoginViewItem viewItem, DialogBase dialog)
		{
			if (viewItem == null) throw new ArgumentNullException("viewItem");
			if (dialog == null) throw new ArgumentNullException("dialog");

			// Apply validations about required fields, ranges & values
			var validationResult = this.Manager.Validate(viewItem.Login);
			//if (validationResult == ValidationResult.Success)
			//{
			//	// Apply business logic
			//	var status = this.Manager.CanAddNew(viewItem.Login).Status;
			//	switch (status)
			//	{
			//		case PermissionStatus.Allow:
			//			// Add the log message
			//			this.Add(viewItem);
			//			break;
			//		case PermissionStatus.Confirm:
			//			// Confirm any user warnings
			//			dialog.Message = this.Manager.Settings.ConfirmAddLongMessageMsg;
			//			dialog.AcceptAction = () => this.Add(viewItem);
			//			await dialog.ShowAsync();
			//			break;
			//		default:
			//			throw new ArgumentOutOfRangeException();
			//	}
			//}
		}

		public async Task MarkHandled(LoginViewItem viewItem, DialogBase dialog)
		{
			if (viewItem == null) throw new ArgumentNullException("viewItem");
			if (dialog == null) throw new ArgumentNullException("dialog");

			// Check if the item isn't already handled
			//var isAlreadyHandled = viewItem.IsHandled;
			//if (isAlreadyHandled)
			//{
			//	await dialog.DisplayAsync(this.Manager.Settings.ItemAlreadyHandledMsg);
			//	return;
			//}

			//// Check if we have other message which is critical and it isn't handled
			//var hasUnhandledCriticalMessage = this.Manager.HasUnhandledCriticalMessage(this.ViewItems.Select(v => v.Login), viewItem.Login);
			//if (hasUnhandledCriticalMessage)
			//{
			//	await dialog.DisplayAsync(this.Manager.Settings.HasOtherUnhandledCriticalMessage);
			//	return;
			//}

			//// Request confirmation for marking the item as handled
			//dialog.Message = this.Manager.Settings.ConfirmMarkHandledMsg;
			//dialog.AcceptAction = () =>
			//					  {
			//						  // Mark the viewItem as Handled
			//						  viewItem.IsHandled = true;

			//						  // Call the manager to update the message
			//						  this.Manager.MarkHandled(viewItem.Login);
			//					  };
			//await dialog.ShowAsync();

		}

		private void Add(LoginViewItem viewItem)
		{
			// Call the manager to add the new message
			if (this.Manager.AddNew(viewItem.Login))
			{
				// Add the item to the list to the right place if sorter != null
				this.ViewItems.Add(viewItem);
			}
		}
	}

	public sealed class LoginManager
	{
		private readonly List<Login> _logins = new List<Login>();

		public LoginAdapter Adapter { get; private set; }
		public LoginSettings Settings { get; private set; }

		public List<Login> Logins
		{
			get { return _logins; }
		}

		public LoginManager(LoginAdapter adapter, LoginSettings settings)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");
			if (settings == null) throw new ArgumentNullException("settings");

			this.Adapter = adapter;
			this.Settings = settings;
		}

		public void Load(DateTime date)
		{
			_logins.Clear();
			_logins.AddRange(this.Adapter.GetAll(date));
		}

		public ValidationResult[] Validate(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			return Validator.GetResults(new[]
			                            {
				                            Validator.ValidateNotEmpty(login.Username, this.Settings.UsernameRequiredMsg),
				                            Validator.ValidateNotEmpty(login.Password, this.Settings.PasswordRequiredMsg),
											Validator.ValidateMaxLength(login.Username, 16, this.Settings.UsernameTooLongMsg),
											Validator.ValidateMinLength(login.Password, 8, this.Settings.PasswordTooShortMsg),
				                            Validator.ValidateMaxLength(login.Password, 32, this.Settings.PasswordTooLongMsg),
			                            });
		}

		public PermissionResult CanAddNew(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			// Trim the username
			var username = (login.Username ?? string.Empty).Trim();

			// Check for system usernames collision
			foreach (var name in this.Settings.SystemUsernames)
			{
				if (name.Equals(username, StringComparison.OrdinalIgnoreCase))
				{
					return PermissionResult.Deny(this.Settings.UsernameIsReservedForInternalUseMsg);
				}
			}

			// Check for duplicate username
			foreach (var current in _logins)
			{
				if (current.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
				{
					return PermissionResult.Deny(this.Settings.UsernameAlreadyTakenMsg);
				}
			}

			var strenght = this.GetPasswordStrenght(login.Password);
			switch (strenght)
			{
				case PasswordStrenght.Weak:
					return PermissionResult.Deny(this.Settings.UsernameAlreadyTakenMsg);
				case PasswordStrenght.Fair:
					return PermissionResult.Confirm(this.Settings.UsernameAlreadyTakenMsg);
				case PasswordStrenght.Good:
				case PasswordStrenght.Strong:
					return PermissionResult.Allow;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private PasswordStrenght GetPasswordStrenght(string password)
		{
			throw new NotImplementedException();
		}

		public bool HasUnhandledCriticalMessage(IEnumerable<Login> messages, Login excludeMessage)
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
					return true;
				}
			}

			return false;
		}

		public bool IsUnhandledCriticalItem(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			//return !login.IsHandled && login.Username.IndexOf(@"Critical", StringComparison.OrdinalIgnoreCase) >= 0;
			return false;
		}

		public bool AddNew(Login login, bool confirmed = false)
		{
			if (login == null) throw new ArgumentNullException("login");

			//var validationResult = this.Validate(login);
			//if (validationResult == ValidationResult.Success)
			//{
			//	var permissionResult = this.CanAddNew(login);

			//	if (permissionResult.Status == PermissionStatus.Allow ||
			//		(permissionResult.Status == PermissionStatus.Confirm && confirmed))
			//	{
			//		// Add the item to the db
			//		this.Adapter.Insert(login);
			//	}
			//}

			return false;
		}

		public void MarkHandled(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			// Check if the item isn't already handled
			//if (login.IsHandled)
			//{
			//	return;
			//}
			//// Check if we have other message which is critical and it isn't handled
			//var hasUnhandledCriticalMessage = this.HasUnhandledCriticalMessage(_logins, login);
			//if (hasUnhandledCriticalMessage)
			//{
			//	return;
			//}

			//// Mask the message as handled
			//login.IsHandled = true;

			//// Update the message in the db
			//this.Adapter.Update(login);
		}
	}

	public enum PasswordStrenght
	{
		Weak,
		Fair,
		Good,
		Strong
	}

	public sealed class LoginViewItem : BindableBase
	{
		private readonly Login _login;

		public Login Login
		{
			get { return _login; }
		}

		public string Username
		{
			get { return this.Login.Username; }
			set
			{
				this.Login.Username = value;
				this.OnPropertyChanged();
			}
		}

		public string Password
		{
			get { return this.Login.Password; }
			set
			{
				this.Login.Password = value;
				this.OnPropertyChanged();
			}
		}

		public bool IsActive
		{
			get { return this.Login.IsActive; }
			set
			{
				this.Login.IsActive = value;
				this.OnPropertyChanged();
			}
		}

		public LoginViewItem(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			_login = login;
		}
	}

	public sealed class LoginSettings
	{
		public string UsernameRequiredMsg { get; set; }
		public string PasswordRequiredMsg { get; set; }
		public string[] SystemUsernames { get; private set; }
		public string UsernameIsReservedForInternalUseMsg { get; set; }
		public string UsernameAlreadyTakenMsg { get; set; }
		public string UsernameTooLongMsg { get; set; }
		public string PasswordTooShortMsg { get; set; }
		public string PasswordTooLongMsg { get; set; }

		public LoginSettings()
		{
			this.UsernameRequiredMsg = @"";
			this.PasswordRequiredMsg = @"";
			this.SystemUsernames = new[] { @"admin", @"administrator", @"system" };
		}
	}

	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		public List<Login> GetAll(DateTime date)
		{
			throw new NotImplementedException();
		}

		public void Insert(Login item)
		{
			throw new NotImplementedException();
		}

		public void Update(Login item)
		{
			throw new NotImplementedException();
		}

		public void Delete(Login item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class Login : IModifiableObject
	{
		public long Id { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool IsActive { get; set; }

		public Login()
		{
			this.Username = string.Empty;
			this.Password = string.Empty;
		}
	}
}