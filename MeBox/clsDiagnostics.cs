using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices; // DllImport
using System.Diagnostics;
using System.Windows.Forms;
using clsBluetooth;

namespace MeBox
{
    public struct serialSettings_t
    {
        public int PortNum;
        public int Baud;
        public System.IO.Ports.Parity Parity;
        public int DataBits;
        public System.IO.Ports.StopBits StopBits;  //Int32
    }

    public struct ComPortSettings_t
    {
        public Int32 PortNum;
        public Int32 Baud;
        public string Parity;
        public Int32 DataBits;
        public Int32 StopBits;
    }
    public class clsDiagnostics
    {
        private const string ACK = "\x06";
        public const string NUL = "\x00";
        public const string CR = "\x0d";
        public const string LF = "\x0a";
        public const string CRLF = "\x0d\x0a";
        public const string SYN = "\x16";
        public const string DC1 = "\x11";
        public const string SPACE = "\x20";
        private const int SOH = 0x01;
        private const int EOT = 0x10;

        public delegate void OnReciveCallback(clsBlueTooth.BtData data);
        public event OnReciveCallback OnRecive;

        #region Variables

        private int m_iIntercharacterDelay = 10;

        private int m_iCommPortNumber = 1;
        private long m_lCommBaudRate = 9600;
        private System.IO.Ports.Parity m_pCommParity = System.IO.Ports.Parity.None;
        private int m_iCommDataBits = 8;
        private System.IO.Ports.StopBits m_sCommStopBits = System.IO.Ports.StopBits.One;

        private System.IO.Ports.SerialPort m_serport = null;
        //private System.IO.Ports.SerialPort Serial = null;

        private bool m_bShowErrors = true;
        private string mstr_error;

        // private System.Windows.Forms.RichTextBox m_tTerminalWindow;

        static readonly string[] LowNames =
        {
        "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL",
        "BS", "HT", "LF", "VT", "FF", "CR", "SO", "SI",
        "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB",
        "CAN", "EM", "SUB", "ESC", "FS", "GS", "RS", "US"
        };

        //private SerialPort m_serport = null;
        private serialSettings_t m_settings;
        // private m_Utils Utils;

        public const string BEL = "\x07";

        #endregion

        #region Constructor

        /// <summary>
        /// Overload Constructor
        /// </summary>
        /// <param name="SerialPortNumber"></param>
        /// <param name="SerialBaudRate"></param>

        public clsDiagnostics()
        {
            m_serport = new System.IO.Ports.SerialPort();
            // m_tTerminalWindow = new System.Windows.Forms.RichTextBox();
        }

        /// <summary>
        /// Overload Constructor
        /// </summary>
        /// <param name="SerialPortNumber"></param>
        /// <param name="SerialBaudRate"></param>
        public clsDiagnostics(int SerialPortNumber, long SerialBaudRate)
        {
            m_iCommPortNumber = SerialPortNumber;
            m_lCommBaudRate = SerialBaudRate;

            m_serport = new System.IO.Ports.SerialPort();
            //m_tTerminalWindow = new System.Windows.Forms.RichTextBox();
        }

        /// <summary>
        /// Overload Constructor
        /// </summary>
        /// <param name="SerialPortNumber"></param>
        /// <param name="SerialBaudRate"></param>
        /// <param name="SerialDataBits"></param>
        /// <param name="SerialParity"></param>
        /// <param name="SerialStopBits"></param>
        public clsDiagnostics(int SerialPortNumber, long SerialBaudRate, System.IO.Ports.Parity SerialParity, int SerialDataBits, System.IO.Ports.StopBits SerialStopBits)
        {
            m_iCommPortNumber = SerialPortNumber;
            m_lCommBaudRate = SerialBaudRate;
            m_pCommParity = SerialParity;
            m_iCommDataBits = SerialDataBits;
            m_sCommStopBits = SerialStopBits;

            m_serport = new System.IO.Ports.SerialPort();
            // m_tTerminalWindow = new System.Windows.Forms.RichTextBox();
        }

        /// <summary>
        /// Overload Constructor
        /// </summary>
        /// <param name="SerialPortNumber"></param>
        /// <param name="SerialBaudRate"></param>
        /// <param name="SerialDataBits"></param>
        /// <param name="SerialParity"></param>
        /// <param name="SerialStopBits"></param>
        /// <param name="IntercharacterDelay"></param>
        public clsDiagnostics(int SerialPortNumber, long SerialBaudRate, System.IO.Ports.Parity SerialParity, int SerialDataBits, System.IO.Ports.StopBits SerialStopBits, int IntercharacterDelay)
        {
            m_iCommPortNumber = SerialPortNumber;
            m_lCommBaudRate = SerialBaudRate;
            m_pCommParity = SerialParity;
            m_iCommDataBits = SerialDataBits;
            m_sCommStopBits = SerialStopBits;

            m_iIntercharacterDelay = IntercharacterDelay;

            m_serport = new System.IO.Ports.SerialPort();
            // m_tTerminalWindow = new System.Windows.Forms.RichTextBox();
        }

        /// <summary>
        /// Overload Constructor
        /// </summary>
        /// <param name="SerialPortNumber"></param>
        /// <param name="SerialBaudRate"></param>
        /// <param name="SerialDataBits"></param>
        /// <param name="SerialParity"></param>
        /// <param name="SerialStopBits"></param>
        /// <param name="IntercharacterDelay"></param>
        public clsDiagnostics(int SerialPortNumber, long SerialBaudRate, System.IO.Ports.Parity SerialParity, int SerialDataBits, System.IO.Ports.StopBits SerialStopBits, int IntercharacterDelay, System.Windows.Forms.RichTextBox TerminalWindow)
        {
            m_iCommPortNumber = SerialPortNumber;
            m_lCommBaudRate = SerialBaudRate;
            m_pCommParity = SerialParity;
            m_iCommDataBits = SerialDataBits;
            m_sCommStopBits = SerialStopBits;

            m_iIntercharacterDelay = IntercharacterDelay;

            m_serport = new System.IO.Ports.SerialPort();
            // m_tTerminalWindow = TerminalWindow;
        }

        #endregion

        #region Properties

        public serialSettings_t dSettings
        {
            get { return m_settings; }
            set
            {
                m_settings = value;
                m_iCommPortNumber = value.PortNum;
                m_lCommBaudRate = value.Baud;
                m_pCommParity = value.Parity;
                m_iCommDataBits = value.DataBits;
                m_sCommStopBits = value.StopBits;
            }
        }

        public bool dPortState
        {
            get { return m_serport.IsOpen; }
            set
            {
                if (value == true)
                {
                    if (m_serport.IsOpen == true) Serial_END(m_serport);
                    //SetPortProperties();
                    //Serial_INIT(m_serport);
                    if (m_serport.IsOpen == false) Serial_INIT(m_serport);
                }
                else
                {
                    if (m_serport.IsOpen == true)
                        Serial_END(m_serport);
                }
            }
        }

        /*
                public bool cCtsState
                {
                    get
                    {
                        bool st;
                        if (m_serport.IsOpen == false)
                        {
                            SetPortProperties();
                            m_serport.Open();
                        }
                        st = m_serport.CtsHolding;
                        //m_ComPort.Close()
                        return st;
                    }
                }

                public bool cRtsEnable
                {
                    get
                    {
                        bool st;
                        if (m_serport.IsOpen == false)
                        {
                            SetPortProperties();
                            m_serport.Open();
                        }
                        st = m_serport.RtsEnable;
                        //'m_ComPort.Close()
                        return st;
                    }
                    set
                    {
                        m_serport.RtsEnable = value;
                    }
                }
        */
        /// <summary>
        /// Get or Set the ability to show communication error messages
        /// </summary>
        public bool ShowErrors
        {
            get
            {
                return m_bShowErrors;
            }
            set
            {
                m_bShowErrors = value;
            }
        }

        /// <summary>
        /// Get error messages
        /// </summary>
        public string err
        {
            get
            {
                return mstr_error;
            }
        }

        /// <summary>
        /// Get or Set the Comm Port Number
        /// </summary>
        public int CommPortNumber
        {
            get
            {
                return m_iCommPortNumber;
            }
            set
            {
                m_iCommPortNumber = value;
            }
        }

        /// <summary>
        /// Get or Set the Comm Port Baud Rate
        /// </summary>
        public long CommBaudRate
        {
            get
            {
                return m_lCommBaudRate;
            }
            set
            {
                m_lCommBaudRate = value;
            }
        }

        /// <summary>
        /// Get or Set the Comm Parity
        /// </summary>
        public System.IO.Ports.Parity CommParity
        {
            get
            {
                return m_pCommParity;
            }
            set
            {
                m_pCommParity = value;
            }
        }

        /// <summary>
        /// Get or Set the Comm DataBits
        /// </summary>
        public int CommDataBits
        {
            get
            {
                return m_iCommDataBits;
            }
            set
            {
                m_iCommDataBits = value;
            }
        }

        /// <summary>
        /// Get or Set the Comm Stop Bits
        /// </summary>
        public System.IO.Ports.StopBits CommStopBits
        {
            get
            {
                return m_sCommStopBits;
            }
            set
            {
                m_sCommStopBits = value;
            }
        }

        /// <summary>
        /// Get or Set the Intercharacter Delay
        /// </summary>
        public int IntercharacterDelay
        {
            get
            {
                return m_iIntercharacterDelay;
            }
            set
            {
                m_iIntercharacterDelay = value;
            }
        }

        /// <summary>
        /// Get or Set the Terminal Window to control
        /// </summary>
        //public System.Windows.Forms.RichTextBox TerminalWindow
        //{
        //    get
        //    {
        //        return m_tTerminalWindow;
        //    }
        //    set
        //    {
        //        m_tTerminalWindow = value;
        //    }
        //}

        #endregion

       #region General

        public bool HSM_Send_Menu_Command(char type, string command, string parameters, char terminator1, char terminator2, ref string buffer)
        {
            int x;
            int value;
            string echo = "";
            //System.IO.Ports.SerialPort Serial = new System.IO.Ports.SerialPort();
            bool Check;

            //Check = Serial_INIT(Serial);
            //if (!Check) return false;

            Serial_FLUSH(m_serport);

            Serial_Write_Byte(m_serport, 22);   //<SYN>
            Serial_Write_Byte(m_serport, (byte)type);
            Serial_Write_Byte(m_serport, 13);   //<CR>

            Serial_Write_String(m_serport, command);

            //If there are no parameters just skip this step
            if (parameters != null && parameters.Length > 0)
                Serial_Write_String(m_serport, parameters);

            if ((int)terminator1 != 0)
                Serial_Write_Byte(m_serport, (byte)terminator1);

            if ((int)terminator2 != 0)
                Serial_Write_Byte(m_serport, (byte)terminator2);

            //Wait for Echo
            echo = command + parameters;
            Check = Serial_Wait_For_String(m_serport, echo, 3000);
            if (!Check)
            {
                Serial_END(m_serport);
                return false;
            }

            //Check for ACK
            Check = false;
            x = 0;
            DateTime to = DateTime.Now.AddMilliseconds(2000);
            while (to > DateTime.Now)
            {
                //Only read if there is something in the buffer
                if (m_serport.IsOpen && m_serport.BytesToRead > 0)
                {
                    value = m_serport.ReadByte();
                    //UpdateTerminal_RX(value);

                    if (value == 6) //ACK
                    {
                        Check = true;
                        break;
                    }
                    else if (value == 21) //NAK
                    {
                        Check = false;
                        break;
                    }
                    else if (value == 5) //ENQ
                    {
                        Check = false;
                        break;
                    }
                    else
                    {
                        //If the first character returned is a colon then ignore it
                        if (x != 0 || value != 58)
                        {
                            buffer = buffer + (char)value;
                            x++;
                        }
                    }
                }
            }

            //Wait for 2mS and clear the buffer
            DelayMS(2);
            Serial_FLUSH(m_serport);

            //Serial_END(m_serport);

            return Check;
        }

        public bool HSM_FACTST(string device, string command, string parameters, ref string buffer)
        {
            string myparams = "";

            myparams = device + ":" + command + ":" + parameters;

            return HSM_Send_Menu_Command('Y', "FACTST", myparams, '.', (char)0, ref buffer);
        }

        public bool HSM_CALSET(string device, string paramid, string bytes)
        {
            string myparams = "";
            string dummy = "";

            myparams = device + ":" + paramid + ":" + bytes;

            return HSM_Send_Menu_Command('Y', "CALSET", myparams, '.', (char)0, ref dummy);
        }

        public bool HSM_CALGET(string device, string paramid, ref string buffer)
        {
            string myparams = "";

            myparams = device + ":" + paramid;

            return HSM_Send_Menu_Command('Y', "CALGET", myparams, '.', (char)0, ref buffer);
        }

        public bool HSM_CALSAV()
        {
            string dummy = "";

            return HSM_Send_Menu_Command('Y', "CALSAV", "", '.', (char)0, ref dummy);
        }

        public bool HSM_GPIO_SET(int index, int value)
        {
            string setparameters = "";
            string buffer = "";
            bool Check;

            //Make sure the value is either a 1 or a 0
            if (value != 0) value = 1;

            setparameters = String.Format("{0:X2}", index) + ":" + value.ToString();

            Check = HSM_FACTST("gpio", "5", setparameters, ref buffer);

            return Check;
        }

        public bool HSM_GPIO_GET(int index, ref int value)
        {
            string setparameters = "";
            string buffer = "";
            bool Check;

            //Make sure the value is either a 1 or a 0
            if (value != 0) value = 1;

            setparameters = String.Format("{0:X2}", index); //+ ":" + value.ToString();

            Check = HSM_FACTST("gpio", "6", setparameters, ref buffer);
            if (Check)
                value = ConvertStringToDecimal(FlipHexString(buffer));
            return Check;
        }
        public bool GetBarcodeString(ref string buffer)
        {
            bool check = false;

            buffer = "";
            check = ReadInBuffer(ref buffer, CR);

            return check;
        }
        public bool HSM_SET_SERIAL(string serial)
        {
            string temp = "";
            bool Check;

            if (serial.Length != 10) return false;

            for (int x = 0; x < 10; x++)
                temp = temp + String.Format("{0:X2}", (int)serial[x]);

            //Append the extra 6 bytes of space not used
            temp = temp + "000000000000";

            Check = HSM_CALSET("system", "1", temp);

            return Check;
        }

        public bool HSM_LEGACY_SET_SERIAL(string serial)
        {
            string temp = "";
            bool Check;

            if (serial.Length != 10) return false;

            Check = HSM_Send_Menu_Command('Y', "SERNUM", serial, '.', (char)0, ref temp);

            return Check;
        }

        public bool HSM_LEGACY_GET_SERIAL(ref string serial)
        {
            bool Check;
            string temp = "";

            serial = "";

            //Get Serial Number
            Check = HSM_Send_Menu_Command('Y', "SERNUM", "", '?', '.', ref temp);

            //Validate Serial # Size
            if (!Check || temp.Length != 10)
                return false;

            serial = temp;

            return true;
        }

        public bool HSM_GET_SERIAL(ref string serial)
        {
            bool Check;
            string temp = "";

            serial = "";

            //Get Serial Number
            Check = HSM_CALGET("system", "1", ref temp);

            //Validate Serial # Size
            if (!Check || temp.Length != 32)
                return false;

            //Convert Serial String from Hex to Characters
            for (int x = 0; x < 10; x++)
            {
                serial = serial + (char)ConvertHexCharToDecimal(temp[x * 2], temp[(x * 2) + 1]);
            }

            return true;
        }

        /*
        public bool HSM_GET_SWNUM(ref string swnum)
        {
            return HSM_Send_Menu_Command('M', "REV_SW", "", '?', '.', ref swnum);
        }
        */
        public bool RTSCTS_NOTIMEOUT()
        {
            string temp = "";
            return HSM_Send_Menu_Command('M', "232CTS1", "", '.', (char)0, ref temp);
        }

        public bool HSM_GET_SWNUM(ref string swnum)
        {
            return HSM_Send_Menu_Command('M', "REV_WA", "", '?', '.', ref swnum);
        }

        public bool HSM_SET_BenchNumber(string BenchNumber)
        {
            bool Check;
            Check = false;
            Check = HSM_CALSET("system", "0", BenchNumber);
            return Check;
        }

        public bool HSM_GET_BenchNumber(ref string BenchNumber)
        {
            bool Check;
            string temp = "";

            BenchNumber = "";
            //Get benchNumber
            Check = HSM_CALGET("system", "0", ref temp);
            //Validate Bench # Data
            if (!Check || temp.Length != 64)
                return false;
            if (!IsAlphaNumeric(temp))
                return false;
            //Convert Serial String from Hex to Characters
            BenchNumber = temp.Substring(0, 17);
            return true;
        }

        public bool IsAlphaNumeric(string BenchNumber)
        {
            int x = 0;
            char c;
            bool check = false;
            for (x = 0; x < BenchNumber.Length; x++)
            {
                c = (char)BenchNumber[x];

                if ((c >= Convert.ToChar("A") && c <= Convert.ToChar("Z") || TryCatchNumber(BenchNumber[x].ToString()) == true))
                {
                    check = true;
                }
                else
                    check = false;

            }
            return check;
        }

        public bool TryCatchNumber(string number)
        {
            bool check = false;
            int x;
            try
            {
                x = int.Parse(number);
                check = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: TryCatchNumber fail" + "\n" + ex.Message);
                check = false;
            }

            return check;
        }

        public bool HSM_GET_SCANNER_BTADDRESS(ref string address)
        {
            bool Check;
            string temp = "";

            address = "";

            //Get Serial Number
            Check = HSM_Send_Menu_Command('M', "BT_LDA", "", '.', (char)0, ref temp);

            //Validate Address Size
            if (!Check || temp.Length != 12)
                return false;

            address = temp;

            return true;
        }

        public bool HSM_GET_SCANNER_BTCONNECT(ref string address)
        {
            bool Check;
            string temp = "";

            address = "";

            //Get Serial Number
            Check = HSM_Send_Menu_Command('M', "BT_ADR", "", '?', '.', ref temp);

            //Validate Address Size
            if (!Check || temp.Length != 12)
                return false;

            address = temp;

            return true;
        }

        public bool HSM_SET_SCANNER_BTCONNECT(string address)
        {
            bool Check;
            string temp = "";

            //Validate Address Size
            if (address.Length != 12)
                return false;

            //Get Serial Number
            Check = HSM_Send_Menu_Command('M', "BT_ADR", address, '.', (char)0, ref temp);

            return Check;
        }

        public bool HSM_GET_CRADLE_BTADDRESS(ref string address)
        {
            bool Check;
            string temp = "";

            address = "";

            //Get Serial Number
            Check = HSM_Send_Menu_Command('M', "BASLDA", "", '.', (char)0, ref temp);

            //Validate Address Size
            if (!Check || temp.Length != 12)
                return false;

            address = temp;

            return true;
        }

        public bool HSM_FACT_DEFAULT()
        {
            bool Check;
            string temp = "";
            //Get Serial Number
            Check = HSM_Send_Menu_Command('M', "DEFALT", "", '.', (char)0, ref temp);

            //Validate Address Size
            if (!Check)
                return false;
            return true;
        }

        public bool HSM_RESET()
        {
            //System.IO.Ports.SerialPort Serial = new System.IO.Ports.SerialPort();
            bool Check;

            //Check = Serial_INIT(Serial);
            //if (!Check) return false;

            // Serial_FLUSH(Serial);
            Serial_FLUSH(m_serport);

            //Send a CR to clear things
            Serial_Write_Byte(m_serport, 13);
            Serial_Write_Byte(m_serport, 22);   //<SYN>
            Serial_Write_Byte(m_serport, (byte)'M');
            Serial_Write_Byte(m_serport, 13);   //<CR>
            Serial_Write_String(m_serport, "RESET_");
            Serial_Write_Byte(m_serport, (byte)'.');

            //Wait for 2mS and clear the buffer
            DelayMS(2);
            Serial_FLUSH(m_serport);

            //Serial_END(Serial);

            return true;
        }
        //+4300 
        public bool HSM4300_EnterFlipperTestMode()
        {
            bool Check;
            string temp = "";

            //Check = HSM_FACTST("gpio", "5", setparameters, ref buffer);

            Check = HSM_FACTST("flipper0", "0", "1", ref temp);

            return Check;
        }
        public bool HSM4300_ExitFlipperTestMode()
        {
            bool Check;
            string temp = "";

            //Check = HSM_FACTST("gpio", "5", setparameters, ref buffer);

            Check = HSM_FACTST("flipper0", "0", "0", ref temp);

            return Check;

        }
        public bool HSM4300_EnterODTestMode()
        {
            bool Check;
            string temp = "";

            //Check = HSM_FACTST("gpio", "5", setparameters, ref buffer);

            Check = HSM_FACTST("object_detect", "0", "1", ref temp);

            return Check;
        }
        public bool HSM4300_ExitODTestMode()
        {
            bool Check;
            string temp = "";

            //Check = HSM_FACTST("gpio", "5", setparameters, ref buffer);

            Check = HSM_FACTST("object_detect", "0", "0", ref temp);

            return Check;

        }
        public bool HSM4300_EnterOpenLoopMode()
        {
            bool Check;

            Check = HSM_CALSET("flipper0", "9E", "01");
            return Check;
        }
        public bool HSM4300_ExitOpenLoopMode()
        {
            bool Check;

            Check = HSM_CALSET("flipper0", "9E", "00");
            return Check;
        }


        #region Serial Functions

        private bool Serial_INIT(System.IO.Ports.SerialPort Serial)
        {
            Serial.BaudRate = (int)m_lCommBaudRate;
            Serial.PortName = "COM" + m_iCommPortNumber.ToString();
            Serial.Parity = m_pCommParity;
            Serial.DataBits = m_iCommDataBits;

            if (m_sCommStopBits == System.IO.Ports.StopBits.None)
                Serial.StopBits = System.IO.Ports.StopBits.One;
            else
                Serial.StopBits = m_sCommStopBits;

            try
            {
                if (!Serial.IsOpen)
                    Serial.Open();
            }
            catch (Exception ex)
            {
                //clsGlobals.log.Error("", ex);
                mstr_error = "Error: Serial open" + "\n" + ex.Message;
                if (m_bShowErrors)
                    throw new ArgumentOutOfRangeException("串口异常不存在: " + Serial.PortName); // throw  //System.Windows.Forms.MessageBox.Show("Error: Tried to open an already open Comm Port", "Comm Port Already Open", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void Serial_END(System.IO.Ports.SerialPort Serial)
        {
            try
            {
                if (Serial.IsOpen)
                    Serial.Close();
            }
            catch (Exception ex)
            {
                //clsGlobals.log.Error("", ex);
                mstr_error = "Error: Serial Close" + "\n" + ex.Message;
                if (m_bShowErrors)
                    Console.WriteLine("Error: Tried to close an already closed Comm Port");
            }
        }
        public void RefreshBuffer()
        {
            Serial_FLUSH(m_serport);
        }
        private void Serial_FLUSH(System.IO.Ports.SerialPort Serial)
        {
            try
            {
                if (Serial.IsOpen)
                {
                    Serial.ReadExisting();
                    Serial.DiscardInBuffer();
                }
            }
            catch (Exception ex)
            {
                //clsGlobals.log.Error("", ex);
                mstr_error = "Error: " + "\n" + ex.Message;
                if (m_bShowErrors)
                    Console.WriteLine("Error: Tried to flush a buffer that was already open");
            }
        }

        public string ReadInBuffer(double tmout, string terminator)
        {
            string commBuff;
            bool commRead;
            long timeTotal;
            long timeRef;

            Debug.Print("Entering ReadInBuffer: " + m_serport.PortName);
            commBuff = "";
            commRead = false;
            try
            {
                //Convert it to 100 nanosecond increments
                timeTotal = Convert.ToInt64(tmout * System.TimeSpan.TicksPerSecond);
                timeRef = (System.DateTime.Now).Ticks;
                while (((System.DateTime.Now).Ticks - timeRef) < timeTotal)
                {
                    System.Windows.Forms.Application.DoEvents();
                    if (m_serport.BytesToRead > 0)
                    {
                        while (m_serport.BytesToRead > 0)
                        {
                            commBuff = commBuff + m_serport.ReadExisting();
                            if (commBuff.ToLower().LastIndexOf(terminator.ToLower()) != -1)
                            {
                                commRead = true;
                                break;
                            }
                        }
                    }
                    if (commRead == true)
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            //Console.WriteLine(commBuff);
            Debug.Print(commBuff);
            Debug.Print("Exiting ReadInBuffer: " + m_serport.PortName);

            return commBuff;
        }
        public bool ReadInBuffer(ref string strbuffer, string terminator)
        {
            string commBuff;
            bool commRead;
            commRead = false;
            commBuff = "";

            if (m_serport.BytesToRead > 0)
            {
                while (m_serport.BytesToRead > 0)
                {
                    commBuff = commBuff + m_serport.ReadExisting();
                    if (commBuff.ToLower().LastIndexOf(terminator.ToLower()) != -1)
                    {
                        commRead = true;
                        break;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            strbuffer = commBuff;
            return commRead;
        }
        private bool Serial_Write_String(System.IO.Ports.SerialPort Serial, string myOutput)
        {
            int x;

            if (Serial.IsOpen)
            {
                for (x = 0; x < myOutput.Length; x++)
                {
                    try
                    {
                        Serial.Write(myOutput.Substring(x, 1));
                    }
                    catch (Exception ex)
                    {
                        //clsGlobals.log.Error("", ex);
                        mstr_error = "Error: Serial Write String" + "\n" + ex.Message;
                        return false;
                    }

                    //Add the intercharacter Delay
                    DelayMS(m_iIntercharacterDelay);
                }
            }
            else
                return false;

            //After the transmission is done, wait a little longer
            DelayMS(m_iIntercharacterDelay);

            return true;
        }

        private bool Serial_Write_Byte(System.IO.Ports.SerialPort Serial, byte myOutput)
        {
            byte[] temp = new byte[2];


            temp[0] = myOutput;
            temp[1] = 0;

            if (Serial.IsOpen)
            {
                try
                {
                    Serial.Write(temp, 0, 1);
                }
                catch (Exception ex)
                {
                    //clsGlobals.log.Error("", ex);
                    mstr_error = "Error: Serial Write Byte" + "\n" + ex.Message;
                    return false;
                }

                //Add the intercharacter Delay
                DelayMS(m_iIntercharacterDelay);
            }
            else
                return false;

            //After the transmission is done, wait a little longer
            DelayMS(m_iIntercharacterDelay);

            return true;
        }

        private bool Serial_Wait_For_ACK(System.IO.Ports.SerialPort Serial, double TimeoutmSeconds)
        {
            System.DateTime myTime = new DateTime();
            int myRead;

            myTime = System.DateTime.Now.AddMilliseconds(TimeoutmSeconds);

            while (myTime > System.DateTime.Now)
            {
                //Only read if there is something in the buffer
                if (Serial.IsOpen && Serial.BytesToRead > 0)
                {
                    myRead = Serial.ReadByte();
                    if (myRead == 6)            //ACK
                        return true;
                    else if (myRead == 21)      //NAK
                        return false;
                }
            }

            return false;
        }

        private int Serial_Wait_For_Char(System.IO.Ports.SerialPort Serial, double TimeoutmSeconds)
        {
            System.DateTime myTime = new DateTime();
            int myRead;

            myTime = System.DateTime.Now.AddMilliseconds(TimeoutmSeconds);

            while (myTime > System.DateTime.Now)
            {
                //Only read if there is something in the buffer
                if (Serial.IsOpen && Serial.BytesToRead > 0)
                {
                    myRead = Serial.ReadByte();

                    return (myRead);
                }
            }

            return -1;
        }

        private bool Serial_Wait_For_String(System.IO.Ports.SerialPort Serial, string MyString, double TimeoutmSeconds)
        {
            System.DateTime myTime = new DateTime();
            int myRead;
            string temp = "";

            myTime = System.DateTime.Now.AddMilliseconds(TimeoutmSeconds);

            while (myTime > System.DateTime.Now)
            {
                //Only read if there is something in the buffer
                if (Serial.IsOpen && Serial.BytesToRead > 0)
                {
                    myRead = Serial.ReadByte();

                    temp = temp + (char)myRead;

                    if (temp.Contains(MyString)) return true;
                }
            }

            return false;
        }

        #endregion

        #region Generic Functions

        private void DelayMS(int Timeout)
        {
            if (Timeout == 0) return;
            System.Threading.Thread.Sleep(Timeout);
            System.Windows.Forms.Application.DoEvents();
        }

        private int ConvertHexCharToDecimal(char character1, char character2)
        {
            string temp = "";

            temp = "" + character1 + character2;

            return Convert.ToInt32(temp, 16);
        }

        private int ConvertStringToDecimal(string temp)
        {

            return Convert.ToInt32(temp, 16);

        }

        private string FlipHexString(string startstring)
        {
            int x;
            string endstring = "";
            int length;

            length = startstring.Length;

            for (x = 0; x < length; x += 2)
            {
                endstring = endstring + startstring[length - (x + 2)];
                endstring = endstring + startstring[length - (x + 1)];
            }

            return endstring;
        }

        #endregion

        #endregion

        public void ReadLoop()
        {
            string strRead = "";
            bool bSOH = false;
            bool bEOT = false;


            //System.IO.StreamReader stmReader = new System.IO.StreamReader(mBluetoothStream,Encoding.Default);

            try
            {

                while (m_serport.IsOpen)
                {
                    while (m_serport.BytesToRead > 0)
                    {
                        char cR = (char)m_serport.ReadByte();


                        if (cR == SOH)
                        {
                            bSOH = true;
                            continue;
                        }

                        if (bSOH == true)
                        {
                            if (cR == EOT)
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

                                clsBlueTooth.BtData data;
                                try
                                {
                                    data.time = Convert.ToInt32(strTemps[0]);

                                    data.angle_Yaw = Convert.ToDouble(strTemps[1]);
                                    data.angle_Pit = Convert.ToDouble(strTemps[2]);
                                    data.angle_Rol = Convert.ToDouble(strTemps[3]);

                                    data.angleAccelerat_Yaw = Convert.ToDouble(strTemps[4]);
                                    data.angleAccelerat_Pit = Convert.ToDouble(strTemps[5]);
                                    data.angleAccelerat_Rol = Convert.ToDouble(strTemps[6]);

                                    data.power = Math.Abs(Convert.ToDouble(strTemps[7]));
                                    data.Intensity = Math.Abs(Convert.ToDouble(strTemps[8]));
                                    data.Consumption = Math.Abs(Convert.ToDouble(strTemps[9]));
                                    data.Save = Math.Abs(Convert.ToDouble(strTemps[10].Replace("\r\n", "")));
                                    OnRecive(data);

                                    bSOH = false;
                                    strRead = "";
                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }

                            strRead += cR;

                        }
                    }
                    System.Threading.Thread.Sleep(10);

                }

                return;

            }
            catch (System.IO.IOException ex)
            {

            }

            return;
        }
    }

}
