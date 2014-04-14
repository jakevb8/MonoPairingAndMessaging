using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using Java.Net;
using Org.Apache.Http.Conn.Util;
using Java.Util;
using PairMessagingLibrary;

namespace ServerApp
{
	[Activity (Label = "Messaging Server", MainLauncher = true)]
	public class MainActivity : Activity
	{
		TextView _txtMessage;
		MessagingReceiver _messagingReceiver;
		AutoDiscoveryServer _autoDiscovery;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			_txtMessage = FindViewById<TextView> (Resource.Id.txtMessage);
			TextView txtIp = FindViewById<TextView> (Resource.Id.textViewIpAddress);
			String ip = GetIPAddress (true);
			txtIp.Text = ip;

			_autoDiscovery = new AutoDiscoveryServer ();
		}

		protected override void OnResume ()
		{
			_messagingReceiver = new MessagingReceiver ();
			_messagingReceiver.MessageReceived += _messageReceivedEventHandler;
			_messagingReceiver.Start ();

			base.OnResume ();
		}

		private void _messageReceivedEventHandler (object sender, EventArgs e)
		{
			if (e is SocketMessageEventArgs) {
				RunOnUiThread (new Action (delegate {
					_txtMessage.Text = ((SocketMessageEventArgs)e).Message;
				}));
			}
		}

		protected override void OnPause ()
		{
			if (_messagingReceiver != null) {
				_messagingReceiver.MessageReceived -= _messageReceivedEventHandler;
				_messagingReceiver.Stop ();
				_messagingReceiver = null;
			}
			base.OnPause ();
		}

		public static String GetIPAddress (bool useIPv4)
		{
			try {
				IEnumeration networkInterfaces = NetworkInterface.NetworkInterfaces;

				while (networkInterfaces.HasMoreElements) {
					NetworkInterface intf = networkInterfaces.NextElement () as NetworkInterface;
					IEnumeration inetAddresses = intf.InetAddresses;

					while (inetAddresses.HasMoreElements) {
						InetAddress addr = inetAddresses.NextElement () as InetAddress;
						if (!addr.IsLoopbackAddress) {
							String sAddr = addr.HostAddress.ToUpper ();
							bool isIPv4 = InetAddressUtils.IsIPv4Address (sAddr); 
							if (useIPv4) {
								if (isIPv4 && !sAddr.Equals ("127.0.0.1"))
									return sAddr;
							} else {
								if (!isIPv4) {
									int delim = sAddr.IndexOf ('%'); // drop ip6 port suffix
									return delim < 0 ? sAddr : sAddr.Substring (0, delim);
								}
							}
						}
					}
				}
			} catch (Exception ex) {
			} // for now eat exceptions
			return "";
		}
	}
}


