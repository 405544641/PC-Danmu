using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Danmu_Server
{
    public partial class DanmuForm : Form
    {
        
        public static Font Dfont = DanmuControl.Dfaultfont;
        public bool token = false;
        string textstr = "";
        int type=1;
        public DanmuForm(string Dstr,int p_type, Color cor,int V_righttoleft,int V_topOrbottom)
        {
            InitializeComponent();
            this.Text = Dstr;
            textstr = Dstr;
            Control.CheckForIllegalCrossThreadCalls = false;
            type = p_type;
            label1.ForeColor = cor;
        }
        public DanmuForm(string Dstr, int p_type, Color cor, int V_righttoleft, int V_topOrbottom,Font f)
        {
            InitializeComponent();
            this.Text = Dstr;
            textstr = Dstr;
            Control.CheckForIllegalCrossThreadCalls = false;
            type = p_type;
            label1.ForeColor = cor;
            Dfont = f;
            
        }
        private void DanmuForm_Load(object sender, EventArgs e)
        {

            //            Graphics g = this.CreateGraphics();
            //SizeF s = g.MeasureString("A", DanmuControl.Dfaultfont);
            label1.Font = Dfont;
            label1.Text = textstr;
            label1.Left = 0;
            label1.Top = 0;
            Width = label1.Width;
            Height = label1.Height;
            //Tools.MessageBox(DanmuControl.TrueHeight);
                label1.Top = -(int)((Height - DanmuControl.TrueHeight) / 2);
                Height = (int)DanmuControl.TrueHeight;
            this.TopMost = true;
            
    }

        private void DanmuForm_Move(object sender, EventArgs e)
        {
           // Debug.WriteLine("Left:{0} Width:{1}", Left,Width);
            if (Left < -Width+10) token = true;
        }
    }
}
