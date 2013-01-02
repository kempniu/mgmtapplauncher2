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

		private MainWindow parent;
		private string latestVersion;

		public UpdateChecker(MainWindow p)
		{
			parent = p;
			this.DoWork += GetLatestVersion;
			this.RunWorkerCompleted += Update;
		}

		private void GetLatestVersion(object sender, DoWorkEventArgs e)
		{
			try
			{
				latestVersion = new WebClient().DownloadString("http://updates.kempniu.pl/mgmtapplauncher2/latest");
			}
			catch (WebException)
			{
				latestVersion = "";
			}
		}

		private void Update(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{

				Version current = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
				Version latest = new Version(latestVersion);

				if (current.CompareTo(latest) < 0)
				{

					MessageBoxResult mbr = MessageBox.Show(
						String.Format(Strings.MessageUpdateAvailable, latest, App.GetName()),
						App.GetName(),
						MessageBoxButton.YesNo,
						MessageBoxImage.Question
					);

					if (mbr == MessageBoxResult.Yes)
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

							parent.Close();

						}
						catch (WebException)
						{
							MessageBox.Show(
								Strings.MessageUpdateDownloadFailed,
								App.GetName(),
								MessageBoxButton.OK,
								MessageBoxImage.Error
							);
						}
					}

				}

			}
			catch { }
		}

	}
}
