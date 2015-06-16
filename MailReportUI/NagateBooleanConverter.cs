using System;
using System.Globalization;
using System.Windows.Data;

namespace MailReportUI
{
	public sealed class NagateBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return !(bool) value;
			}
			catch (Exception)
			{
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}