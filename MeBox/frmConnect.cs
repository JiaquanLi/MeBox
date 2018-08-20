using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using clsBluetooth;
using System.Threading;
namespace MeBox
{
    public partial class frmConnect : Form
    {
        frmBluetoothDev frmDev = null;
        clsBlueTooth mobjBt = null;
        private clsDiagnostics mSerialPort;

        System.Windows.Forms.Timer objTimer;

        public clsDiagnostics SerialPort
        {
            set
            {
                mSerialPort = value;
            }
        }

        //*************************************************************//
        //Thread thDetectDev = null;

        public clsBlueTooth Bluetooth
        {
            set
            {
                mobjBt = value;
            }
        }
        public frmConnect()
        {
            InitializeComponent();
        }

        private void frmConnect_Load(object sender, EventArgs e)
        {

            objTimer = new System.Windows.Forms.Timer();
            this.objTimer.Tick += new System.EventHandler(this.Form_Timer_Tick);
            
            objTimer.Interval = 1000;
            objTimer.Enabled = true;

            
        }
        private void ConnectToDevice(object obj)
        {
            InTheHand.Net.BluetoothAddress tempDev = null;
            if (mobjBt == null)
            {
                MessageBox.Show("本机未找到蓝牙设备");
                return;
            }

            tempDev = UserSelectDev();
            if (tempDev == null) return;

            mobjBt.ConnectToDevice(tempDev);
            System.Threading.Thread.Sleep(3000);
            DateTime timeTo = DateTime.Now.AddSeconds(15);

            while (mobjBt.ConnectionStatue != clsBlueTooth.en_ConStatue.EN_SUCCEED && timeTo > DateTime.Now)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

            }

            if (mobjBt.ConnectionStatue != clsBlueTooth.en_ConStatue.EN_SUCCEED)
            {
                MessageBox.Show("连接蓝牙设备失败!!!");
                return;
            }

            this.Close();
            return;
        }

        private void 蓝牙设备ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ConnectToDevice(this);
            frmDev.Bluetooth = mobjBt;
            frmDev.ShowDialog();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private InTheHand.Net.BluetoothAddress UserSelectDev()
        {
            InTheHand.Windows.Forms.SelectBluetoothDeviceDialog dialog = new InTheHand.Windows.Forms.SelectBluetoothDeviceDialog();

            dialog.ShowRemembered = true;
            dialog.ShowAuthenticated = true;
            dialog.ShowUnknown = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedDevice.DeviceAddress;
            }

            return null;
        }

        private bool ReadINISettings(ref string strError)
        {
            string strPath;
            string strRes;

            ReadIniSettings objIniSettings;
            ComPortSettings_t mtyp_CommUUT = new ComPortSettings_t();
            objIniSettings = new ReadIniSettings();
            strPath = Application.StartupPath + "\\"  + "config.ini";
            if (System.IO.File.Exists(strPath) == false)
            {
                strError = " incorrect path " + strPath;
                return false;
            }

            strRes = objIniSettings.ReadPrivateProfileStringKey("Comport", "ComPort", strPath);
            if (strRes == "Error")
            {
                strError = " port setting";
                return false;
            }
            mtyp_CommUUT.PortNum = Convert.ToInt32(strRes);

            strRes = objIniSettings.ReadPrivateProfileStringKey("Comport", "Baud", strPath);
            if (strRes == "Error")
            {
                strError = " baud setting";
                return false;
            }
            mtyp_CommUUT.Baud = Convert.ToInt32(strRes);

            strRes = objIniSettings.ReadPrivateProfileStringKey("Comport", "Parity", strPath);
            if (strRes == "Error")
            {
                strError = " parity setting";
                return false;
            }
            mtyp_CommUUT.Parity = strRes;

            strRes = objIniSettings.ReadPrivateProfileStringKey("Comport", "DataBits", strPath);
            if (strRes == "Error")
            {
                strError = " data bits setting";
                return false;
            }
            mtyp_CommUUT.DataBits = Convert.ToInt32(strRes);

            strRes = objIniSettings.ReadPrivateProfileStringKey("Comport", "StopBits", strPath);
            if (strRes == "Error")
            {
                strError = " stop bits setting";
                return false;
            }
            mtyp_CommUUT.StopBits = Convert.ToInt32(strRes);

            serialSettings_t mySettings;

            try
            {
                mySettings.PortNum = mtyp_CommUUT.PortNum;
                mySettings.Baud = mtyp_CommUUT.Baud;
                mySettings.DataBits = mtyp_CommUUT.DataBits;
                switch (mtyp_CommUUT.Parity)
                {
                    case "n":
                    case "N":
                        mySettings.Parity = System.IO.Ports.Parity.None;
                        break;
                    case "e":
                    case "E":
                        mySettings.Parity = System.IO.Ports.Parity.Even;
                        break;
                    case "o":
                    case "O":
                        mySettings.Parity = System.IO.Ports.Parity.Odd;
                        break;
                    default:
                        mySettings.Parity = System.IO.Ports.Parity.None;
                        break;
                }

                switch (mtyp_CommUUT.StopBits)
                {
                    case 0:
                        mySettings.StopBits = System.IO.Ports.StopBits.None;
                        break;
                    case 1:
                        mySettings.StopBits = System.IO.Ports.StopBits.One;
                        break;
                    case 2:
                        mySettings.StopBits = System.IO.Ports.StopBits.Two;
                        break;
                    default:
                        mySettings.StopBits = System.IO.Ports.StopBits.None;
                        break;
                }

                mSerialPort.dSettings = mySettings;

            }
            catch (Exception ex)
            {
                try
                {
                    mSerialPort.dPortState = false;
                }
                catch (Exception exx)
                {
                    return false;
                }
            }

            

            return true;
        }

        private void Form_Timer_Tick(object sender, EventArgs e)
        {
            string strErr = "";

            objTimer.Enabled = false;
            if ( ReadINISettings(ref strErr) == false)
            {
                MessageBox.Show(strErr);
                objTimer.Enabled = true;
                return;
            }

            try
            {
                mSerialPort.dPortState = true;
            }
            catch(Exception ex)
            {
                if (MessageBox.Show("串口异常\r\n请设置后点击重试\r\n点取消退出程序", "异常", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question) == DialogResult.Retry)
                {
                    objTimer.Enabled = true;
                    return;
                }
                else
                {
                    System.Environment.Exit(0);
                }
            }

            objTimer.Enabled = false;
            this.Close();
        }
    }
        
}
