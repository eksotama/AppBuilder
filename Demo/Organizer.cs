using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core;
using Core.Data;
using Core.Dialog;
using Core.Objects;
using Core.Validation;

namespace Demo
{
	public sealed class LogMessageViewManager : ViewObject
	{
		public LoginManager Manager { get; private set; }
		public DialogBase Dialog { get; private set; }

		private string _usernameValidationHint = string.Empty;
		public string UsernameValidationHint
		{
			get { return _usernameValidationHint; }
			set { this.SetField(ref _usernameValidationHint, value); }
		}

		private string _passwordValidationHint = string.Empty;
		public string PasswordValidationHint
		{
			get { return _passwordValidationHint; }
			set { this.SetField(ref _passwordValidationHint, value); }
		}

		public LoginViewItem ViewItem { get; private set; }
		public ObservableCollection<LoginViewItem> ViewItems { get; private set; }

		public ICommand AddCommand { get; private set; }

		public LogMessageViewManager(LoginManager manager, DialogBase dialog)
		{
			if (manager == null) throw new ArgumentNullException("manager");
			if (dialog == null) throw new ArgumentNullException("dialog");

			this.Manager = manager;
			this.Dialog = dialog;
			this.ViewItem = new LoginViewItem(new Login());
			this.ViewItems = new ObservableCollection<LoginViewItem>();
			this.AddCommand = new DelegateCommand<LoginViewItem>(this.Add, this.CanExecuteAdd);
		}

		private bool CanExecuteAdd(LoginViewItem viewItem)
		{
			return !string.IsNullOrWhiteSpace(viewItem.Username) &&
				   !string.IsNullOrWhiteSpace(viewItem.Password);
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

		public async Task Add(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException("viewItem");

			var login = viewItem.Login;
			// Display validation to UI
			this.UsernameValidationHint = Validator.CombineResults(this.Manager.ValidateUsername(login.Username));
			this.PasswordValidationHint = Validator.CombineResults(this.Manager.ValidatePassword(login.Password));

			// Apply validations about required fields, ranges & values			
			var validationResults = this.Manager.Validate(login);
			if (validationResults.Length == 0)
			{
				// Apply business logic
				var permissionResult = this.Manager.CanAdd(login);
				switch (permissionResult.Status)
				{
					case PermissionStatus.Allow:
						this.AddValidated(viewItem);
						break;
					case PermissionStatus.Confirm:
						// Confirm any user warnings
						this.Dialog.Message = permissionResult.Message;
						this.Dialog.AcceptAction = () => this.AddValidated(viewItem);
						await this.Dialog.ShowAsync();
						break;
					case PermissionStatus.Deny:
						await this.Dialog.DisplayAsync(permissionResult.Message);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public async Task MarkInactive(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException("viewItem");

			// Apply business logic
			var permissionResult = this.Manager.CanMarkInactive(viewItem.Login);
			switch (permissionResult.Status)
			{
				case PermissionStatus.Allow:
					// Request confirmation for marking the item as inactive
					this.Dialog.Message = this.Manager.Settings.ConfirmMarkInactiveMsg;
					this.Dialog.AcceptAction = () =>
					{
						// Mark the viewItem as Inactive
						viewItem.IsActive = false;

						// Call the manager to update the message
						this.Manager.MarkInactive(viewItem.Login);
					};
					await this.Dialog.ShowAsync();
					break;
				case PermissionStatus.Deny:
					await this.Dialog.DisplayAsync(permissionResult.Message);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void AddValidated(LoginViewItem viewItem)
		{
			// Call the manager to add the new message
			if (this.Manager.Add(viewItem.Login, true))
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

		public ValidationResult[] ValidateUsername(string username)
		{
			if (username == null) throw new ArgumentNullException("username");

			return Validator.GetResults(
				new[]
				{
					Validator.ValidateNotEmpty(username, this.Settings.UsernameRequiredMsg),
					Validator.ValidateMaxLength(username, 16, this.Settings.UsernameTooLongMsg),
				});
		}

		public ValidationResult[] ValidatePassword(string password)
		{
			if (password == null) throw new ArgumentNullException("password");

			return Validator.GetResults(
				new[]
				{
					Validator.ValidateNotEmpty(password, this.Settings.PasswordRequiredMsg),
					Validator.ValidateLength(password, 8, 32, this.Settings.PasswordTooShortOrTooLongMsg),
				});
		}

		public ValidationResult[] Validate(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			return this.ValidateUsername(login.Username)
				.Concat(this.ValidatePassword(login.Password))
				.ToArray();
		}

		public PasswordStrength GetPasswordStrength(string password)
		{
			return PasswordValidator.GetPasswordStrength(password);
		}

		public PermissionResult CanAdd(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			// Trim the username
			var username = (login.Username ?? string.Empty).Trim();

			if (this.IsSystem(login))
			{
				return PermissionResult.Deny(this.Settings.UsernameIsReservedForInternalUseMsg);
			}

			// Check for duplicate username
			foreach (var current in _logins)
			{
				if (current.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
				{
					return PermissionResult.Deny(this.Settings.UsernameAlreadyTakenMsg);
				}
			}

			// Check password strength
			var strength = GetPasswordStrength(login.Password);
			switch (strength)
			{
				case PasswordStrength.Weak:
					return PermissionResult.Deny(this.Settings.PasswordTooWeakMsg);
				case PasswordStrength.Medium:
					return PermissionResult.Confirm(this.Settings.ConfirmPasswordMediumStrengthMsg);
				case PasswordStrength.Good:
				case PasswordStrength.Strong:
					return PermissionResult.Allow;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public PermissionResult CanMarkInactive(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			// Check if the item isn't already inactive
			var isInactive = !login.IsActive;
			if (isInactive)
			{
				return PermissionResult.Deny(this.Settings.LoginIsAlreadyInactiveMsg);
			}

			// Check if we have other message which is critical and it isn't handled
			var isSystem = this.IsSystem(login);
			if (isSystem)
			{
				return PermissionResult.Deny(this.Settings.LoginIsSystemMsg);
			}

			return PermissionResult.Allow;
		}

		public bool Add(Login login, bool confirmed = false)
		{
			if (login == null) throw new ArgumentNullException("login");

			var validationResults = this.Validate(login);
			if (validationResults.Length == 0)
			{
				var permissionResult = this.CanAdd(login);
				if (permissionResult.Status == PermissionStatus.Allow ||
					(permissionResult.Status == PermissionStatus.Confirm && confirmed))
				{
					// Add the item to the db
					this.Adapter.Insert(login);

					return true;
				}
			}

			return false;
		}

		public bool MarkInactive(Login login)
		{
			if (login == null) throw new ArgumentNullException("login");

			// Check if the item isn't already handled
			var permissionResult = this.CanMarkInactive(login);
			if (permissionResult.Status == PermissionStatus.Allow)
			{
				// Mask the message as Inactive
				login.IsActive = false;

				// Update the message in the db
				this.Adapter.Update(login);
			}

			return false;
		}

		private bool IsSystem(Login login)
		{
			var username = (login.Username ?? string.Empty).Trim();

			// Check for system usernames collision
			foreach (var name in this.Settings.SystemUsernames)
			{
				if (name.Equals(username, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static class PasswordValidator
	{
		public static PasswordStrength GetPasswordStrength(string password)
		{
			throw new NotImplementedException();
		}
	}

	public enum PasswordStrength
	{
		Weak,
		Medium,
		Good,
		Strong
	}

	public sealed class LoginViewItem : ViewObject
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
				this.Login.Username = (value ?? string.Empty).Trim();
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
		public string PasswordTooShortOrTooLongMsg { get; set; }
		public string PasswordTooWeakMsg { get; set; }
		public string ConfirmPasswordMediumStrengthMsg { get; set; }
		public string LoginIsAlreadyInactiveMsg { get; set; }
		public string LoginIsSystemMsg { get; set; }
		public string ConfirmMarkInactiveMsg { get; set; }

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