using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data;
using MySql.Data.MySqlClient;
namespace Danmu_Server
{
    /// <summary>
    /// Interaction logic for regionWindow.xaml
    /// </summary>
    public partial class regionWindow : Window
    {
        string account = "";
        string password = "";
        string name = "";
        string sign = "";
        public regionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if(textBox.Text=="" || passwordBox.Password == "")
            {
                Tools.MessageBox("账号或密码不能为空");
                return;
            }
            password = passwordBox.Password;
            account = textBox.Text;
            //载入下一界面
            new Task( ()=> {
                Dispatcher.Invoke(async () => {
                for (int i = 0; i < reg2.Width-1; i+=5)
                {
                    reg2.Margin = new Thickness(reg2.Margin.Left+5, reg2.Margin.Top, reg2.Margin.Right , reg2.Margin.Bottom); 
                    label_Copy2.Margin= new Thickness(label_Copy2.Margin.Left + 5, label_Copy2.Margin.Top, label_Copy2.Margin.Right, label_Copy2.Margin.Bottom);
                    label_Copy3.Margin = new Thickness(label_Copy3.Margin.Left + 5, label_Copy3.Margin.Top, label_Copy3.Margin.Right, label_Copy3.Margin.Bottom);
                    label_Copy4.Margin = new Thickness(label_Copy4.Margin.Left + 5, label_Copy4.Margin.Top, label_Copy4.Margin.Right, label_Copy4.Margin.Bottom);
                        textBox_Copy.Margin= new Thickness(textBox_Copy.Margin.Left + 5, textBox_Copy.Margin.Top, textBox_Copy.Margin.Right, textBox_Copy.Margin.Bottom);
                        textBox_Copy1.Margin = new Thickness(textBox_Copy1.Margin.Left + 5, textBox_Copy1.Margin.Top, textBox_Copy1.Margin.Right, textBox_Copy1.Margin.Bottom);
                        button_Copy.Margin = new Thickness(button_Copy.Margin.Left + 5, button_Copy.Margin.Top, button_Copy.Margin.Right, button_Copy.Margin.Bottom);
                        await Task.Delay(1);
                }
                });
            }).Start();
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            if(textBox_Copy.Text=="" || textBox_Copy1.Text == "")
            {
                MessageBox.Show("不能为空");
                return;
            }
            sign = textBox_Copy1.Text;
            name= textBox_Copy.Text;
            //载入下一界面
            new Task(() => {
                Dispatcher.Invoke(async () => {
                    for (int i = 0; i < reg2.Width - 1; i += 5)
                    {
                        reg3.Margin = new Thickness(reg3.Margin.Left + 5, reg3.Margin.Top, reg3.Margin.Right, reg3.Margin.Bottom);
                        label_Copy5.Margin = new Thickness(label_Copy5.Margin.Left + 5, label_Copy5.Margin.Top, label_Copy5.Margin.Right, label_Copy5.Margin.Bottom);
                        image1.Margin = new Thickness(image1.Margin.Left + 5, image1.Margin.Top, image1.Margin.Right, image1.Margin.Bottom);
                        imgrec.Margin = new Thickness(imgrec.Margin.Left + 5, imgrec.Margin.Top, imgrec.Margin.Right, imgrec.Margin.Bottom);
                        button1.Margin = new Thickness(button1.Margin.Left + 5, button1.Margin.Top, button1.Margin.Right, button1.Margin.Bottom);
                        button1_Copy.Margin= new Thickness(button1_Copy.Margin.Left + 5, button1_Copy.Margin.Top, button1_Copy.Margin.Right, button1_Copy.Margin.Bottom);
                        await Task.Delay(1);
                    }
                });
            }).Start();
        }

        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            MySqlCommand cmd = MainWindow.connect.CreateCommand();
            cmd.CommandText = "insert into account(accountID,Nick,sign,password) values("+"\""+account+"\","+"\""+name+"\","+"\""+sign+"\","+"\""+password+"\""+")";
            try{
                cmd.ExecuteNonQuery();
                this.Close();
            }catch
            {

            }
        }
    }
}
