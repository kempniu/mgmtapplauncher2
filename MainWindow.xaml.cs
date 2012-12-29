using mgmtapplauncher2.Language;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace mgmtapplauncher2
{
	public partial class MainWindow : Window
	{

		// Property bound to combobox
		public ObservableCollection<Protocol> Protocols { get; set; }

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
			Protocol p = null;

			if (File.Exists(App.GetConfigFile()) == false)
			{
				MessageBoxResult mbr = MessageBox.Show(
					Strings.MessageConfigNotFound,
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
							System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("mgmtapplauncher2.Resources.DefaultConfig.xml")
						).ReadToEnd()
					);
				}
			}

			try
			{
				XmlRootAttribute xra = new XmlRootAttribute("Protocols");
				XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Protocol>), xra);
				StreamReader tr = new StreamReader(App.GetConfigFile());
				Protocols = (ObservableCollection<Protocol>)xs.Deserialize(tr);
				tr.Close();
			}
			catch (FileNotFoundException)
			{
				// Assume the user will configure everything from scratch
			}
			catch (InvalidOperationException)
			{
				MessageBox.Show(
					String.Format(Strings.MessageConfigCorrupt, App.GetConfigFile()),
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}

			if (defaultInit)
			{
				foreach (Protocol protocol in Protocols)
				{
					protocol.Handled = true;
					SaveRegistryKeyForProtocol(protocol);
				}
			}
			else
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
						Strings.MessageIncorrectURIFormat,
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
							String.Format(Strings.MessageProtocolNotSupported, protocol),
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
							String.Format(Strings.MessageFailedToStartProgram, p.App),
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
			ofd.Filter = Strings.FilterExecutableFiles;
			Nullable<bool> result = ofd.ShowDialog();
			if (result == true)
				((Protocol)CBProtocol.SelectedItem).App = ofd.FileName;
		}

		private void BSave_Click(object sender, RoutedEventArgs e)
		{

			var sortedProtocols = from p in Protocols orderby p.Name select p;
			ObservableCollection<Protocol> sortedProtocolsCollection = new ObservableCollection<Protocol>();

			foreach (var protocol in sortedProtocols)
			{
				// Do a pre-save sanity check
				if (protocol.App == null)
				{
					CBProtocol.SelectedItem = protocol;
					MessageBox.Show(
						String.Format(Strings.MessageNoProtocolApp, protocol.Name),
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Warning
					);
					return;
				}
				else
				{
					sortedProtocolsCollection.Add(protocol);
					SaveRegistryKeyForProtocol(protocol);
				}
			}

			try
			{

				XmlRootAttribute xra = new XmlRootAttribute("Protocols");
				XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Protocol>), xra);
				TextWriter tw = new StreamWriter(App.GetConfigFile());
				xs.Serialize(tw, sortedProtocolsCollection);
				tw.Close();

				MessageBox.Show(
					Strings.MessageSettingsSaved,
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Information
				);

			}
			catch
			{
				MessageBox.Show(
					String.Format(Strings.MessageSettingsNotSaved, App.GetConfigFile()),
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}

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
			if (selected > -1)
			{
				try
				{
					Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + ((Protocol)CBProtocol.SelectedItem).Name);
				}
				catch (ArgumentException) { }
				Protocols.Remove((Protocol)CBProtocol.SelectedItem);
				CBProtocol.SelectedIndex = selected - 1;
			}
		}

		private void BQuit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

	}
}
