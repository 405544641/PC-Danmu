using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Danmu_Server;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Client_local
{
    public partial class erweiCode : Form
    {
        ImageSource eimage;
        public erweiCode(ImageSource ima)
        {
            InitializeComponent();
            eimage = ima;
        }

        private void erweiCode_Load(object sender, EventArgs e)
        {
            Top = 0;
            Width = (int)(Tools.ScreenHeight*0.7);
            Height =(int)( Tools.ScreenHeight*0.7);
            pictureBox1.Width = Width;
            pictureBox1.Height = Height;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)eimage));
            encoder.Save(ms);

            Bitmap bp = new Bitmap(ms);
            ms.Close();
            pictureBox1.Image = bp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MainWindow.erweiCodeCreted = false;
            
            this.Close();
        }
    }
}
