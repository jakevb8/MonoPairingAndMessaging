using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PairMessagingLibrary
{
	public class AutoDiscoveryClient: IDisposable
	{
		private Thread _listener;
		public EventHandler NewServerFound;
		private string _ipAddress;

		public AutoDiscoveryClient ()
		{
			IPAddress[] addresses = Dns.GetHostAddresses (Dns.GetHostName ());
			foreach (IPAddress ip in addresses) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					_ipAddress = ip.ToString ();
				}
			}
			_listener = new Thread (InitializeListener);
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			Stop ();
		}

		#endregion

		public void Start ()
		{
			_listener.Start ();

			UdpUtils.SendBroadcastMessage (Constants.AUTODISCOVERY_ID);
		}

		public void Stop ()
		{
			_listener.Abort ();
		}

		private void InitializeListener ()
		{
			bool alive = true;
			var socket = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Bind (new IPEndPoint (IPAddress.Any, Constants.AUTODISCOVERY_SERVERPORT));
			socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption (IPAddress.Parse (Constants.AUTODISCOVERY_IP_ADDRESS), IPAddress.Any));

			while (alive && _listener.IsAlive) {
				try {
					byte[] packet = new byte[1500];
					socket.Receive (packet);
					if (packet != null) {
						var message = Encoding.Default.GetString (packet);
						if (message.StartsWith (Constants.AUTODISCOVERY_FOUND_ID)) {
							var messageParts = message.Split (new string[] { Constants.EOF_ID }, StringSplitOptions.RemoveEmptyEntries) [0].Split (':');
							if (messageParts.Length > 1) {
								if (!messageParts [1].Equals (_ipAddress)) {
									if (NewServerFound != null) {
										NewServerFound (this, new NewServerFoundEventArgs (messageParts [1]));
									}
								}
							}

						}
					}
				} catch {
					alive = false;
				}
			}
		}
	}
}

