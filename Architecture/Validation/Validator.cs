using System;

namespace Architecture.Validation
{
	public static class Validator
	{
		public static ValidationResult ValidateNotNull(object value, string message)
		{
			if (message == null) throw new ArgumentNullException("message");

			return value == null ? new ValidationResult(message) : ValidationResult.Success;
		}

		public static ValidationResult ValidateNotEmpty(string value, string message)
		{
			return string.IsNullOrWhiteSpace(value) ? new ValidationResult(message) : ValidationResult.Success;
		}
	}
}