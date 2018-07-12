using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Sockets;
using System.Net;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text.RegularExpressions;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Windows.Interop;
using System.Collections.ObjectModel;
using serverkhd;
using Danmu_Server;
using Client_local;


namespace Danmu_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<object> ObservableObj=new ObservableCollection<object>();
        public static MySqlConnection connect;
        MySqlDataReader dr;
        MySqlCommand cmd;
        
        static System.Windows.Forms.NotifyIcon ico = new System.Windows.Forms.NotifyIcon();
        socket_server ss;
        socket_client tocentreserver;
        //state var
        static bool Login_showing = false;
        static bool logined = false;
        static bool multicolormodel = true;
        static bool colorcontrol = true;
        static bool OnlyLocal = false;
        static string L_account = "";
        static string L_password= "";
        static int danmu_v_top = 5000;
        static System.Drawing.Color currentcolor = System.Drawing.Color.White;
        static int danmu_v_righttoleft = 20;
        static bool logingrid_showed = false;
        Random r = new Random();  //可以用来搞彩色模式
        public static bool erweiCodeCreted = false;
        DanmuControl d = new DanmuControl(0);
        public MainWindow()
        {
            InitializeComponent();
            //DanmuForm d = new DanmuForm();
            // d.Show();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            
            d.start(DanmuControl.Dfaultfont,-20);
            
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {


            //listView1.Items[1] = "22";
            
            d.additem(2, "Test", danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)): currentcolor);
            System.Windows.Forms.Application.DoEvents();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
            d.additem(1, "Test", danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
            System.Windows.Forms.Application.DoEvents();
           // try
           //  {
           //     System.Windows.Forms.Application.Run();
           //  }
           // catch { }
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("UI:" + AppDomain.GetCurrentThreadId().ToString());


            //initialize UI
           
            ico.Icon = new System.Drawing.Icon("favicon.ico");
            ico.Text = "Danmu Server";
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            System.Windows.Forms.MenuItem configure = new System.Windows.Forms.MenuItem("Configure");
            configure.Click += new EventHandler((a, er) => {
                //Danmu_server.configure cw = new Danmu_server.configure();
                //cw.Show();
            });
            exit.Click += new EventHandler((a, er) => { Environment.Exit(0); });
            System.Windows.Forms.MenuItem mainw = new System.Windows.Forms.MenuItem("MainWindow");
            mainw.Click += new EventHandler((a, er) => { this.WindowState = WindowState.Normal; this.Show(); });
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { mainw, configure, exit };
            ico.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            ico.Visible = true;
            
            textBox.Text = Tools.Getipv4ip();

            string conStr = @"server=39.108.112.159;user id=root;password=hs295093653;database=test";
            //尝试连接到数据库
            try
            {
                connect = new MySqlConnection(conStr); connect.Open();

            }
            catch { System.Windows.Forms.MessageBox.Show("与数据库通讯出现问题,只能使用本地模式！"); OnlyLocal = true; }
        }


        //创建服务器按钮
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (button2.Content.ToString()== "创建服务器")
            {
                ss = new socket_server(IPAddress.Parse(textBox.Text), 9240);
                ss.isAsyncReceive = false;
                ss.clientLeave = delegate (Socket soc)
                {
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        Dispatcher.Invoke(delegate { try { listView1.Items.RemoveAt(i); } catch { } });
                       
                    }
                };
                ss.ClientInEvent = delegate (Socket soc)
                {
                    Dispatcher.Invoke(delegate { listView1.Items.Add(soc.RemoteEndPoint.ToString()); });
                    
                };
                ss.MessageIn = delegate (string str, Socket soc)
                {
                    string typed = getvalue(str, "Type");
                    Debug.WriteLine(str);
                    if ( typed== "")
                    {
                        d.additem(1, str, danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                        Dispatcher.Invoke(() => {
                            listView1_Copy.Items.Add(str);

                        });
                    }else
                    {
                        if (typed == "0")   //移动
                        {
                            Dispatcher.Invoke(() => {
                                listView1_Copy.Items.Add(new { Context = str.Substring(10, str.Length - 10), Time = DateTime.Now.ToString("HH:mm:ss") });

                            });
                            d.additem(1, str.Substring(10,str.Length-10), danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                        }else if (typed == "1")   //顶部
                        {
                            Dispatcher.Invoke(() => {
                                listView1_Copy.Items.Add(new { Context = str.Substring(10, str.Length - 10), Time = DateTime.Now.ToString("HH:mm:ss") });

                            });
                            d.additem(2, str.Substring(10, str.Length - 10), danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                        }else
                        {
                            Dispatcher.Invoke(() => {
                                listView1_Copy.Items.Add(new { Context = str, Time = DateTime.Now.ToString("HH:mm:ss") });

                            });
                            d.additem(1, str, danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                        }

                       
                    }

                    
                    System.Windows.Forms.Application.Run();
                };
               
                if (ss.Create())
                {
                    Title += " Created";
                    //自动生成二维码
                    if (radioButton.IsChecked == true)
                    {
                        Bitmap qccode = Tools.GetQrCode("<QRTYPE>Local/><IP>" + textBox.Text + "/>\n<Port>9240/>");
                        IntPtr ip = qccode.GetHbitmap();
                        BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());


                        image1.Source = bitmapSource;
                    }else
                    {
                        if (logined)
                        {
                            Bitmap qccode = Tools.GetQrCode("<QRTYPE>Network/><Account>" + L_account + "/>\n<Password>"+L_password+"/>");
                            IntPtr ip = qccode.GetHbitmap();
                            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());


                            image1.Source = bitmapSource;
                        }else
                        {
                            System.Windows.Forms.MessageBox.Show("还未登入.");
                            radioButton.IsChecked = true;
                        }
                    }
                    button2.Content = "关闭服务器";
                    textBox.IsEnabled = false;
                    
                }
                else { Tools.MessageBox("创建服务器失败!"); }
            }else
            {
               
                    ss.Close();
                    if(tocentreserver!=null) tocentreserver.Close();
                    Title = "Local Mode:Danmu Server(端口:9240)";
                    logined = false;
                textBox.IsEnabled = true;
                button2.Content = "创建服务器";
                    image1.Source = null;
                logined = false;
                L_account = "";
                L_password = "";
                
            }

        }
        //放大二维码
        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!erweiCodeCreted)
            {
                erweiCode ecw = new erweiCode(image1.Source);
                erweiCodeCreted = true;
                ecw.Show();
            }
        }

        //窗口关闭
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                
                ss.Close();
                if (tocentreserver != null) tocentreserver.Close();
                connect.Close();

            }
            catch { }
            System.Windows.Forms.Application.Exit();
        }
        //窗口已关闭
        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if(cd.ShowDialog()==System.Windows.Forms.DialogResult.Cancel) return;
            
            System.Windows.Media.Brush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B));
            DIY_color_box.Fill = br;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FontDialog fd = new FontDialog();
            if(fd.ShowDialog()==System.Windows.Forms.DialogResult.Cancel) return;
            
            d.start(fd.Font,0);
            label6.Content = "Name="+fd.Font.Name+"  Size:"+fd.Font.Size.ToString();
        }

        //单击头像
        private void Ellipse_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            
            if (!logingrid_showed && !Login_showing) {
                Login_showing = true;
                int height = (int)logingrid.Height;
                Task temp = new Task(async () => {
                    for (int i = 0; i < height; i+=15)
                    {
                        Dispatcher.Invoke(() => { logingrid.Margin = new Thickness(logingrid.Margin.Left, logingrid.Margin.Top, logingrid.Margin.Right, logingrid.Margin.Bottom +15); });
                        await Task.Delay(20);
                    }
                    Login_showing = false;
                    logingrid_showed = true;
                });
                temp.Start();
                



                }
            else if(!Login_showing && logingrid_showed)
            {
                logingrid.Margin = new Thickness(logingrid.Margin.Left, logingrid.Margin.Top, logingrid.Margin.Right,  - logingrid.Height-5); logingrid_showed =false;
            }
        }

        //登入
        private void loginbutton_Click(object sender, RoutedEventArgs e)
        {
            if (OnlyLocal)
            {
                System.Windows.Forms.MessageBox.Show("无法连接到数据库,只限于本地模式！");
                return;
            }
            
            cmd = connect.CreateCommand();
            cmd.CommandText = "select * from account where accountid=\'"+textBox1.Text+"\'";
            dr=cmd.ExecuteReader();
            dr.Read();
            try
            {
                string pass = dr.GetString(6);
                if (pass == passwordbox.Password)
                {
                    
                    logined = true;
                    L_account = textBox1.Text;
                    L_password = pass;
                    Sign_lable.Content= dr.GetString(2);     //签名
                    NickName_lable.Content= dr.GetString(1);   //昵称
                    
                    Ellipse_MouseDown_1(null, null);
                    dr.Close();
                }
                else
                {

                    System.Windows.Forms.MessageBox.Show("密码错误！");
                    dr.Close();
                    
                    return;
                }
                
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("账号不存在！");
                dr.Close();
                return;
            }

            try { tocentreserver.Close();} catch { }
            tocentreserver = new socket_client(IPAddress.Parse("39.108.112.159"), 9000);
            if (!tocentreserver.Connect())
            {
                System.Windows.Forms.MessageBox.Show("连接中心服务器失败！");
                logined = false;
                Sign_lable.Content = "Personalization Sign";     //签名
                NickName_lable.Content = "Name";  //昵称

                
                return;
            }
            if(button2.Content.Equals( "创建服务器"))
                button2_Click(null, null);
            tocentreserver.sendMessage("<UType>Login</><Account>"+textBox1.Text+"</><LocalIP>"+ss.getEndpoint()+"</><User>Server</>");
            tocentreserver.Message_Receive = delegate (string str)
            {
                string typed = getvalue(str, "Type");
                Debug.WriteLine(str);
                if (typed == "")
                {
                    Dispatcher.Invoke(() => {
                        listView1_Copy.Items.Add(new { Context = str, Time = DateTime.Now.ToString("HH:mm:ss") });
                        ;
                    });
                    d.additem(1, str, danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                }
                else
                {
                    if (typed == "0")   //移动
                    {
                        Dispatcher.Invoke(() => {
                            listView1_Copy.Items.Add(new { Context = str.Substring(10, str.Length - 10), Time = DateTime.Now.ToString("HH:mm:ss") });

                        });
                        d.additem(1, str.Substring(10, str.Length - 10), danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                    }
                    else if (typed == "1")   //顶部
                    {
                        Dispatcher.Invoke(() => {
                            listView1_Copy.Items.Add( new { Context = str.Substring(10, str.Length - 10), Time = DateTime.Now.ToString("HH:mm:ss") });

                        });
                        d.additem(2, str.Substring(10, str.Length - 10), danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => {
                            listView1_Copy.Items.Add(new { Context = str, Time = DateTime.Now.ToString("HH:mm:ss") });

                        });
                        d.additem(1, str, danmu_v_top, danmu_v_righttoleft, multicolormodel ? System.Drawing.Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)) : currentcolor);
                    }
                }
                System.Windows.Forms.Application.Run();
            };
            radioButton2.IsChecked = true;
        }

        //注册
        private void regionbutton_Click(object sender, RoutedEventArgs e)
        {
            if (OnlyLocal)
            {
                System.Windows.Forms.MessageBox.Show("本地模式下无法进行此操作。");
                return;
            }
            regionWindow rw = new regionWindow();
            rw.Show();
            
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (!logined)
            {
                radioButton.IsChecked = true;
                return;
            }else
            {
                Bitmap qccode = Tools.GetQrCode("<QRTYPE>Network/><Account>" + L_account + "/>\n<Password>" + L_password + "/>");
                IntPtr ip = qccode.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());


                image1.Source = bitmapSource;
            }
        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            Bitmap qccode = Tools.GetQrCode("<QRTYPE>Local/><IP>" + textBox.Text + "/>\n<Port>9240/>");
            IntPtr ip = qccode.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

           
            image1.Source = bitmapSource;
        }

        private void colorg1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            DIY_color_box.Fill = colorg1.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg2.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg3.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg4.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg5.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg6.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg7.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg8.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg9.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void colorg10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DIY_color_box.Fill = colorg10.Fill;
            currentcolor = System.Drawing.Color.FromArgb(((SolidColorBrush)DIY_color_box.Fill).Color.R, ((SolidColorBrush)DIY_color_box.Fill).Color.G, ((SolidColorBrush)DIY_color_box.Fill).Color.B);
        }

        private void label9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse_MouseDown_1(null,null);
        }

        private void button_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void button5_Click_1(object sender, RoutedEventArgs e)
        {
            //Valid check
            try
            {
                int v = Convert.ToInt32(textBox2.Text);
                if (v <= 0) throw new Exception();
                danmu_v_righttoleft = v;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("数据输入错误！");
                return;
            }
        }
        static string getvalue(string allstr, string item)
        {
            return getmidString(allstr, "<" + item + ">", "</>", 0);
        }

        static string getmidString(string allstr, string former, string behind, int beginfrom)
        {
            int positionA = allstr.IndexOf(former, beginfrom);
            int positionB = allstr.IndexOf(behind, positionA + 1);
            if (positionA < 0 || positionB < 0) return "";
            return allstr.Substring(positionA + former.Length, positionB - positionA - former.Length);
        }
        private void button5_Copy_Click(object sender, RoutedEventArgs e)
        {
            //Valid check
            try
            {
                int v = Convert.ToInt32(textBox2_Copy.Text);
                if (v <= 0) throw new Exception();
                danmu_v_top = v;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("数据输入错误！");
                return;
            }
        }

        private void radioButton_Unchecked(object sender, RoutedEventArgs e)
        {
         
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            multicolormodel = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            multicolormodel = false;
        }
    }

    
}
