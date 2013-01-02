using System;
using System.Diagnostics;

namespace mgmtapplauncher2
{

	class UriHandler
	{

		private Configuration m_Configuration;
		private string m_Uri;

		public UriHandler(Configuration configuration, string uri)
		{
			m_Configuration = configuration;
			m_Uri = uri;
		}

		public void Handle()
		{

			int pos1;
			int pos2;
			string protocol;
			string host;
			Protocol p;

			try
			{
				pos1 = m_Uri.IndexOf(':');
				if (pos1 == -1 || m_Uri.Substring(pos1, 3) != "://")
					throw new ArgumentException();
				protocol = m_Uri.Substring(0, pos1);
				pos1 += 3;
				pos2 = m_Uri.IndexOf('/', pos1);
				if (pos2 == -1)
					pos2 = m_Uri.Length;
				host = m_Uri.Substring(pos1, pos2 - pos1);
			}
			catch
			{
				throw new InvalidUriException();
			}

			p = m_Configuration.GetProtocol(protocol);

			if (p == null)
			{
				throw new ProtocolNotSupportedException(protocol);
			}
			else
			{
				try
				{
					Process process = new Process();
					process.StartInfo.FileName = p.App;
					process.StartInfo.Arguments = p.Args.Replace("%P%", protocol).Replace("%H%", host);
					process.Start();
				}
				catch
				{
					throw new ProgramStartFailedException(p.App);
				}
			}

		}

	}

	public class InvalidUriException : Exception
	{
	}

	public class ProtocolNotSupportedException : Exception
	{
		public string protocol;
		public ProtocolNotSupportedException(string p)
			: base(p)
		{
			protocol = p;
		}
	}

	public class ProgramStartFailedException : Exception
	{
		public string program;
		public ProgramStartFailedException(string p)
			: base(p)
		{
			program = p;
		}
	}

}
