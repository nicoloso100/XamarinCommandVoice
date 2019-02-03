using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Content;
using Android.Bluetooth;
using Java.Util;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Android.Content.PM;
using Android.Views;
using System.Threading;
using System;

namespace ComandosVoz
{

    #region Inicio
    

    [Activity(Label = "ComandosVoz", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        public AlertDialog alerta;
        private TextView textBox;
        private TextView Muestra;
        private Button recButton;
        public static Button ConBut;
        public static Button SBut;
        private BluetoothAdapter AdaptadorBlue;
        private BluetoothSocket btSocket = null;
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Stream outStream = null;
        private Stream inStream = null;
        public static string Address;
        Intent voiceIntent;
        int Boton1;
        int Boton2;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            recButton = FindViewById<Button>(Resource.Id.button1);
            ConBut = FindViewById<Button>(Resource.Id.button2);
            SBut = FindViewById<Button>(Resource.Id.button3);
            SBut.Visibility = ViewStates.Invisible;
            textBox = FindViewById<TextView>(Resource.Id.textView1);
            Muestra = FindViewById<TextView>(Resource.Id.textView2);
            AdaptadorBlue = BluetoothAdapter.DefaultAdapter;
            recButton.Enabled = false;
            Boton1 = Resource.Drawable.Boton;
            Boton2 = Resource.Drawable.Boton2;
            CheckBlue();
            CheckMicro();

            ConBut.Click += ConBut_Click;
            SBut.Click += SBut_Click1;
            
        }

        private void SBut_Click1(object sender, EventArgs e)
        {
            if (SBut.Text.Equals("Desconectar"))
            {
                ConBut.Enabled = true;
                btSocket.Close();
                SBut.Text = "Conectar";
            }
            else if(SBut.Text.Equals("Conectar"))
            {
                alerta = new AlertDialog.Builder(this).Create();
                alerta.DismissEvent += Alerta_DismissEvent1;
                alerta.SetTitle("Mensaje:");
                alerta.SetMessage("El dispodsitivo se conectará");
                alerta.Show();
                SBut.Text = "Conectando...";
                alerta.Dismiss();
                SBut.Enabled = true;
                ConBut.Enabled = false;
            }

        }

        private void Alerta_DismissEvent1(object sender, EventArgs e)
        {
            Conecta();
        }

        private void ConBut_Click(object sender, System.EventArgs e)
        {
            var myIntent = new Intent(this, typeof(Conexion));
            StartActivityForResult(myIntent, 2);
        }

        #endregion

        #region CheckMic

        public void CheckMicro()
        {
            string rec = PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                AlertDialog alerta = new AlertDialog.Builder(this).Create();
                alerta.SetTitle("Error!");
                alerta.SetMessage("No se ha detectado un micrófono disponible!");
                alerta.SetButton("Aceptar", (g, h) => { });
                alerta.Show();
            }
            else
            {
                recButton.Click += RecButton_Click;
            }
        }

        private void RecButton_Click(object sender, EventArgs e)
        {
            Habla();
            recButton.SetBackgroundResource(Boton2);
        }

        #endregion

        #region CheckBlue

        public void CheckBlue()
        {
            if (AdaptadorBlue == null)
            {
                AlertDialog alerta = new AlertDialog.Builder(this).Create();
                alerta.SetTitle("Error!");
                alerta.SetMessage("No se ha podido comunicar con el Bluetooth del dispositivo");
                alerta.SetButton("Aceptar", (g, h) => { });
                alerta.Show();
            }
            else
            {
                if (!AdaptadorBlue.IsEnabled)
                {
                    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                    StartActivityForResult(enableBtIntent, 0);
                }
            }
            
        }

        #endregion

        #region Conecta

        public void Conecta()
        {
            BluetoothDevice device = AdaptadorBlue.GetRemoteDevice(Address);
            AdaptadorBlue.CancelDiscovery();
            try
            {
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                btSocket.Connect();
                recButton.Enabled = true;
                beginListenForData();
                SBut.Text = "Desconectar";
            }
            catch (Exception e)
            {
                alerta = new AlertDialog.Builder(this).Create();
                alerta.SetTitle("Error!");
                alerta.SetMessage("No se ha podido conectar con el dispositivo");
                alerta.SetButton("Aceptar", (g, h) => { });
                alerta.Show();
                SBut.Text = "Conectar";
                ConBut.Enabled = true;
            }
        }

        #endregion

        #region Recibe

        public void beginListenForData()
        {
            try
            {
                inStream = btSocket.InputStream;
                Muestra.Text = "Recibir";
            }
            catch (IOException ex)
            {
                alerta = new AlertDialog.Builder(this).Create();
                alerta.SetTitle("Error!");
                alerta.SetMessage(ex.ToString());
                alerta.SetButton("Aceptar", (g, h) => { });
                alerta.Show();
            }

            Task.Factory.StartNew(() => {
                byte[] buffer = new byte[1024];
                int bytes;
                while (true)
                {
                    try
                    {
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        if (bytes > 0)
                        {
                            RunOnUiThread(() => {
                                string valor = Encoding.ASCII.GetString(buffer);
                                Muestra.Text += Convert.ToString(valor);
                            });
                        }
                    }
                    catch (Java.IO.IOException)
                    {
                        RunOnUiThread(() => {
                            Muestra.Text = string.Empty;
                        });
                        break;
                    }
                }
            });
        }

        #endregion

        #region Habla y envía

        public void Habla()
        {
            voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 200);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            StartActivityForResult(voiceIntent, 1);
        }
        

        private void writeData(string data)
        {
            string message = data;
            byte[] msgBuffer = Encoding.ASCII.GetBytes(message);
            try
            {
                outStream = btSocket.OutputStream;
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
                Habla();
            }
            catch 
            {
                alerta.SetTitle("Error!");
                alerta.SetMessage("Error al enviar el comando, posiblemente no esté conectado");
                alerta.SetButton("Aceptar", (g, h) => { });
                alerta.Show();
                recButton.Enabled = false;
                SBut.Text = "Conectar";
                recButton.SetBackgroundResource(Boton1);
            }
        }

        #endregion

        #region Activity Result

        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if(requestCode == 0)
            {
                if(resultVal == Result.Ok)
                {
                    CheckBlue();
                }
                else
                {
                    alerta.SetTitle("Atención");
                    alerta.SetMessage("La aplicación no puede usarse sin el bluetooth activado!");
                    alerta.SetButton("Aceptar", (g, h) => { });
                    alerta.Show();
                    alerta.DismissEvent += Alerta_DismissEvent;
                }
            }

            else if(requestCode == 1)
            {

                textBox.Text = "";
                if (resultVal == Result.Ok)
                {

                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = textBox.Text + matches[0];

                        if (!textInput.ToLower().Equals("detener comandos"))
                        {
                            if (textInput.Split(' ')[textInput.Split(' ').Length - 1].ToLower().Equals("cancelar"))
                            {
                                textBox.Text = "Cancelado";
                                Habla();
                            }
                            else
                            {
                                textBox.Text = textInput;
                                textInput = textInput + ".";
                                if (AdaptadorBlue.IsEnabled)
                                {
                                    writeData(textInput);
                                }
                                else
                                {
                                    alerta.SetTitle("Error!");
                                    alerta.SetMessage("No está conectado!");
                                    alerta.SetButton("Aceptar", (g, h) => { });
                                    alerta.Show();
                                }
                            }
                        }
                        else
                        {
                            recButton.SetBackgroundResource(Boton1);
                            textBox.Text = "Detenido";
                        }
                    }
                    else
                    {
                        recButton.SetBackgroundResource(Boton1);
                        textBox.Text = "No se han detectado palabras";
                    }
                }
                else
                {
                    recButton.SetBackgroundResource(Boton1);
                    textBox.Text = "Detenido";
                }

                base.OnActivityResult(requestCode, resultVal, data);

            }
        }

        private void Alerta_DismissEvent(object sender, System.EventArgs e)
        {
            Process.KillProcess(Process.MyPid());
        }

        #endregion

    }

}