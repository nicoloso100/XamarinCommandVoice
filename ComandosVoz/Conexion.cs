using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;

namespace ComandosVoz
{
    [Activity(Label = "Conexio")]
    public class Conexion : Activity
    {
        private ListView LV;
        private Button Salir;
        private BluetoothAdapter AdaptadorBlue;
        List<string> DConectados;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Conecta);
            CargaBlue();
        }

        public void CargaBlue()
        {
            LV = FindViewById<ListView>(Resource.Id.listView1);
            Salir = FindViewById<Button>(Resource.Id.button1);
            Salir.Click += Salir_Click;
            LV.ItemClick += DeviceListClick;

            AdaptadorBlue = BluetoothAdapter.DefaultAdapter;
            var pairedDevices = AdaptadorBlue.BondedDevices;
            DConectados = new List<string>();

            if (pairedDevices.Count > 0)
            {
                foreach (var device in pairedDevices)
                {
                    DConectados.Add(device.Name + "\n" + device.Address);
                }
            }
            ArrayAdapter<string> pairedDevicesArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, DConectados);
            LV.Adapter = pairedDevicesArrayAdapter;
        }

        private void Salir_Click(object sender, EventArgs e)
        {
            Finish();
        }

        void DeviceListClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var info = (e.View as TextView).Text.ToString();
            var address = info.Substring(info.Length - 17);
            MainActivity.Address = address;
            MainActivity.ConBut.Text = info;
            MainActivity.SBut.Visibility = ViewStates.Visible;
            this.Finish();
        }
    }
}