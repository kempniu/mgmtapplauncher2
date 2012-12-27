using mgmtapplauncher2.Language;
using System;
using System.Windows;

namespace mgmtapplauncher2
{
	public partial class ProtocolNamePopup : Window
	{

		public string name = null;

		public ProtocolNamePopup()
		{
			InitializeComponent();
		}

		private void BNameOK_Click(object sender, RoutedEventArgs e)
		{
			name = TBProtocolName.Text;
			if (name.Length == 0)
				MessageBox.Show(
					Strings.MessageEmptyProtocolName,
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);
			else
				DialogResult = true;
		}

		private void BNameCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

	}
}
