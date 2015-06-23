using System;

namespace Core.Validation
{
	public sealed class ValidationResult
	{
		public static readonly ValidationResult Success = new ValidationResult(string.Empty);

		public string ErrorMessage { get; private set; }

		public ValidationResult(string errorMessage)
		{
			if (errorMessage == null) throw new ArgumentNullException("errorMessage");

			this.ErrorMessage = errorMessage;
		}
	}

	public enum PermissionStatus
	{
		Allow,
		Deny,
		Confirm
	}

	public sealed class PermissionResult
	{
		public static readonly PermissionResult Allow = new PermissionResult(PermissionStatus.Allow, string.Empty);

		public PermissionStatus Status { get; private set; }
		public string Message { get; private set; }

		public PermissionResult(PermissionStatus status, string message)
		{
			this.Status = status;
			this.Message = message;
		}

		public static PermissionResult Deny(string message)
		{
			if (message == null) throw new ArgumentNullException("message");

			throw new NotImplementedException();
		}

		public static PermissionResult Confirm(string message)
		{
			throw new NotImplementedException();
		}
	}
}