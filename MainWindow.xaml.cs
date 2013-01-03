﻿using mgmtapplauncher2.Language;
using System;
using System.Windows;
using System.Windows.Input;

namespace mgmtapplauncher2
{
	public partial class MainWindow : Window
	{

		private Configuration m_Configuration;

		public MainWindow()
		{

			bool initializeWithDefaults = false;
			bool loadConfigurationFile = true;

			m_Configuration = new Configuration();

			if (!m_Configuration.ConfigFileExists())
			{
				MessageBoxResult mbr = MessageBox.Show(
					Strings.MessageConfigNotFound,
					App.GetName(),
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
				);
				if (mbr == MessageBoxResult.Yes)
					initializeWithDefaults = true;
				else
					loadConfigurationFile = false;
			}

			try
			{
				m_Configuration.Initialize(initializeWithDefaults, loadConfigurationFile);
			}
			catch (InvalidOperationException)
			{
				MessageBox.Show(
					String.Format(Strings.MessageConfigCorrupt, m_Configuration.GetConfigFile()),
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
				m_Configuration.Initialize(initializeWithDefaults, false);
			}

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
				catch (ProtocolNotSupportedException exc)
				{
					MessageBox.Show(
						String.Format(Strings.MessageProtocolNotSupported, exc.m_Protocol),
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Error
					);
				}
				catch (ProgramStartFailedException exc)
				{
					MessageBox.Show(
						String.Format(Strings.MessageFailedToStartProgram, exc.m_Program),
						App.GetName(),
						MessageBoxButton.OK,
						MessageBoxImage.Error
					);
				}
				this.Close();
			}

		}

		public bool AskToUpdate(string latestVersion)
		{
			bool retval = false;
			MessageBoxResult mbr = MessageBox.Show(
				String.Format(Strings.MessageUpdateAvailable, latestVersion, App.GetName()),
				App.GetName(),
				MessageBoxButton.YesNo,
				MessageBoxImage.Question
			);
			if (mbr == MessageBoxResult.Yes)
				retval = true;
			return retval;
		}

		public void DisplayErrorMessage(string message)
		{
			MessageBox.Show(
				message,
				App.GetName(),
				MessageBoxButton.OK,
				MessageBoxImage.Error
			);
		}

		private void MIAbout_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow().ShowDialog();
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

		private void BBrowse_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
			ofd.Filter = Strings.FilterExecutableFiles;
			Nullable<bool> result = ofd.ShowDialog();
			if (result == true)
				m_Configuration.SetProtocolApp((Protocol)CBProtocol.SelectedItem, ofd.FileName);
		}

		private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = m_Configuration.IsConfigurationChanged;
		}

		private void SaveExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Save(false);
		}

		private void CloseExecute(object sender, ExecutedRoutedEventArgs e)
		{
			if (m_Configuration.IsConfigurationChanged)
			{
				MessageBoxResult mbr = MessageBox.Show(
					Strings.MessageConfigUnsaved,
					App.GetName(),
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question
				);
				if (mbr == MessageBoxResult.Yes)
					Save(true);
				else if (mbr == MessageBoxResult.No)
					this.Close();
			}
			else
			{
				this.Close();
			}
		}

		private void Save(bool close)
		{
			try
			{
				m_Configuration.Save();
				if (close)
					this.Close();
			}
			catch (NoAppProtocolException exc)
			{
				CBProtocol.SelectedItem = exc.m_protocol;
				MessageBox.Show(
					String.Format(Strings.MessageNoProtocolApp, exc.m_protocol.Name),
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);
			}
			catch (InvalidOperationException)
			{
				MessageBox.Show(
					String.Format(Strings.MessageSettingsNotSaved, m_Configuration.GetConfigFile()),
					App.GetName(),
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}

	}
}
