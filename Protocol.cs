using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace mgmtapplauncher2
{
	public class Protocol : INotifyPropertyChanged
	{

		private string m_App;

		public string Name { get; set; }
		[XmlIgnore]
		public bool Handled { get; set; }
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
		public string Args { get; set; }

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
