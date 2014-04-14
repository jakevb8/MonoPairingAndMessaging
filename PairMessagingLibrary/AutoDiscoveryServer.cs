using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PairMessagingLibrary
{
	public class AutoDiscoveryServer
	{
		private Thread _listener;

		public AutoDiscoveryServer ()
		{
			_listener = new Thread (InitializeListener);
			_listener.Start ();
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
						if (message.StartsWith (Constants.AUTODISCOVERY_ID)) {
							IPAddress[] addresses = Dns.GetHostAddresses (Dns.GetHostName ());
							foreach (IPAddress ip in addresses) {
								if (ip.AddressFamily == AddressFamily.InterNetwork) {
									UdpUtils.SendBroadcastMessage (Constants.AUTODISCOVERY_FOUND_ID + ":" + ip.ToString ());
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

