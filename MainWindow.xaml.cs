using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace mgmtapplauncher2
{
	public partial class MainWindow : Window
	{

		// Property bound to combobox
		public ObservableCollection<Protocol> Protocols { get; set; }

		private string GetKeyValue(string line, string key)
		{
			Regex r = new Regex("^" + key + "=(.*)$");
			Match m = r.Match(line);
			return m.Groups[1].Value;
		}

		private void SaveRegistryKeyForProtocol(Protocol protocol)
		{

			try
			{
				Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + protocol.Name);
			}
			catch (ArgumentException) { }

			if (protocol.Handled)
			{
				// See: http://msdn.microsoft.com/en-us/library/aa767914%28v=vs.85%29.aspx
				RegistryKey rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + protocol.Name);
				rk.SetValue("", "URL:" + protocol.Name + " Protocol", RegistryValueKind.String);
				rk.SetValue("URL Protocol", "", RegistryValueKind.String);
				rk = rk.CreateSubKey("shell\\open\\command");
				rk.SetValue("", "\"" + Application.ResourceAssembly.Location + "\" \"%1\"");
			}

		}

		public MainWindow()
		{

			Protocols = new ObservableCollection<Protocol>();
			DataContext = this;

			bool defaultInit = false;
			string[] config;
			Protocol p = null;

			if (File.Exists(App.GetConfigFile()) == false)
			{
				MessageBoxResult mbr = MessageBox.Show(
					"Nie znaleziono pliku konfiguracyjnego. Czy chcesz skorzystać z domyślnych ustawień?",
					App.GetName(),
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
				);
				if (mbr == MessageBoxResult.Yes)
				{
					defaultInit = true;
					// Read whole text file resource, save as configuration file
					File.WriteAllText(
						App.GetConfigFile(),
						new StreamReader(
							System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("mgmtapplauncher2.Resources.DefaultConfig.txt")
						).ReadToEnd()
					);
				}
			}

			try
			{
				config = File.ReadAllLines(App.GetConfigFile());
			}
			catch (FileNotFoundException)
			{
				config = new string[0];
			}

			foreach (string line in config)
			{
				if (Regex.IsMatch(line, "^\\[[a-z]+\\]$"))
				{
					p = new Protocol() { Name = line.Substring(1, line.Length - 2) };
					if (defaultInit)
					{
						p.Handled = true;
						SaveRegistryKeyForProtocol(p);
					}
					Protocols.Add(p);
				}
				else if (Regex.IsMatch(line, "^app="))
				{
					if (p == null)
						throw new System.NullReferenceException();
					p.App = GetKeyValue(line, "app");

				}
				else if (Regex.IsMatch(line, "^args="))
				{
					if (p == null)
						throw new System.NullReferenceException();
					p.Args = GetKeyValue(line, "args");
				}
			}

			if (!defaultInit)
			{
				foreach (Protocol protocol in Protocols)
				{
					try
					{
						RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + protocol.Name);
						if (rk.GetValue("").ToString() == "URL:" + protocol.Name + " Protocol")
						{
							if (rk.GetValue("URL Protocol").ToString() == "")
							{
								if (rk.OpenSubKey("shell\\open\\command").GetValue("").ToString().IndexOf(Application.ResourceAssembly.Location) != -1)
								{
									protocol.Handled = true;
								}
							}
						}
					}
					catch (NullReferenceException)
					{
						protocol.Handled = false;
					}
				}
			}

			if (App.args.Length == 0)
			{
				InitializeComponent();
			}
			else
			{

				string uri = App.args[0];
				int pos1;
				int pos2;
				string protocol;
				string host;
				int i = 0;
				bool found = false;

				try
				{
					pos1 = uri.IndexOf(':');
					if (pos1 == -1 || uri.Substring(pos1, 3) != "://")
						throw new ArgumentException();
					protocol = uri.Substring(0, pos1);
					pos1 += 3;
					pos2 = uri.IndexOf('/', pos1);
					if (pos2 == -1)
						pos2 = uri.Length;
					host = uri.Substring(pos1, pos2 - pos1);
				}
				catch
				{
					protocol = "";
					host = "";
					MessageBox.Show(
						"Nieprawidłowy format URI!",
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Error
					);
				}

				while (!found && i < Protocols.Count)
				{
					if (Protocols[i++].Name == protocol)
						found = true;
				}

				if (!found)
				{
					if (protocol.Length > 0)
						MessageBox.Show(
							String.Format("Protokół \"{0}\" nie jest obsługiwany!", protocol),
							App.GetName(),
							MessageBoxButton.OK,
							MessageBoxImage.Error
						);
				}
				else
				{
					try
					{
						p = Protocols[i - 1];
						Process process = new Process();
						process.StartInfo.FileName = p.App;
						process.StartInfo.Arguments = p.Args.Replace("%P%", protocol).Replace("%H%", host);
						process.Start();
					}
					catch
					{
						MessageBox.Show(
							String.Format("Nie udało się uruchomić programu \"{0}\"!", p.App),
							App.GetName(),
							MessageBoxButton.OK,
							MessageBoxImage.Error
						);
					}
				}

				this.Close();

			}

		}

		private void BBrowse_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
			ofd.Filter = "Pliki wykonywalne|*.exe";
			Nullable<bool> result = ofd.ShowDialog();
			if (result == true)
				((Protocol)CBProtocol.SelectedItem).App = ofd.FileName;
		}

		private void BSave_Click(object sender, RoutedEventArgs e)
		{

			string[] config = new string[Protocols.Count * 4];
			int i = 0;

			var sortedProtocols = from p in Protocols orderby p.Name select p;

			foreach (var protocol in sortedProtocols)
			{
				// Do a pre-save sanity check
				if (protocol.App == null)
				{
					CBProtocol.SelectedItem = protocol;
					MessageBox.Show(
						"Protokół \"" + protocol.Name + "\" nie ma przypisanej żadnej aplikacji!",
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Warning
					);
					return;
				}
			}

			foreach (var protocol in sortedProtocols)
			{

				config[i++] = "[" + protocol.Name + "]";
				config[i++] = "app=" + protocol.App;
				config[i++] = "args=" + protocol.Args;
				config[i++] = "";

				SaveRegistryKeyForProtocol(protocol);

			}

			File.WriteAllLines(App.GetConfigFile(), config);

			MessageBox.Show(
				"Ustawienia zostały zapisane!",
				App.GetName(),
				MessageBoxButton.OK,
				MessageBoxImage.Information
			);

		}

		private void BAdd_Click(object sender, RoutedEventArgs e)
		{
			ProtocolNamePopup pnp = new ProtocolNamePopup();
			if (pnp.ShowDialog() == true)
			{
				Protocols.Add(new Protocol() { Name = pnp.name.ToLower(), Handled = true });
				CBProtocol.SelectedIndex = CBProtocol.Items.Count - 1;
			}
		}

		private void BDelete_Click(object sender, RoutedEventArgs e)
		{
			int selected = CBProtocol.SelectedIndex;
			try
			{
				Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + ((Protocol)CBProtocol.SelectedItem).Name);
			}
			catch (ArgumentException) { }
			Protocols.Remove((Protocol)CBProtocol.SelectedItem);
			if (selected > -1)
				CBProtocol.SelectedIndex = selected - 1;
		}

		private void BQuit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

	}
}
