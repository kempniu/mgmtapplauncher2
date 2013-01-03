using mgmtapplauncher2.Language;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace mgmtapplauncher2
{

	class Configuration
	{

		public ObservableCollection<Protocol> Protocols { get; set; }

		public Configuration()
		{

			Protocols = new ObservableCollection<Protocol>();
			bool defaultInit = false;

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

		}

		public Protocol GetProtocol(string name)
		{

			int i = 0;
			bool found = false;

			while (!found && i < Protocols.Count)
			{
				if (Protocols[i++].Name == name)
					found = true;
			}

			if (found)
				return Protocols[i - 1];
			else
				return null;

		}

		public void AddProtocol(string name, bool handled)
		{
			Protocols.Add(new Protocol() { Name = name, Handled = handled });
		}

		public void DeleteProtocol(Protocol protocol)
		{
			try
			{
				Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + protocol.Name);
			}
			catch (ArgumentException) { }
			Protocols.Remove(protocol);
		}

		public void SetProtocolApp(Protocol protocol, string app)
		{
			protocol.App = app;
		}

		public void Save()
		{

			var sortedProtocols = from p in Protocols orderby p.Name select p;
			ObservableCollection<Protocol> sortedProtocolsCollection = new ObservableCollection<Protocol>();

			foreach (var protocol in sortedProtocols)
			{
				// Do a pre-save sanity check
				if (protocol.App == null)
				{
					throw new NoAppProtocolException(protocol);
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

	}

	public class NoAppProtocolException : Exception
	{
		public Protocol m_protocol;
		public NoAppProtocolException(Protocol protocol)
		{
			m_protocol = protocol;
		}
	}

}
