using mgmtapplauncher2.Language;
using System;
using System.Windows;

namespace mgmtapplauncher2
{
	public partial class MainWindow : Window
	{

		private Configuration m_Configuration;

		public MainWindow()
		{

			m_Configuration = new Configuration();
			DataContext = m_Configuration;

			if (App.GetArgs().Length == 0)
			{
				InitializeComponent();
				new UpdateChecker(this).RunWorkerAsync();
			}
			else
			{
				try
				{
					new UriHandler(m_Configuration, App.GetArgs()[0]).Handle();
				}
				catch (InvalidUriException)
				{
					MessageBox.Show(
						Strings.MessageIncorrectURIFormat,
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Error
					);
				}
				catch (ProtocolNotSupportedException e)
				{
					MessageBox.Show(
						String.Format(Strings.MessageProtocolNotSupported, e.protocol),
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Error
					);
				}
				catch (ProgramStartFailedException e)
				{
					MessageBox.Show(
						String.Format(Strings.MessageFailedToStartProgram, e.program),
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Error
					);
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
				m_Configuration.SetProtocolApp((Protocol)CBProtocol.SelectedItem, ofd.FileName);
		}

		private void BSave_Click(object sender, RoutedEventArgs e)
		{
			m_Configuration.Save();
		}

		private void BAdd_Click(object sender, RoutedEventArgs e)
		{
			ProtocolNamePopup pnp = new ProtocolNamePopup();
			if (pnp.ShowDialog() == true)
			{
				m_Configuration.AddProtocol(pnp.GetName().ToLower(), true);
				CBProtocol.SelectedIndex = CBProtocol.Items.Count - 1;
			}
		}

		private void BDelete_Click(object sender, RoutedEventArgs e)
		{
			int selected = CBProtocol.SelectedIndex;
			if (selected > -1)
			{
				m_Configuration.DeleteProtocol((Protocol)CBProtocol.SelectedItem);
				CBProtocol.SelectedIndex = selected - 1;
			}
		}

		private void BQuit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void MIAbout_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow().ShowDialog();
		}

	}
}
