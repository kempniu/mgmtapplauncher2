using System.ComponentModel;
using System.Xml.Serialization;

namespace mgmtapplauncher2
{
	public class Protocol : INotifyPropertyChanged
	{

		private bool m_Handled;
		private string m_App;
		private string m_Args;

		public string Name { get; set; }

		[XmlIgnore]
		public bool Handled {
			get
			{
				return m_Handled;
			}
			set
			{
				m_Handled = value;
				NotifyPropertyChanged("Handled");
			}
		}

		public string App
		{
			get
			{
				return m_App;
			}
			set
			{
				m_App = value;
				NotifyPropertyChanged("App");
			}
		}

		public string Args
		{
			get
			{
				return m_Args;
			}
			set
			{
				m_Args = value;
				NotifyPropertyChanged("Args");
			}
		}

		public Protocol()
		{
		}

		#region INotifyPropertyChanged Members

		// Need to implement this interface in order to get data binding to work properly
		private void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

	}
}
