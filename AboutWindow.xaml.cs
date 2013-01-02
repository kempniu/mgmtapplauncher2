using System.Windows;

namespace mgmtapplauncher2
{
	public partial class AboutWindow : Window
	{

		public AboutWindow()
		{
			InitializeComponent();
		}

		private void BOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void VisitHomepage(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.ToString());
		}

	}
}
