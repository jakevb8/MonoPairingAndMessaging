using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using PairMessagingLibrary;
using System.Xml;

namespace AndroidClient
{
	[Activity (Label = "Messaging Client", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private Button _btnSendMessage;
		private TextView _txtResult;
		private TextView _txtIp;
		private EditText _txtMessage;
		private ListView _listServers;
		private AlertDialog _alertDialog;
		private AutoDiscoveryClient _autoDiscovery;
		private IList<string> _serverIps;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			_btnSendMessage = (Button)FindViewById (Resource.Id.btnSendMessage);
			_txtIp = (TextView)FindViewById (Resource.Id.txtIp);
			_txtResult = (TextView)FindViewById (Resource.Id.txtResult);
			_txtMessage = (EditText)FindViewById (Resource.Id.txtMessage);

			Spinner spinner = FindViewById<Spinner> (Resource.Id.spnrType);
			IList<String> items = new List<String> ();
			items.Add ("Tcp");
			items.Add ("Udp");
			spinner.Adapter = new ArrayAdapter<String> (this, Android.Resource.Layout.SimpleSpinnerItem, items);

			Button btnRefreshServers = (Button)FindViewById (Resource.Id.btnRefreshServers);
			btnRefreshServers.Click += (object sender, EventArgs e) => {
				FindServers ();
			};

			_btnSendMessage.Click += (object sender, EventArgs e) => {
				TextView txtIp = (TextView)FindViewById (Resource.Id.txtIp);

				try {
					if (spinner.SelectedItem == null) {
						return;
					}
					String protocolType = spinner.SelectedItem.ToString ();
					String result = "";
					if (protocolType.Equals ("Udp")) {
						UdpUtils.SendMessage (txtIp.Text, _txtMessage.Text);
					} else if (protocolType.Equals ("Tcp")) {
						result = TcpUtils.SendMessage (txtIp.Text, _txtMessage.Text);
					}
					_txtResult.Text = result;
				} catch (Exception ex) {
					Android.Util.Log.Error ("PairClient", ex.ToString ());
				} 
			};

			FindServers ();
		}

		protected override void OnStop ()
		{
			if (_listServers != null) {
				_listServers.ItemClick -= _listServersItemClick;
			}
			if (_alertDialog != null) {
				_alertDialog.Dismiss ();
				_alertDialog = null;
			}
			RemoveDiscoveryListener ();
			base.OnStop ();
		}

		private void RemoveDiscoveryListener ()
		{
			if (_autoDiscovery != null) {
				_autoDiscovery.NewServerFound -= _newServerFound;
				_autoDiscovery.Stop ();
				_autoDiscovery = null;
			}
		}

		private void FindServers ()
		{
			RemoveDiscoveryListener ();
			_serverIps = new List<string> ();
			var builder = new AlertDialog.Builder (this);
			builder.SetTitle ("Pick a server to pair with:");
			_alertDialog = builder.Create ();
			_listServers = new ListView (this);
			_listServers.Adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleSpinnerItem, _serverIps);
			_listServers.ItemClick += _listServersItemClick;
			_alertDialog.SetView (_listServers);
			_alertDialog.Show ();

			_autoDiscovery = new AutoDiscoveryClient ();
			_autoDiscovery.NewServerFound += _newServerFound;
			_autoDiscovery.Start ();
		}

		private void _listServersItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			_listServers.ItemClick -= _listServersItemClick;
			RemoveDiscoveryListener ();
			_txtIp.Text = _serverIps [e.Position];
			_btnSendMessage.Enabled = true;
			_txtMessage.Enabled = true;
			_alertDialog.Dismiss ();
			_alertDialog = null;
		}

		private void _newServerFound (object sender, EventArgs e)
		{
			if (e is NewServerFoundEventArgs) {
				RunOnUiThread (new Action (delegate {
					_serverIps.Add (((NewServerFoundEventArgs)e).IpAddress);
					if (_alertDialog != null) {
						_listServers.Adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleSpinnerItem, _serverIps);
						_alertDialog.SetView (_listServers);
					}
				}));

			}
		}
	}
}


