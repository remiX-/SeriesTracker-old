using System;
using System.Windows;
using System.Windows.Data;

namespace SeriesTracker.Utilities.Converters
{
	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class VisibilityToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (Visibility)value == Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((bool)value) return Visibility.Visible;
			else return Visibility.Collapsed;
		}
	}
}
