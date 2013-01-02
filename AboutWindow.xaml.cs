using System.Windows;

namespace mgmtapplauncher2
{
	public partial class AboutWindow : Window
	{

		public AboutWindow()
		{
			InitializeComponent();
			LBuild.Content = "Build: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			LGit.Content = "GIT: " + new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("mgmtapplauncher2.Resources.GitCommit.txt")).ReadToEnd().Trim();
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
