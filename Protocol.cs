using System;
using System.ComponentModel;

namespace mgmtapplauncher2
{
	public class Protocol : INotifyPropertyChanged
	{

		private string app;

		public string Name { get; set; }
		public bool Handled { get; set; }
		public string App
		{
			get
			{
				return app;
			}
			set
			{
				app = value;
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
