using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MeBox
{
    public class VerticalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }


        public VerticalProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
        /// <summary>
        /// 从下到上 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            SolidBrush brush = null;

            Rectangle rec = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);
            }
            //Pen pen = new Pen(this.ForeColor, 1); //左上的线色
            Pen pen = new Pen(Color.LightGray, 1);
            e.Graphics.DrawRectangle(pen, rec);
            //绘制进度条空白处
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), 1, 1, rec.Width - 1, rec.Height - 1);

            rec.Width -= 1;
            rec.Height = (int)(rec.Height * ((double)Value / Maximum)) - 1;
            brush = new SolidBrush(this.ForeColor);
            //绘制进度条进度
            e.Graphics.FillRectangle(brush, 1, Height - rec.Height - 1, rec.Width, rec.Height);
        }
        /// <summary>
        /// 从左到右
        /// </summary>
        /// <param name = "e" ></ param >
        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    SolidBrush brush = null;
        //    Rectangle rec = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

        //    if (ProgressBarRenderer.IsSupported)
        //    {
        //        ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);
        //    }
        //    //Pen pen = new Pen(this.ForeColor, 1); //左上的线色
        //    Pen pen = new Pen(Color.Red, 1);
        //    e.Graphics.DrawRectangle(pen, rec);
        //    e.Graphics.FillRectangle(new SolidBrush(this.BackColor), 1, 1, rec.Width - 1, rec.Height - 1);

        //    rec.Height -= 1;
        //    rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 1;
        //    brush = new SolidBrush(this.ForeColor);
        //    e.Graphics.FillRectangle(brush, 1, 1, rec.Width, rec.Height);
        //}
    }
}
