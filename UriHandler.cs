using System;
using System.Diagnostics;

namespace mgmtapplauncher2
{

	class UriHandler
	{

		private Configuration configuration;
		private string uri;

		public UriHandler(Configuration c, string u)
		{
			configuration = c;
			uri = u;
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
				throw new InvalidUriException();
			}

			p = configuration.GetProtocol(protocol);

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
