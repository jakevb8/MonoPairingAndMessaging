using System;

namespace PairMessagingLibrary
{
	public class SocketMessageEventArgs : EventArgs
	{
		private String _message;

		public SocketMessageEventArgs (String message)
		{
			_message = message;
		}

		public String Message {
			get {
				return _message;
			}
		}
	}
}

