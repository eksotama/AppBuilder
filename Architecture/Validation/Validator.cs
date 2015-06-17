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

		public static ValidationResult ValidateLength(string username, int max)
		{
			throw new NotImplementedException();
		}

		public static ValidationResult ValidateLength(string input, int min, int max)
		{
			throw new NotImplementedException();
		}
	}
}