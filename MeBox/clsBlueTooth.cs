using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace clsBluetooth
{


    public class clsBlueTooth
    {
        public delegate void OnReciveCallback(BtData data);
        public event OnReciveCallback OnRecive;

        private const int SOH = 0x01;
        private const int EOT = 0x10;
        
        public enum en_ConStatue
        {
            EN_INVALID = -1,
            EN_CANCELLED,
            EN_FAILED,
            EN_SUCCEED
        }

        public struct BtData
        {
            public int time;
            public double angle_Yaw;
            public double angle_Pit;
            public double angle_Rol;

            public double angleAccelerat_Yaw;
            public double angleAccelerat_Pit;
            public double angleAccelerat_Rol;

            public double power;
            public double Intensity;
            public double Consumption;
            public double Save;

        }

        public en_ConStatue ConnectionStatue
        {
           
       
            get
            {
                lock (this)
                {
                    return mConnectStatus;
                }
            }
        
        }
        private en_ConStatue mConnectStatus;

        BluetoothClient mobj_BluetootClinet = null;
        private System.Net.Sockets.NetworkStream mBluetoothStream = null;
        private System.IO.StreamWriter mBluetoothStreamWriter = null;

        public clsBlueTooth()
        {
            mConnectStatus = en_ConStatue.EN_INVALID;
            mobj_BluetootClinet = new BluetoothClient();
        }

        ~clsBlueTooth()
        {
            Close();
        }

        private void CreatBluetoothClient()
        {
            if(mBluetoothStream != null)
            {
                mBluetoothStream.Close();
            }
            mobj_BluetootClinet.Close();
            mobj_BluetootClinet = new BluetoothClient();
        }

        public BluetoothDeviceInfo[] FindBlueToothDev()
        {
            return mobj_BluetootClinet.DiscoverDevices();
        }

        public bool ConnectToDevice(BluetoothAddress adr)
        {

            if (mobj_BluetootClinet.Connected == true) return true;

            //BluetoothDeviceInfo btDevToFind = null;
            //BluetoothDeviceInfo []btInfo = FindBlueToothDev();
            //foreach (BluetoothDeviceInfo btDev in btInfo)
            //{
            //    if(btDev.DeviceAddress.ToString() == strDeviceMac)
            //    {
            //        btDevToFind = btDev;
            //        break;
            //    }

            //}
            //// try to connect bluetooth server
            AsyncCallback cbk = new AsyncCallback(ConnectCallback);//connect information callbarck
            mobj_BluetootClinet.SetPin("1234");
            mobj_BluetootClinet.BeginConnect(adr, BluetoothService.SerialPort, cbk, null);
            return true;
            
        }

        public void DisConnectToDevice()
        {
            if(mConnectStatus == en_ConStatue.EN_SUCCEED)
            {
                mobj_BluetootClinet.Close();
            }
        }

        private bool Send(string strToSend)
        {

            mBluetoothStream = mobj_BluetootClinet.GetStream();
            mBluetoothStreamWriter = new System.IO.StreamWriter(mBluetoothStream);

            if (mBluetoothStreamWriter == null || mBluetoothStreamWriter.BaseStream.CanWrite == false)
            {
                return false;
            }

            try
            {
                mBluetoothStreamWriter.Write(strToSend);
                mBluetoothStreamWriter.Flush();
            }
            catch(Exception ex)
            {
                CreatBluetoothClient();
                return false;
            }
            return true;
        }

        public void ReadLoop()
        {
            string strRead = "";
            bool bSOH =false;
            bool bEOT =false;
            mBluetoothStream = null;

            mBluetoothStream = mobj_BluetootClinet.GetStream();
            mBluetoothStream.ReadTimeout = 100;
            if (mobj_BluetootClinet.Connected == false  || mBluetoothStream.CanRead == false)
            {
                return ;
            }

            //System.IO.StreamReader stmReader = new System.IO.StreamReader(mBluetoothStream,Encoding.Default);

            try
            {

                //char []cRead = new char[99];
                while (mBluetoothStream.CanRead)
                {
                    char cR =(char) mBluetoothStream.ReadByte();


                    if (cR == SOH)
                    {
                        bSOH = true;
                        continue;
                    }

                    if(bSOH == true)
                    {
                        if(cR == EOT)
                        {
                            char[] cSplit = { ';' };
                            string[] strTemps = strRead.Split(cSplit, StringSplitOptions.RemoveEmptyEntries);

                            if (strTemps.Length != 11)
                            {
                                strRead = "";
                                bEOT = false;
                                bSOH = false;
                                continue;
                            }

                            BtData data;
                            try
                            {
                                data.time = Convert.ToInt32(strTemps[0]);

                                data.angle_Yaw = Convert.ToDouble(strTemps[1]);
                                data.angle_Pit = Convert.ToDouble(strTemps[2]);
                                data.angle_Rol = Convert.ToDouble(strTemps[3]);

                                data.angleAccelerat_Yaw = Convert.ToDouble(strTemps[4]);
                                data.angleAccelerat_Pit = Convert.ToDouble(strTemps[5]);
                                data.angleAccelerat_Rol = Convert.ToDouble(strTemps[6]);

                                data.power = Math.Abs( Convert.ToDouble(strTemps[7]));
                                data.Intensity = Math.Abs(Convert.ToDouble(strTemps[8]));
                                data.Consumption = Math.Abs(Convert.ToDouble(strTemps[9]));
                                data.Save = Math.Abs(Convert.ToDouble(strTemps[10].Replace("\r\n", "")));
                                OnRecive(data);

                                bSOH = false;
                                strRead = "";
                                continue;
                            }
                            catch(Exception ex)
                            {
                                continue;
                            }
                        }

                        strRead += cR;

                    }
                    System.Threading.Thread.Sleep(10);

                }
                //stmReader.Read(cRead, 0, 99);
                //Console.WriteLine(stmReader.ReadLine());
                //return new string(cRead);
                return ;
                
            }
            catch(System.IO.IOException ex)
            {

            }

            return ;
        }


        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                mobj_BluetootClinet.EndConnect(ar);
                mConnectStatus = en_ConStatue.EN_SUCCEED;
               
            }
            catch(NullReferenceException ex)
            {
                mConnectStatus = en_ConStatue.EN_CANCELLED;
            }
            catch(ObjectDisposedException ex)
            {
                mConnectStatus = en_ConStatue.EN_CANCELLED;
            }
            catch(System.Net.Sockets.SocketException ex)
            {
                mConnectStatus = en_ConStatue.EN_FAILED;
                CreatBluetoothClient();
            }

            
        }
        private void Close()
        {
            if(mConnectStatus == en_ConStatue.EN_SUCCEED)
            {
                mobj_BluetootClinet.Close();
            }
            else
            {
                //CreatBluetoothClient();
            }
        }

    }
}
