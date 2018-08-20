using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeBox
{
    public class clsMyProgressBar: System.Windows.Forms.ProgressBar
    {

        private double devider_13 = (double)1 / (double)3;
        private double devider_23 = (double)2 / (double)3;
        public clsMyProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

        }

        //重写OnPaint方法
        protected override void OnPaint(PaintEventArgs e)
        {
            SolidBrush brush = null;
            Rectangle rec = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);
            }

            Pen pen = new Pen(Color.LightGray, 1);
            e.Graphics.DrawRectangle(pen, rec);
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), 1, 1, rec.Width - 1, rec.Height - 1);

            rec.Height -= 1;
            rec.Width = (int)(rec.Width * ((double)Value / (double)Maximum)) - 1;

            double d_Persent = ((double)Value) / ((double)Maximum);

            if (d_Persent < devider_13)
            {
                brush = new SolidBrush(Color.LimeGreen);
                e.Graphics.FillRectangle(brush, 1, 1, rec.Width, rec.Height);
            }
            else if (d_Persent >= devider_13 && d_Persent < devider_23)
            {
                brush = new SolidBrush(Color.Orange);
                e.Graphics.FillRectangle(brush, 1, 1, rec.Width, rec.Height);
            }
            else if(d_Persent >= devider_23)
            {
                brush = new SolidBrush(Color.Red);
                e.Graphics.FillRectangle(brush, 1, 1, rec.Width, rec.Height);

            }

        }
    }
}

