using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace mgmtapplauncher2
{
	public partial class AboutWindow : Window
	{

		public AboutWindow()
		{
			InitializeComponent();
			LBuild.Content = "Build: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
			LGit.Content = "GIT: " + new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("mgmtapplauncher2.Resources.GitCommit.txt")).ReadToEnd().Trim();
		}

		private void BOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void VisitHomepage(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.ToString());
		}

	}
}
