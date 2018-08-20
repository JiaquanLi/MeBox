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
using ZedGraph;

namespace MeBox
{
    public partial class frm_Main : Form
    {
        //**************Object******************************//
        private frm_User mobjFrmUser;
        private  clsBlueTooth mobjBluetoothClient;
        private clsDiagnostics mobjSerial;
        private ComPortSettings_t mtyp_CommUUT;
        private st_UserInfo mUserInfo;
        System.Collections.Queue qBluetoothData;

        public enum en_PeopleAction
        {
            EN_INVALID = -1,
            STEP1,
            STEP2,
            STEP3,
            STEP4,
            STEP5,
            STEP6,
            STEP7
        }

        public enum en_ProgressBar
        {
            PGB_Evaluate,
            PGB_Intensity,
            PGB_Power
        }

        public enum en_LabelText
        {
            EN_Consumption,
            EN_Saving,
            EN_EvaluatePercent,
            EN_Time
        }

        private struct stData
        {
            public double dConsumption;
            public double dSave;
        }
        stData myData;
        //**********************GraphPane****************************//
        GraphPane myPane;
        PointPair pCurrentPoint1 = new PointPair();
        PointPair pCurrentPoint2 = new PointPair();
        PointPair pCurrentPoint3 = new PointPair();

        private PointPairList list1 = new PointPairList();
        private PointPairList list2 = new PointPairList();
        private PointPairList list3 = new PointPairList();
        LineItem curve1;
        LineItem curve2;
        LineItem curve3;
        int tickStart = 0;

        double valueXShow = 0;
        double valueYShow = -120;
        //*******************Show time*****************************//
        public delegate void DelegateShowText(en_LabelText lbl, string strText);
        public delegate void DelegateShowProgress(en_ProgressBar pgb, int value);
        //************************************************************//
        Thread thUpdateChart = null;
        Thread thWorkThread = null;
        Thread thTimeMonitor = null;

        private bool b_ExitThread;
        public frm_Main()
        {
            InitializeComponent();
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            //CheckForIllegalCrossThreadCalls = false;
            InitGraphPanel();
            InitObject();

            StartConnectFrom();
            StartUserInfoFrom();
            myData.dConsumption = 0;
            myData.dConsumption = 0;

        }
        private void btn_Back_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            StartUserInfoFrom();
            this.Visible = true;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            b_ExitThread = true;
            this.Close();
        }

        private void InitObject()
        {
            qBluetoothData = new System.Collections.Queue();
            myData = new stData();
            myData.dConsumption = 0;
            myData.dConsumption = 0;

            // frm object init

            mobjSerial = new clsDiagnostics();
            mobjSerial.OnRecive += new clsDiagnostics.OnReciveCallback(OnReceiveBluetoothMsg);
            //try
            //{
            //    mobjBluetoothClient = new clsBlueTooth();
            //    mobjBluetoothClient.OnRecive += new clsBlueTooth.OnReciveCallback(OnReceiveBluetoothMsg);
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show("本机没有蓝牙设备,点击确定后退出!!!","蓝牙错误",MessageBoxButtons.OK,MessageBoxIcon.Stop);

            //    //System.Environment.Exit(0);
            //}

            mobjFrmUser = new frm_User();
            thWorkThread = new Thread(new ThreadStart(WorkingThread));
            thWorkThread.IsBackground = true;
            thWorkThread.Start();

            thUpdateChart = new Thread(new ThreadStart(OnChartUpdate));
            thUpdateChart.IsBackground = true;
            thUpdateChart.Start();

            thTimeMonitor = new Thread(new ThreadStart(UpdateTime));
            thTimeMonitor.IsBackground = true;
            //variable
            mUserInfo = new st_UserInfo();
            
            b_ExitThread = false;
        }
        /// <summary>
        /// Graph 属性设置，已经界面显示
        /// </summary>
        private void InitGraphPanel()
        {        
            //获取引用
            myPane = zedGraphControl1.GraphPane;
            curve1 = myPane.AddCurve("YAW", list1, Color.Red, SymbolType.None);
            curve2 = myPane.AddCurve("PIT", list2, Color.Blue, SymbolType.None);
            curve3 = myPane.AddCurve("ROL", list3, Color.Lime, SymbolType.None);

            //设置标题
            myPane.Title.Text = "角速度信息";
            //设置X轴说明文字
            myPane.XAxis.Title.Text = "时间(秒)";
            //设置Y轴说明文字
            myPane.YAxis.Title.Text = "角速度(w)";
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);
            //myPane.Chart.Fill = new Fill(Color.White);

            myPane.YAxis.Scale.Min = -120;
            myPane.YAxis.Scale.Max = 120;
            myPane.YAxis.Scale.MinorStep = 10;
            myPane.YAxis.Scale.MajorStep = 30;
            myPane.YAxis.MajorGrid.IsVisible = true;

            myPane.XAxis.Type = ZedGraph.AxisType.Linear;
            myPane.XAxis.Scale.Min = 0;		
            myPane.XAxis.Scale.Max = 100;	
            myPane.XAxis.Scale.MinorStep = 5;
            myPane.XAxis.Scale.MajorStep = 20;
            myPane.XAxis.MajorGrid.IsVisible = true;

            //改变轴的刻度
            zedGraphControl1.AxisChange();
            //保存开始时间
            tickStart = Environment.TickCount;
            zedGraphControl1.IsShowPointValues = true;
            zedGraphControl1.Invalidate();         
        }

        private void StartConnectFrom()
        {
            frmConnect mobjFrmCon;
            mobjFrmCon = new frmConnect();
            //mobjFrmCon.Bluetooth = mobjBluetoothClient;
            mobjFrmCon.SerialPort = mobjSerial;
            mobjFrmCon.ShowDialog();
            
        }
        private void StartUserInfoFrom()
        {
            mobjFrmUser.ShowDialog(this);
            mUserInfo = mobjFrmUser.UserInfo;
        }

        private void UpdateTime()
        {
            int iTime = 0;
            while (true)
            {
                DeUpdateLabelText(en_LabelText.EN_Time,(iTime++).ToString() );
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// main working thread
        /// </summary>
        private void WorkingThread()
        {
           

            while (mobjSerial.dPortState == false)
            {
                //if (mobjSerial.dPortState == false) mobjSerial.dPortState = true;

                Thread.Sleep(2000);
            }

            thTimeMonitor.Start();
            mobjSerial.ReadLoop();

        }
        /// <summary>
        /// chart update thread
        /// </summary>
        private void OnChartUpdate()
        {
            clsBlueTooth.BtData data;
            int iXStep = 1; //milSec
            int i = 0;

            double d_PreRol = -1;
            while (true)
            {
                lock (qBluetoothData.SyncRoot)
                {
                    if(qBluetoothData.Count> 0)
                    {
                        data = (clsBlueTooth.BtData)qBluetoothData.Dequeue();
                        UpdateChart(data, iXStep);

                        myData.dConsumption += data.Consumption/10000;
                        myData.dSave += data.Save/10000;
                        DeUpdateLabelText(en_LabelText.EN_Consumption, myData.dConsumption.ToString("0.00"));
                        DeUpdateLabelText(en_LabelText.EN_Saving, myData.dSave.ToString("0.00"));

                        //if (i == 7) i = 0;
                        UpdateImagePeopleImage(d_PreRol, data.angle_Rol);
                        DeUpdateProgress(en_ProgressBar.PGB_Evaluate, (int)(myData.dConsumption/10));
                        DeUpdateProgress(en_ProgressBar.PGB_Intensity, (int)data.Intensity);
                        DeUpdateProgress(en_ProgressBar.PGB_Power, (int)data.power);

                        d_PreRol = data.angle_Rol;

                    }

                }               
                Thread.Sleep(5);
            }
        }

        private void UpdateChart(clsBlueTooth.BtData data, double milSec)
        {
            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
            {
                return;
            }

            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            if (curve == null)
            {
                return;
            }
            IPointListEdit list = curve.Points as IPointListEdit;
            if (list == null)
            {
                return;
            }

            valueXShow += (milSec);
            double x = valueXShow;

            try
            {
                pCurrentPoint1.X = x;
                pCurrentPoint1.Y = data.angle_Yaw;

                if (pCurrentPoint1.Y > 120) pCurrentPoint1.Y = 119;
                if (pCurrentPoint1.Y < -120) pCurrentPoint1.Y = -119;

                pCurrentPoint2.X = x;
                pCurrentPoint2.Y = data.angle_Pit;
                if (pCurrentPoint2.Y > 120) pCurrentPoint2.Y = 110;
                if (pCurrentPoint2.Y < -120) pCurrentPoint2.Y = -119;

                pCurrentPoint3.X = x;
                pCurrentPoint3.Y = data.angle_Rol;
                if (pCurrentPoint3.Y > 120) pCurrentPoint3.Y = 100;
                if (pCurrentPoint3.Y < -120) pCurrentPoint3.Y = -119;

            }
            catch (Exception ex)
            {
                return;
            }

            list1.Add(pCurrentPoint1);
            list2.Add(pCurrentPoint2);
            list3.Add(pCurrentPoint3);

            if (valueXShow >= 100)
            {
                list1.Clear();
                list2.Clear();
                list3.Clear();
                valueXShow = 0;

            }
            zedGraphControl1.Invalidate();
        }
        private void UpdateImagePeopleImage(double dPreRol,double dCurRol)
        {
            int iAction = -1;
            if (dPreRol == -1) return;

            if (dCurRol > 0 && dCurRol <= 15)
            {
                if (dCurRol > dPreRol)
                {
                    iAction = 0;
                }
                else if(dCurRol < dPreRol)
                {
                    iAction = 6;
                }
            }
            else if (dCurRol > 15 && dCurRol <= 30)
            {
                if (dCurRol > dPreRol)
                {
                    iAction = 1;
                }
                else if (dCurRol < dPreRol)
                {
                    iAction = 5;
                }
            }
            else if (dCurRol > 30 && dCurRol <= 60)
            {
                if (dCurRol > dPreRol)
                {
                    iAction = 2;
                }
                else if (dCurRol < dPreRol)
                {
                    iAction = 4;
                }
            }
            else if (dCurRol > 60 && dCurRol <= 100)
            {
                iAction = 3;
            }

            if (iAction == -1) return;

            pbx_People.Image.Dispose();
            pbx_People.Image = imglst_Peple.Images[(int)iAction];
        }
        private void DeUpdateLabelText(en_LabelText lbl,string strText)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    DelegateShowText dgateMsg = new DelegateShowText(ShowLableText);
                    this.Invoke(dgateMsg, new object[] {lbl, strText });
                }
                else
                {
                    ShowLableText(lbl,strText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DeUpdateProgress(en_ProgressBar pgb,int value)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    DelegateShowProgress dgateMsg = new DelegateShowProgress(ShowProgressBar);
                    this.Invoke(dgateMsg, new object[] { pgb, value });
                }
                else
                {
                    ShowProgressBar(pgb, value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowLableText(en_LabelText lbl,string strText)
        {
            switch(lbl)
            {
                case en_LabelText.EN_EvaluatePercent:
                    lbl_EvaluatePercent.Text = strText;
                    break;
                case en_LabelText.EN_Consumption:
                    lbl_Consumption.Text = strText;
                    break;
                case en_LabelText.EN_Saving:
                    lbl_Saving.Text = strText;
                    break;
                case en_LabelText.EN_Time:
                    lbl_Time.Text = strText;
                    break;
                default:
                    break;
            }
            
        }

        private void ShowProgressBar(en_ProgressBar pgb ,int value)
        {



            if (value < 0 || value > 100) return;
            Random rd = new Random();
            switch (pgb)
            {
                case en_ProgressBar.PGB_Evaluate:
                    if (value < 0) value = 0;
                    if (value > 100) value = 100;
                    pgb_Evaluate.Value = value;
                    DeUpdateLabelText(en_LabelText.EN_EvaluatePercent, value.ToString() + "%");
                    break;
                case en_ProgressBar.PGB_Intensity:

                    if (value > 1000000)
                    {
                        value = rd.Next(67,90);
                    }else if(value > 100000 && value < 1000000)
                    {
                        value = rd.Next(34, 66);
                    }
                    else
                    {
                        value = rd.Next(10, 33);
                    }
                    pgb_Intensity.Value = value;
                    break;
                case en_ProgressBar.PGB_Power:
                    if (value > 200)
                    {
                        value = rd.Next(67, 90);
                    }
                    else if (value > 100 && value < 200)
                    {
                        value = rd.Next(34, 66);
                    }
                    else
                    {
                        value = rd.Next(10, 33);
                    }
                    pgb_Power.Value = value;
                    break;
                default:
                    break;

            }
        
        }

        /// <summary>
        /// bluetooth message call back funtion
        /// </summary>
        /// <param name="data"></param>
        /// bluetooth message
        private void OnReceiveBluetoothMsg(clsBlueTooth.BtData data)
        {
            lock (qBluetoothData.SyncRoot)
            {
                qBluetoothData.Enqueue(data);

            }
            Console.WriteLine(data.angle_Pit + "  " + data.angle_Rol + "  " + data.angle_Yaw);
        }

    }
}
