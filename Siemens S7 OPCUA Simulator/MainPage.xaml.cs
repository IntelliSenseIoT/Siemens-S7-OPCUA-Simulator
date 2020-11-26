using OPCUA.Library;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Siemens_S7_OPCUA_Simulator
{
    public sealed partial class MainPage : Page
    {
        OPCUAClient OPCUAClient = new OPCUAClient();

        public MainPage()
        {
            this.InitializeComponent();

            Connect_Click(null, null);
        }

        //OPC UA Simulator connection
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (OPCUAClient.Connected == true)
            {
                MessageList.Items.Add("It is connected!");
                return;
            }

            var certificateFile = await Package.Current.InstalledLocation.GetFileAsync(@"Client.Uwp.pfx");
            OPCUAClient.CertificateFilePath = certificateFile.Path;
            OPCUAClient.ServerAddress = ConnectionString.Text;
            var res = OPCUAClient.Connect();

            if (res.Success == true)
            {
                MessageList.Items.Add("Connection is successful. " + ConnectionString.Text);

                List<OPCReadNode> ReadOPCNodes = new List<OPCReadNode>();

                OPCReadNode onenode = new OPCReadNode();
                onenode.Name = "DistanceMin";
                onenode.NodeId = "ns=3;s=DistanceMin";
                ReadOPCNodes.Add(onenode);
               
                OPCReadNode onenode1 = new OPCReadNode();
                onenode1.Name = "DistanceMax";
                onenode1.NodeId = "ns=3;s=DistanceMax";
                ReadOPCNodes.Add(onenode1);

                OPCReadNode onenode2 = new OPCReadNode();
                onenode2.Name = "TorqueMin";
                onenode2.NodeId = "ns=3;s=TorqueMin";
                ReadOPCNodes.Add(onenode2);

                OPCReadNode onenode3 = new OPCReadNode();
                onenode3.Name = "TorqueMax";
                onenode3.NodeId = "ns=3;s=TorqueMax";
                ReadOPCNodes.Add(onenode3);

                var resultreadopc = await OPCUAClient.ReadValues(ReadOPCNodes);

                if (resultreadopc.Success == true)
                {
                    foreach (var item in resultreadopc.OPCValues)
                    {
                        if (item.Name == "DistanceMin" && item.IsGood == true)
                        {
                            DistanceMin.Value = (double)item.Value; 
                        }
                        else if (item.Name == "DistanceMax" && item.IsGood == true)
                        {
                            DistanceMax.Value = (double)item.Value;
                        }
                        else if (item.Name == "TorqueMin" && item.IsGood == true)
                        {
                            TorqueMin.Value = (double)item.Value;
                        }
                        else if (item.Name == "TorqueMax" && item.IsGood == true)
                        {
                            TorqueMax.Value = (double)item.Value;
                        }
                    }
                }
                else
                {
                    MessageList.Items.Add("Limit values reading - OPC UA communication error!");
                }
            }
            else
            {
                MessageList.Items.Add("Connection is not successful! " + ConnectionString.Text);
            }
        }

        //Disconnect OPC UA Simulator
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            var res = OPCUAClient.Disconnect();
            if (res.Success == true)
            {
                MessageList.Items.Add("Disconnection is successful.");
            }
            else
            {
                MessageList.Items.Add("Disconnection is not successful!");
            }
        }

        //Manufacturing process
        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            LoadingControl.IsLoading = true;
            MessageList.Items.Clear();

            if (OPCUAClient.Connected == false)
            {
                MessageList.Items.Add("Not connected to the OPC UA server!");
                LoadingControl.IsLoading = false;
                return;
            }

            #region Read Run Status

            List<OPCReadNode> ReadOPCNodes = new List<OPCReadNode>();

            OPCReadNode onenode = new OPCReadNode();

            onenode.Name = "Run";
            onenode.NodeId = "ns=3;s=Run";
            ReadOPCNodes.Add(onenode);
            var resultreadopc = await OPCUAClient.ReadValues(ReadOPCNodes);

            bool GoodTag = false;
            bool RunStatus = false;
            if (resultreadopc.Success == true)
            {
                foreach (var item in resultreadopc.OPCValues)
                {
                    if (item.Name == "Run" && item.IsGood == true)
                    {
                        GoodTag = true;
                        if ((double)item.Value >0)
                        {
                            RunStatus = true;
                        }
                    }
                }   
            }
            else
            {
                MessageList.Items.Add("OPC UA communication error!");
                LoadingControl.IsLoading = false;
                return;
            }

            if (GoodTag == false)
            {
                MessageList.Items.Add("OPC UA communication error!");
                LoadingControl.IsLoading = false;
                return;
            }
            if (RunStatus == true)
            {
                MessageList.Items.Add("Process is running!");
                LoadingControl.IsLoading = false;
                return;
            }
            #endregion

            #region Start Process
            List<OPCWriteNode> OPCNodes = new List<OPCWriteNode>();

            OPCWriteNode onenodew = new OPCWriteNode();
            onenodew.Name = "Run";
            onenodew.NodeId = "ns=3;s=Run";
            onenodew.Value = 1.0; //true
            OPCNodes.Add(onenodew);

            OPCWriteNode onenodew1 = new OPCWriteNode();
            onenodew1.Name = "Completed";
            onenodew1.NodeId = "ns=3;s=Completed";
            onenodew1.Value = 0.0; //false
            OPCNodes.Add(onenodew1);

            OPCWriteNode onenodew2 = new OPCWriteNode();
            onenodew2.Name = "Error";
            onenodew2.NodeId = "ns=3;s=Error";
            onenodew2.Value = 0.0; //false
            OPCNodes.Add(onenodew2);

            OPCWriteNode onenodew4 = new OPCWriteNode();
            onenodew4.Name = "ProcessStatus";
            onenodew4.NodeId = "ns=3;s=ProcessStatus";
            onenodew4.Value = 0.0;
            OPCNodes.Add(onenodew4);

            OPCWriteNode onenodew5 = new OPCWriteNode();
            onenodew5.Name = "Distance";
            onenodew5.NodeId = "ns=3;s=Distance";
            onenodew5.Value = 0.0;
            OPCNodes.Add(onenodew5);

            OPCWriteNode onenodew6 = new OPCWriteNode();
            onenodew6.Name = "Torque";
            onenodew6.NodeId = "ns=3;s=Torque";
            onenodew6.Value = 0.0;
            OPCNodes.Add(onenodew6);

            var resultw1 = await OPCUAClient.WriteValues(OPCNodes);
            if (resultw1.Success == false)
            {
                MessageList.Items.Add("Process start is not successful! " + resultw1.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Process start is successful.");
            }

            await Task.Delay(5000);
            #endregion

            #region Process 1
            List<OPCWriteNode> OPCNodes20 = new List<OPCWriteNode>();

            OPCWriteNode onenodew20 = new OPCWriteNode();
            onenodew20.Name = "ProcessStatus";
            onenodew20.NodeId = "ns=3;s=ProcessStatus";
            onenodew20.Value = 1.0; 
            OPCNodes20.Add(onenodew20);

            var resultw2 = await OPCUAClient.WriteValues(OPCNodes20);
            if (resultw2.Success == false)
            {
                MessageList.Items.Add("Process 1 start is not successful! " + resultw2.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Process 1 start is successful.");
            }

            await Task.Delay(8000);
            #endregion

            #region Process 2
            List<OPCWriteNode> OPCNodes30 = new List<OPCWriteNode>();

            OPCWriteNode onenodew30 = new OPCWriteNode();
            onenodew30.Name = "ProcessStatus";
            onenodew30.NodeId = "ns=3;s=ProcessStatus";
            onenodew30.Value = 2.0;
            OPCNodes30.Add(onenodew30);

            var resultw3 = await OPCUAClient.WriteValues(OPCNodes30);
            if (resultw3.Success == false)
            {
                MessageList.Items.Add("Process 2 start is not successful! " + resultw3.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Process 2 start is successful.");
            }

            await Task.Delay(12000);
            #endregion

            #region Process 3
            List<OPCWriteNode> OPCNodes40 = new List<OPCWriteNode>();

            OPCWriteNode onenodew40 = new OPCWriteNode();
            onenodew40.Name = "ProcessStatus";
            onenodew40.NodeId = "ns=3;s=ProcessStatus";
            onenodew40.Value = 3.0;
            OPCNodes40.Add(onenodew40);

            var resultw4 = await OPCUAClient.WriteValues(OPCNodes40);
            if (resultw4.Success == false)
            {
                MessageList.Items.Add("Process 3 start is not successful! " + resultw4.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Process 3 start is successful.");
            }

            await Task.Delay(6000);
            #endregion

            #region Process Completed
            List<OPCWriteNode> OPCNodesC = new List<OPCWriteNode>();
            Random rnd = new Random();

            OPCWriteNode onenodewc = new OPCWriteNode();
            onenodewc.Name = "Run";
            onenodewc.NodeId = "ns=3;s=Run";
            onenodewc.Value = 0.0; //false
            OPCNodesC.Add(onenodewc);

            OPCWriteNode onenodewc1 = new OPCWriteNode();
            onenodewc1.Name = "Completed";
            onenodewc1.NodeId = "ns=3;s=Completed";
            onenodewc1.Value = 1.0; //true
            OPCNodesC.Add(onenodewc1);

            double distance = Convert.ToDouble(rnd.Next(1, 100))/10;
            double torque = Convert.ToDouble(rnd.Next(1, 100))/10;
            bool error = false;
            if (torque < TorqueMin.Value || torque > TorqueMax.Value || distance < DistanceMin.Value || distance > DistanceMax.Value)
            {
                MessageList.Items.Add("Product quality is not good! Distance: " + distance + " mm Torque: " + torque + " kN");
                error = true;
            }
            else
            {
                MessageList.Items.Add("Product quality is good. Distance: " + distance + " mm Torque: " + torque + " kN");
            }

            OPCWriteNode onenodewc2 = new OPCWriteNode();
            onenodewc2.Name = "Error";
            onenodewc2.NodeId = "ns=3;s=Error";
            if (error == false)
            {
                onenodewc2.Value = 0.0; //false
            }
            else
            {
                onenodewc2.Value = 1.0; //true
            }
            OPCNodesC.Add(onenodewc2);

            OPCWriteNode onenodewc4 = new OPCWriteNode();
            onenodewc4.Name = "ProcessStatus";
            onenodewc4.NodeId = "ns=3;s=ProcessStatus";
            onenodewc4.Value = 0.0;
            OPCNodesC.Add(onenodewc4);

            OPCWriteNode onenodewc5 = new OPCWriteNode();
            onenodewc5.Name = "Distance";
            onenodewc5.NodeId = "ns=3;s=Distance";
            onenodewc5.Value = distance;
            OPCNodesC.Add(onenodewc5);

            OPCWriteNode onenodewc6 = new OPCWriteNode();
            onenodewc6.Name = "Torque";
            onenodewc6.NodeId = "ns=3;s=Torque";
            onenodewc6.Value = torque;
            OPCNodesC.Add(onenodewc6);

            var resultwc1 = await OPCUAClient.WriteValues(OPCNodesC);
            if (resultwc1.Success == false)
            {
                MessageList.Items.Add("Process completed is not successful! " + resultwc1.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Process completed is successful.");
            }
            #endregion

            LoadingControl.IsLoading = false;
        }

        //Apply limit values
        private async void LimitValuesButton_Click(object sender, RoutedEventArgs e)
        {
            if (OPCUAClient.Connected == false)
            {
                MessageList.Items.Add("Not connected to the OPC UA server!");
                return;
            }

            List<OPCWriteNode> OPCNodes = new List<OPCWriteNode>();

            OPCWriteNode onenodew = new OPCWriteNode();
            onenodew.Name = "DistanceMin";
            onenodew.NodeId = "ns=3;s=DistanceMin";
            onenodew.Value = DistanceMin.Value; 
            OPCNodes.Add(onenodew);

            OPCWriteNode onenodew1 = new OPCWriteNode();
            onenodew1.Name = "DistanceMax";
            onenodew1.NodeId = "ns=3;s=DistanceMax";
            onenodew1.Value = DistanceMax.Value;
            OPCNodes.Add(onenodew1);

            OPCWriteNode onenodew2 = new OPCWriteNode();
            onenodew2.Name = "TorqueMin";
            onenodew2.NodeId = "ns=3;s=TorqueMin";
            onenodew2.Value = TorqueMin.Value; 
            OPCNodes.Add(onenodew2);

            OPCWriteNode onenodew3 = new OPCWriteNode();
            onenodew3.Name = "TorqueMax";
            onenodew3.NodeId = "ns=3;s=TorqueMax";
            onenodew3.Value = TorqueMax.Value;
            OPCNodes.Add(onenodew3);

            var resultw1 = await OPCUAClient.WriteValues(OPCNodes);

            if (resultw1.Success == false)
            {
                MessageList.Items.Add("Limit values writing are not successful! " + resultw1.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Limit values writing are successful.");
            }
        }

        //Stop Manufacturing process
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            List<OPCWriteNode> OPCNodes = new List<OPCWriteNode>();

            OPCWriteNode onenodew = new OPCWriteNode();
            onenodew.Name = "Run";
            onenodew.NodeId = "ns=3;s=Run";
            onenodew.Value = 0.0; //true
            OPCNodes.Add(onenodew);

            OPCWriteNode onenodew1 = new OPCWriteNode();
            onenodew1.Name = "Completed";
            onenodew1.NodeId = "ns=3;s=Completed";
            onenodew1.Value = 0.0; //false
            OPCNodes.Add(onenodew1);

            OPCWriteNode onenodew2 = new OPCWriteNode();
            onenodew2.Name = "Error";
            onenodew2.NodeId = "ns=3;s=Error";
            onenodew2.Value = 0.0; //false
            OPCNodes.Add(onenodew2);

            OPCWriteNode onenodew4 = new OPCWriteNode();
            onenodew4.Name = "ProcessStatus";
            onenodew4.NodeId = "ns=3;s=ProcessStatus";
            onenodew4.Value = 0.0;
            OPCNodes.Add(onenodew4);

            OPCWriteNode onenodew5 = new OPCWriteNode();
            onenodew5.Name = "Distance";
            onenodew5.NodeId = "ns=3;s=Distance";
            onenodew5.Value = 0.0;
            OPCNodes.Add(onenodew5);

            OPCWriteNode onenodew6 = new OPCWriteNode();
            onenodew6.Name = "Torque";
            onenodew6.NodeId = "ns=3;s=Torque";
            onenodew6.Value = 0.0;
            OPCNodes.Add(onenodew6);

            var resultw1 = await OPCUAClient.WriteValues(OPCNodes);
            if (resultw1.Success == false)
            {
                MessageList.Items.Add("Process stop and initialize is not successful! " + resultw1.ErrorMessage);
            }
            else
            {
                MessageList.Items.Add("Process stop and initialize is successful.");
            }
        }

        private void ClearEvent_Click(object sender, RoutedEventArgs e)
        {
            MessageList.Items.Clear();
        }
    }
}
