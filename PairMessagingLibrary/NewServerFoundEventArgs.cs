using System;

namespace PairMessagingLibrary
{
	public class NewServerFoundEventArgs: EventArgs
	{
		private string _ipAddress;

		public NewServerFoundEventArgs (string ipAddress)
		{
			_ipAddress = ipAddress;
		}

		public string IpAddress {
			get {
				return _ipAddress;
			}
		}
	}
}

