using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using ThoughtWorks.QRCode;
using Gma.QrCodeNet;
using serverkhd;
using System.Web;
using System.Net;
using System.Drawing;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Controls;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.ExceptionHandler;
using ThoughtWorks.QRCode.Codec.Data;

namespace Danmu_Server
{
    class Tools
    {
        public static string Getipv4ip()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        public static int ScreenWidth = (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        public static int ScreenHeight = (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height; 
        public static void MessageBox<T>(params T[] str)
        {
            string Allstr = "";
            foreach(T x in str)
            {
                Allstr += x.ToString();
            }
            System.Windows.Forms.MessageBox.Show(Allstr);
        }

        public static Bitmap GetQrCode(string str)
        {
            QRCodeEncoder qc = new QRCodeEncoder();
            qc.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qc.QRCodeScale = 4;
            qc.QRCodeVersion = 7;
            qc.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            Bitmap ima = qc.Encode(str);
            return ima;
        }
    }
}
