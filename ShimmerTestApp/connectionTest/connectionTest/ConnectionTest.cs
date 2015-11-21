using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.Windows.Forms;
using ShimmerAPI;
using ShimmerLibrary;

namespace connectionTest
{
    public partial class ConnectionTest : Form
    {
        private string ComPort;

        //use long constructor here
        public ShimmerSDBT ShimmerDevice = new ShimmerSDBT("Shimmer", "");

        public ConnectionTest()
        {
            InitializeComponent();
            Application.ThreadException += new ExceptionEventHandler().ApplicationThreadException;
            AppDomain.CurrentDomain.UnhandledException += new ExceptionEventHandler().CurrentDomainUnhandledException;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShimmerDevice = new ShimmerSDBT("Shimmer", ComPort, 20.0,0,0,0,0);
            ShimmerDevice.UICallback += this.HandleEvent;
            populateComPorts();
        }

        public void populateComPorts()
        {
            ComPort = comboBoxComPorts.Text;

            String[] names = SerialPort.GetPortNames();
            foreach (String s in names)
            {
                comboBoxComPorts.Items.Add(s);
            }
            comboBoxComPorts.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxComPorts.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        public void Connect()
        {
            //for Shimmer and ShimmerSDBT
            ShimmerDevice.SetComPort(comboBoxComPorts.Text);
            //ShimmerDevice.
            ShimmerDevice.SetAccelRange(Convert.ToInt32(textBoxAccelRange.Text));
            ShimmerDevice.SetGyroRange(Convert.ToInt32(textBoxAccelRange.Text));
            ShimmerDevice.SetGSRRange(Convert.ToInt32(textBoxAccelRange.Text));
            //ShimmerDevice.Set
            //ShimmerDevice.SetEnabledSensors(Convert.ToInt32(textBoxAccelRange.Text));
            //ShimmerDevice.SetComPort(comPort);

            //for Shimmer32Feet and ShimmerSDBT32Feet
            //shimmer.SetAddress("00066666940E");
            bool connect = true; // check to connect one at a time

            if (ShimmerDevice.GetState() != Shimmer.SHIMMER_STATE_CONNECTED)
            {
                if (connect)
                {
                    ShimmerDevice.StartConnectThread();
                    connect = false;
                }
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Connect();
        }

        public void HandleEvent(object sender, EventArgs args)
        {
            CustomEventArgs eventArgs = (CustomEventArgs)args;
            int indicator = eventArgs.getIndicator();

            switch (indicator)
            {
                case (int)Shimmer.ShimmerIdentifier.MSG_IDENTIFIER_STATE_CHANGE:
                    System.Diagnostics.Debug.Write(((Shimmer)sender).GetDeviceName() + " State = " + ((Shimmer)sender).GetStateString() + System.Environment.NewLine);
                    int state = (int)eventArgs.getObject();
                    if (state == (int)Shimmer.SHIMMER_STATE_CONNECTED)
                    { 
                        labelConnectionState.Text = "Connected";
                    }
                    else if (state == (int)Shimmer.SHIMMER_STATE_CONNECTING)
                    {
                        labelConnectionState.Text = "Connecting";
                    }
                    else if (state == (int)Shimmer.SHIMMER_STATE_NONE)
                    {
                        labelConnectionState.Text = "Disconnected";
                    }
                    else if (state == (int)Shimmer.SHIMMER_STATE_STREAMING)
                    {
                        labelConnectionState.Text = "Streaming";
                    }
                    break;
                default:
                    labelConnectionState.Text = "State not changed";
                    break;
            }
        }
    }

    // ExceptionReporter Class
    internal class ExceptionEventHandler
    {
        private static readonly string ApplicationName = Application.ProductName.ToString().Replace("_", " ");
        //    private string versionNumber = Application.ProductVersion.ToString().Substring(0, Application.ProductVersion.ToString().LastIndexOf(".")).ToLower();
        private string versionNumber = Application.ProductVersion.ToString();

        public void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //ReportCrash(e.ExceptionObject as Exception);
            Environment.Exit(0);
        }

        public void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //ReportCrash(e.Exception);
            Environment.Exit(0);
        }

        private static void ReportCrash(Exception exception)
        {

        }
    }
}
