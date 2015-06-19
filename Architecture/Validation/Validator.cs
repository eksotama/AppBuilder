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

		public static ValidationResult[] GetResults(ValidationResult[] results)
		{
			if (results == null) throw new ArgumentNullException("results");
			if (results.Length == 0) throw new ArgumentOutOfRangeException("results");

			throw new NotImplementedException();
		}

		public static ValidationResult ValidateMinLength(string value, int minLength, string message)
		{
			if (value == null) throw new ArgumentNullException("value");

			if (value.Length < minLength)
			{
				return new ValidationResult(message);
			}
			return ValidationResult.Success;
		}

		public static ValidationResult ValidateMaxLength(string value, int maxLength, string message)
		{
			if (value.Length > maxLength)
			{
				return new ValidationResult(message);
			}
			return ValidationResult.Success;
		}

		public static ValidationResult ValidateLength(string value, int min, int max, string message)
		{
			var length = value.Length;
			if (min <= length && length <= max)
			{
				return ValidationResult.Success;
			}
			return new ValidationResult(message);
		}
	}
}