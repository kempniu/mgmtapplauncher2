using mgmtapplauncher2.Language;
using System;
using System.Windows;

namespace mgmtapplauncher2
{
	public partial class ProtocolNamePopup : Window
	{

		private string m_Name = null;

		public ProtocolNamePopup()
		{
			InitializeComponent();
		}

		public string GetName()
		{
			return m_Name;
		}

		private void BNameOK_Click(object sender, RoutedEventArgs e)
		{
			m_Name = TBProtocolName.Text;
			if (m_Name.Length == 0)
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
