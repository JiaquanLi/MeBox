using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeBox
{
    public enum en_Gender
    {
        MALE = 0,
        FEMALE 
    }
    public struct st_UserInfo
    {
        public en_Gender Gender;
        public int Age;
        public double Weight;
        public double Height;
    }

    public partial class frm_User : Form
    {
        private st_UserInfo mUserInfo;

        public st_UserInfo UserInfo
        {
            get
            {
                return mUserInfo;
            }
        }
        public frm_User()
        {
            InitializeComponent();
        }

        private void frm_User_Load(object sender, EventArgs e)
        {
            //object create
            mUserInfo = new st_UserInfo();

            //Info init
            rb_Female.Checked = false;
            rb_Male.Checked = true;

            tb_Age.Text = "";
            tb_Height.Text = "";
            tb_Weight.Text = "";
        }
        private void btn_Next_Click(object sender, EventArgs e)
        {
            //Get Gender
            if(rb_Female.Checked == true)
            {
                mUserInfo.Gender = en_Gender.FEMALE;
            }
            else
            {
                mUserInfo.Gender = en_Gender.MALE;
            }
            //Get Age
            if (tb_Age.Text == "")
            {
                //MessageBox.Show("请填写年龄信息!!!","填写信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_Age.Focus();
                return;
            }
            try
            {
                mUserInfo.Age = Convert.ToInt32(tb_Age.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("年龄信息格式不正确!!!", "填写信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_Age.Focus();
                return;
            }

            //Get Weight
            if (tb_Weight.Text == "")
            {
                //MessageBox.Show("请填写体重信息!!!", "填写信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_Weight.Focus();
                return;
            }
            try
            {
                mUserInfo.Weight = Convert.ToDouble(tb_Weight.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("体重信息格式不正确!!!", "填写信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_Weight.Focus();
                return;
            }

            //Get Height
            if (tb_Height.Text == "")
            {
                //MessageBox.Show("请填写身高信息!!!", "填写信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_Height.Focus();
                return;
            }
            try
            {
                mUserInfo.Height = Convert.ToDouble(tb_Height.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("身高信息格式不正确!!!", "填写信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_Height.Focus();
                return;
            }

            this.Hide();

        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
