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
    public partial class frmBluetoothDev : Form
    {

        private InTheHand.Net.Sockets.BluetoothDeviceInfo[] devInfo = null;
        clsBlueTooth mobjBt = null;

        public clsBlueTooth Bluetooth
        {
            set
            {
                mobjBt = value;
            }
        }
        public frmBluetoothDev()
        {
            InitializeComponent();
        }

        private void frmBluetoothDev_Load(object sender, EventArgs e)
        {
            FindBluetoothDev();
        }
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            FindBluetoothDev();
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            //string strMac = "";
            //strMac = devInfo[lstb_Dev.SelectedIndex].DeviceAddress.ToString();
            lbl_Message.Text = "连接设备中:" + devInfo[lstb_Dev.SelectedIndex].DeviceName +"  " + devInfo[lstb_Dev.SelectedIndex].DeviceAddress.ToString(); 
            mobjBt.ConnectToDevice(devInfo[lstb_Dev.SelectedIndex].DeviceAddress);

            System.Threading.Thread.Sleep(10000);
            DateTime timeTo = DateTime.Now.AddSeconds(1000);

            while(mobjBt.ConnectionStatue != clsBlueTooth.en_ConStatue.EN_SUCCEED)
            {            
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

            }

            if(mobjBt.ConnectionStatue != clsBlueTooth.en_ConStatue.EN_SUCCEED)
            {
                lbl_Message.Text = "连接设备失败";
                lbl_Message.BackColor = Color.Red;
                return;
                
            }

            this.Hide();
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Exit_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
        private void FindBluetoothDev()
        {
            lbl_Message.Text = "寻找周围蓝牙设备...";
            lstb_Dev.Items.Clear();
            if (mobjBt == null) return;

            devInfo = mobjBt.FindBlueToothDev();

            foreach(InTheHand.Net.Sockets.BluetoothDeviceInfo dev in devInfo)
            {
                lstb_Dev.Items.Add("名称: " + dev.DeviceName + "  " + "Mac: " + dev.DeviceAddress + "\r\n");
            }
        }


    }
}
