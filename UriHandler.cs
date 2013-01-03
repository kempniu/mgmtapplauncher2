using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

			string protocol;
			string host;
			Protocol p;

			Regex r = new Regex("^(?'protocol'[a-z]+)://(?'host'[^/]+)/?$", RegexOptions.IgnoreCase);
			Match m = r.Match(m_Uri);

			if (m.Success)
			{
				protocol = m.Groups["protocol"].Value.ToLower();
				host = m.Groups["host"].Value;
			}
			else
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
		public string m_Protocol;
		public ProtocolNotSupportedException(string protocol)
		{
			m_Protocol = protocol;
		}
	}

	public class ProgramStartFailedException : Exception
	{
		public string m_Program;
		public ProgramStartFailedException(string program)
		{
			m_Program = program;
		}
	}

}
