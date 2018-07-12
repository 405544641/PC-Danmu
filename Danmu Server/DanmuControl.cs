using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;


namespace Danmu_Server
{
    class DanmuControl
    {
        delegate void voidmove();
        
        static int last_screen_position = 0;
        Random random_screen = new Random();
        public static Font Dfaultfont = new Font("Microsoft YaHei UI", 38, FontStyle.Bold);   //默认弹幕字体
        static Image img = Image.FromFile("favicon.ico");                                     
        static Graphics g = Graphics.FromImage(img);
        public static int offset = -20;
        public static int TrueHeight = (int)g.MeasureString("A",Dfaultfont).Height+offset;           //字体真实高度
        List<Task> process = new List<Task>();
        Dictionary<Task, DanmuForm> processdic = new Dictionary<Task, DanmuForm>();            //任务列表
        string state = "Null";   //Null:实例刚创建 Inited:已初始化 Running:服务正在运行 Pause:服务暂停 Stop:服务停止 Error:出错
        public static bool[] Screentoken=new bool[Tools.ScreenHeight/TrueHeight];
        double scrennpart = 0.7;
        long handle = 0;  //0:全局 非0为指定窗口句柄
        public DanmuControl(long handle)
        {
            this.handle = handle;
            state = "Inited";
        }

        
        public bool start()
        {

            return start(Dfaultfont,0);
        }

        public bool start(Font f,int _offset)
        {
            if (state == "Error" || state == "Start") return false;
            DanmuForm temp = new DanmuForm("test", 1, Color.White, 20, 3000);
            offset = _offset;
            state = "Running";
            DanmuForm.Dfont = f;
            TrueHeight= (int)g.MeasureString("A", f).Height + offset;
            
            return true;
        }
        

        public void additem(int type, string str, int time, int v, Color c)
        {
            //MessageBox.Show("");
            
            if (state != "Running") return;
            //Debug.WriteLine("执行add内部" + AppDomain.GetCurrentThreadId().ToString());
            
            if (handle == 0)
            {


                if (type == 1)
                {
                    
                    DanmuForm f1 = new DanmuForm(str, 1, c, 20, 3000);
                   f1.Show();
                    
                    int p_temp = random_screen.Next(0, (int)(Tools.ScreenHeight * scrennpart / TrueHeight));
                    while (p_temp==last_screen_position) {
                        p_temp = random_screen.Next(0, (int)(Tools.ScreenHeight * scrennpart / TrueHeight));
                    };
                    
                    last_screen_position = p_temp;
                    f1.Top = p_temp * TrueHeight;
                    f1.Left = Tools.ScreenWidth;
                    //f1.Left -= 300;
                    f1.Show();
                    Task temp = new Task(async () =>
                    {
                        
                       // Debug.WriteLine("执行add thread内部" + AppDomain.GetCurrentThreadId().ToString());
                        
                        //Tools.Message(Thread.CurrentThread.ManagedThreadId);
                        while (!f1.token)
                            {
                        //Debug.WriteLine("A:" + AppDomain.GetCurrentThreadId().ToString() + "   " + f1.Left.ToString() + " " + f1.Top.ToString());

                            

                            f1.Left = f1.Left - 5;
                           
                                await Task.Delay(v);
                            }
                        f1.Close();
                    });
                    //process.Add(temp);     -----之后完善
                    try {  processdic.Add(temp, f1); temp.Start(); } catch { }
                    

                    //f1.startprocess();
                    

                }
                else if (type == 2)
                {
                    //间隔过大问题---已解决（利用静态偏移变量解决）
                    DanmuForm f1 = new DanmuForm(str, 2, c, 20, 3000);
                    f1.Show();
                    f1.Left = (Tools.ScreenWidth - f1.Width) / 2;
                    Task temp = new Task(async () => {
                        int i = 0;
                        for (; i < Screentoken.Length; i++)
                        {
                            if (!Screentoken[i]) { Screentoken[i] = true; break; }
                            else if (i == Screentoken.Length - 1) i = 0;
                        }

                        //Tools.Message(i);
                        f1.Top = i * TrueHeight;

                        await Task.Delay(time);
                            f1.Close();      
                        Screentoken[i] = false;
                    }

                    );
                    temp.Start();
                }
                else if (type == 3)
                {
                    //暂缺
                }


            }






        }
        public void additem(int type, string str, int time, int v)
        {
            additem(type, str, time, v, Color.White);

        }
        public void stop()
        {
            state = "Stop";
        }
    }
}
