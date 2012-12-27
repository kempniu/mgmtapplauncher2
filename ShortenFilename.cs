using System;
using System.Windows.Data;

namespace mgmtapplauncher2
{
	class ShortenFilename : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string f = (string)value;
			if (f != null && f.Length > 50)
				f = f.Substring(0, 24) + "..." + f.Substring(f.Length - 24);
			return f;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}
}
