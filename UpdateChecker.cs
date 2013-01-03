using mgmtapplauncher2.Language;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace mgmtapplauncher2
{
	class UpdateChecker : BackgroundWorker
	{

		private MainWindow m_Parent;
		private string m_LatestVersion;

		public UpdateChecker(MainWindow p)
		{
			m_Parent = p;
			this.DoWork += GetLatestVersion;
			this.RunWorkerCompleted += Update;
		}

		private void GetLatestVersion(object sender, DoWorkEventArgs e)
		{
			try
			{
				m_LatestVersion = new WebClient().DownloadString("http://updates.kempniu.pl/mgmtapplauncher2/latest");
			}
			catch (WebException)
			{
				m_LatestVersion = "";
			}
		}

		private void Update(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{

				Version current = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
				Version latest = new Version(m_LatestVersion);

				if (current.CompareTo(latest) < 0 && m_Parent.AskToUpdate(latest.ToString()))
				{
					try
					{

						string tempinstaller = Path.GetTempFileName();
						string installer = Path.GetDirectoryName(tempinstaller) + "\\mgmtapplauncher2.msi";

						new WebClient().DownloadFile("http://updates.kempniu.pl/mgmtapplauncher2/mgmtapplauncher2.msi", tempinstaller);

						if (File.Exists(installer))
							File.Delete(installer);
						File.Move(tempinstaller, installer);

						Process updater = new Process();
						updater.StartInfo.FileName = "msiexec.exe";
						updater.StartInfo.Arguments = "/i \"" + installer + "\" REINSTALL=ALL REINSTALLMODE=vomus";
						updater.Start();

						m_Parent.Close();

					}
					catch (WebException)
					{
						m_Parent.DisplayErrorMessage(Strings.MessageUpdateDownloadFailed);
					}
				}

			}
			catch { }
		}

	}
}
