using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace PairMessagingLibrary
{
	public class TcpUtils
	{
		public static int REDIRECTED_SERVERPORT = 6000;

		public TcpUtils ()
		{
		}

		public static string SendMessage(String ipAddress, String message)
		{
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName ());
				foreach (IPAddress ip in addresses) {
					if (ip.AddressFamily == AddressFamily.InterNetwork) {
						IPAddress[] serverAddresses = Dns.GetHostAddresses (ipAddress);
						IPEndPoint endPoint = new IPEndPoint (serverAddresses[0], REDIRECTED_SERVERPORT);
						socket.Connect (endPoint);
						byte[] send_buffer = Encoding.ASCII.GetBytes (ip.ToString () + ": " + message + "<EOF>");
						socket.Send (send_buffer);
						byte[] returnData = new byte[1024];
						socket.Receive (returnData);
						return Encoding.ASCII.GetString(returnData);
					}
				}
			}
			return "";
		}
	}
}

