using System;
using System.Windows;

namespace mgmtapplauncher2
{
	public partial class App : Application
	{

		private static string[] m_Args;

		public static string GetName()
		{
			return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
		}

		public static string GetConfigFile()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\mgmtapplauncher2.xml";
		}

		public static string[] GetArgs()
		{
			return m_Args;
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			m_Args = e.Args;
		}

	}
}
