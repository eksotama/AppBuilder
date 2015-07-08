using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Core.Dialog;
using Core.Objects;
using Core.Validation;

namespace Core
{
	public sealed class LogMessageViewManager : ViewObject
	{
		public LoginManager Manager { get; private set; }
		public DialogBase Dialog { get; private set; }

		private string _usernameValidationMsg = string.Empty;
		public string UsernameValidationMsg
		{
			get { return _usernameValidationMsg; }
			set { this.SetField(ref _usernameValidationMsg, value); }
		}

		private string _passwordValidationMsg = string.Empty;
		public string PasswordValidationMsg
		{
			get { return _passwordValidationMsg; }
			set { this.SetField(ref _passwordValidationMsg, value); }
		}

		public string UsernameCaption { get { return this.Manager.Settings.UsernameCaption; } }
		public string PasswordCaption { get { return this.Manager.Settings.PasswordCaption; } }
		public string LoginCaption { get { return this.Manager.Settings.LoginCaption; } }

		private string _strength;
		public string Strength
		{
			get { return _strength; }
			set { this.SetField(ref _strength, value); }
		}

		private Color _passwordColor;
		public Color PasswordColor
		{
			get { return _passwordColor; }
			set { this.SetField(ref _passwordColor, value); }
		}

		private double _passwordValue;
		public double PasswordValue
		{
			get { return _passwordValue; }
			set { this.SetField(ref _passwordValue, value); }
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
			this.AddCommand = new DelegateCommand<LoginViewItem>(this.Add);
			this.ViewItems = new ObservableCollection<LoginViewItem>();
			this.ViewItem = new LoginViewItem(new Login());
			this.ViewItem.PropertyChanged += (sender, arg) =>
											 {
												 var color = Colors.Black;
												 var value = 0D;

												 var password = this.ViewItem.Password ?? string.Empty;
												 if (password != string.Empty)
												 {
													 value = this.Manager.GetPasswordStrengthValue(password);
													 color = this.Manager.GetPasswordColor(value);
												 }

												 var strength = string.Empty;
												 if (value > 0)
												 {
													 strength = value.ToString(@"F2");
												 }
												 this.Strength = strength;
												 this.PasswordColor = color;
												 this.PasswordValue = value;
											 };
		}

		private Color GetColorValue(Color color)
		{
			//var r = color.R;
			//var g = color.G;
			//var b = color.B;
			//var a = color.A;

			//return a << 24 + r << 16 + g << 8 + b;
			return color;
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

			viewItem = new LoginViewItem(new Login()) { Username = (viewItem.Username ?? string.Empty).Trim(), Password = viewItem.Password };

			try
			{
				var login = viewItem.Login;
				// Display validation to UI
				this.UsernameValidationMsg = Validator.CombineResults(this.Manager.ValidateUsername(login.Username));
				this.PasswordValidationMsg = Validator.CombineResults(this.Manager.ValidatePassword(login.Password));

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
							this.Dialog.AcceptAction = () => this.AddValidated(viewItem);
							await this.Dialog.ConfirmAsync(permissionResult.Message);
							break;
						case PermissionStatus.Deny:
							await this.Dialog.DisplayAsync(permissionResult.Message);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
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
					this.Dialog.AcceptAction = () =>
					{
						// Mark the viewItem as Inactive
						viewItem.IsActive = false;

						// Call the manager to update the message
						this.Manager.MarkInactive(viewItem.Login);
					};
					await this.Dialog.ShowAsync(this.Manager.Settings.ConfirmMarkInactiveMsg);
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

		public List<Login> Logins
		{
			get { return _logins; }
		}

		public LoginAdapter Adapter { get; private set; }
		public LoginSettings Settings { get; private set; }

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
					Validator.ValidateMinLength(password, 8, this.Settings.PasswordTooShortMsg),
					Validator.ValidateMaxLength(password, 32, this.Settings.PasswordTooLongMsg),
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
			if (password == null) throw new ArgumentNullException("password");

			return PasswordValidator.GetPasswordStrength(password);
		}

		public double GetPasswordStrengthValue(string password)
		{
			if (password == null) throw new ArgumentNullException("password");

			return PasswordValidator.GetPasswordStrengthValue(password);
		}

		public Color GetPasswordColor(double value)
		{
			return PasswordValidator.GetPasswordColor(value);
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
			// Check in the database ?!?? Do we need the field ?!
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

					// Add the item to the list
					_logins.Add(login);

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
		public static readonly Color[] PasswordColors =
		{
			Colors.Red, // Red,
			Colors.OrangeRed, // RedOrange
			Colors.Orange, // Orange,
			Colors.Yellow, // Yellow
			Colors.YellowGreen, // YellowGreen
			Colors.Green, // Green
			Colors.DarkGreen, // DarkGreen
		};

		public static PasswordStrength GetPasswordStrength(string password)
		{
			if (password == null) throw new ArgumentNullException("password");

			if (password.Length < 10)
			{
				return PasswordStrength.Weak;
			}
			if (password.Length < 15)
			{
				return PasswordStrength.Medium;
			}
			return PasswordStrength.Good;
		}

		public static double GetPasswordStrengthValue(string password)
		{
			var value = 0;

			if (password.Any(char.IsDigit))
			{
				value++;
			}
			if (password.Any(char.IsLower))
			{
				value++;
			}
			if (password.Any(char.IsUpper))
			{
				value++;
			}
			if (password.Any(char.IsSymbol))
			{
				value++;
			}
			if (password.Any(char.IsWhiteSpace))
			{
				value++;
			}
			if (password.Length > 12)
			{
				value++;
			}

			return ((double)value / PasswordColors.Length) * 100.0;
		}

		public static Color GetPasswordColor(double value)
		{
			return PasswordColors[((int)((value / 100.0) * PasswordColors.Length))];
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
		public string PasswordTooWeakMsg { get; set; }
		public string ConfirmPasswordMediumStrengthMsg { get; set; }
		public string LoginIsAlreadyInactiveMsg { get; set; }
		public string LoginIsSystemMsg { get; set; }
		public string ConfirmMarkInactiveMsg { get; set; }
		public string UsernameCaption { get; set; }
		public string PasswordCaption { get; set; }
		public string LoginCaption { get; set; }
		public string PasswordTooShortMsg { get; set; }
		public string PasswordTooLongMsg { get; set; }

		public LoginSettings()
		{
			this.UsernameRequiredMsg = @"Username is required";
			this.PasswordRequiredMsg = @"Password is required";
			this.SystemUsernames = new[] { @"admin", @"administrator", @"system" };
			this.UsernameCaption = @"Username";
			this.PasswordCaption = @"Password";
			this.PasswordCaption = @"Login";
			this.PasswordTooShortMsg = @"Password is too short";
			this.PasswordTooLongMsg = @"Password is too long";
			this.PasswordTooWeakMsg = @"Password is too weak";
			this.ConfirmPasswordMediumStrengthMsg = @"Password isn't strong enought. Do you want to continue with this password ?";
			this.UsernameAlreadyTakenMsg = @"This username is already taken. Please choose another one.";
		}
	}

	public sealed class LoginAdapter
	{
		public List<Login> GetAll(DateTime date)
		{
			throw new NotImplementedException();
		}

		public void Insert(Login item)
		{
			Console.WriteLine(@"Insert data into db.");
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