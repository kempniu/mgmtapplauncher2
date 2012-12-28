using System;
using System.Windows;

namespace mgmtapplauncher2
{
	public partial class App : Application
	{

		public static string[] args;

		public static string GetName()
		{
			return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
		}

		public static string GetConfigFile()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\mgmtapplauncher2.xml";
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			args = e.Args;
		}

	}
}
