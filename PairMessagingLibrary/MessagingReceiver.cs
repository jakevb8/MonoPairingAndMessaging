using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PairMessagingLibrary
{
	public class MessagingReceiver
	{
		public EventHandler MessageReceived;
		public EventHandler ErrorMessageReceived;
		private Thread _udpListener;
		private Thread _tcpListener;

		public MessagingReceiver ()
		{

		}

		public void Start ()
		{
			_udpListener = new Thread (InitializeUdpListening);
			_tcpListener = new Thread (InitializeTcpListening);
			_udpListener.Start ();
			_tcpListener.Start ();
		}

		public void Stop ()
		{
			_udpListener.Abort ();
			_tcpListener.Abort ();
		}

		private void InitializeUdpListening ()
		{
			bool alive = true;
			while (alive && _udpListener.IsAlive) {
				try {
					//UdpClient udpClient = new UdpClient (UdpUtils.REDIRECTED_SERVERPORT);

					var socket = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					socket.Bind (new IPEndPoint (IPAddress.Any, Constants.MESSAGING_SERVERPORT));

					byte[] packet = new byte[1500];
					socket.Receive (packet);
					if (packet != null) {
						if (MessageReceived != null) {

							var message = Encoding.Default.GetString (packet);
							MessageReceived (this, new SocketMessageEventArgs (message));
						}
					}
				} catch (Exception ex) {
					alive = false;
					if (ErrorMessageReceived != null) {
						ErrorMessageReceived (this, new SocketMessageEventArgs (ex.ToString ()));
					}
				}
			}
		}

		private void InitializeTcpListening ()
		{
			bool alive = true;
			try {
				var socket = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Bind (new IPEndPoint (IPAddress.Any, Constants.MESSAGING_SERVERPORT));
				socket.Listen (10);
				while (alive && _tcpListener.IsAlive) {
					try {
						String data = null;
						using (var connectedSocket = socket.Accept ()) {
							while (true) {
								byte[] bytes = new byte[1024];
								int bytesRec = connectedSocket.Receive (bytes);
								data += Encoding.ASCII.GetString (bytes, 0, bytesRec);
								if (data.IndexOf ("<EOF>") > -1) {
									break;
								}
							}
							data = data.Replace ("<EOF>", "");
							if (MessageReceived != null) {
								MessageReceived (this, new SocketMessageEventArgs (data));
							}
							byte[] successMessage = Encoding.ASCII.GetBytes ("Message sent successful");
							int result = connectedSocket.Send (successMessage);
							connectedSocket.Shutdown (SocketShutdown.Both);
						}

					} catch (Exception ex) {
						alive = false;
						if (ErrorMessageReceived != null) {
							ErrorMessageReceived (this, new SocketMessageEventArgs (ex.ToString ()));
						}
					}
				}
			} catch (Exception ex) {
				alive = false;
				if (ErrorMessageReceived != null) {
					ErrorMessageReceived (this, new SocketMessageEventArgs (ex.ToString ()));
				}
			}
		}
	}
}

