using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace PairMessagingLibrary
{
	public class UdpUtils
	{
		public UdpUtils ()
		{
		}

		public static void SendMessage (String ipAddress, String message)
		{
			using (UdpClient udpClient = new UdpClient (ipAddress, Constants.MESSAGING_SERVERPORT)) {
				IPAddress[] addresses = Dns.GetHostAddresses (Dns.GetHostName ());
				foreach (IPAddress ip in addresses) {
					if (ip.AddressFamily == AddressFamily.InterNetwork) {
						byte[] send_buffer = Encoding.Default.GetBytes (ip.ToString () + ": " + message);
						udpClient.Send (send_buffer, send_buffer.Length);
					}
				}
			}
		}

		public static void SendBroadcastMessage (string message)
		{
			using (var socket = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption (IPAddress.Parse (Constants.AUTODISCOVERY_IP_ADDRESS)));

				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
				byte[] buffer = Encoding.Default.GetBytes (message + Constants.EOF_ID);

				socket.Connect (new IPEndPoint (IPAddress.Parse (Constants.AUTODISCOVERY_IP_ADDRESS), Constants.AUTODISCOVERY_SERVERPORT));
				socket.Send (buffer, buffer.Length, SocketFlags.None);
			}
		}
	}
}

