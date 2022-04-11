using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Security.Permissions;

namespace WpfApp1
{
    public class CmdUtils
    {
        public String shell = "";
        public async Task sendCmd(MainWindow cmdoom)
        {
            await Task.Run(() =>
            { 
            Process cmd = null;
            if (cmd == null)
            {
                cmd = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "";//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = true;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                cmd.StartInfo = startInfo;
                // cmd.Start();
            }
            if (cmd.Start())//开始进程  
            {
                cmd.StandardOutput.ReadLine().Trim();
                cmd.StandardOutput.ReadLine().Trim();
                while (cmdoom.cmd_isRun.IndexOf("start") != -1)
                {
                    if (shell.Length > 0)
                    {
                        cmd.StandardInput.WriteLine(shell);
                        cmd.StandardOutput.ReadLine().Trim();

                        cmd.StandardInput.WriteLine("\n");
                        String log = cmd.StandardOutput.ReadLine().Trim();
                        String path = log.Substring(0, 2).ToUpper();
                        updateLog(cmdoom, log);
                        log = "";
                        do
                        {
                            String logm = cmd.StandardOutput.ReadLine().Trim();
                            if (logm.IndexOf(path) != -1)
                            {
                                break;
                            }
                            updateLog(cmdoom, logm + "\n");
                            log += logm;

                        } while (cmdoom.cmd_isRun.IndexOf("start") != -1);

                        //shell = "";
                    }
                }

                cmd.Close();

                cmd = null;
                return;
            }
            });
            return;
        }
        private delegate void UpdateLog();

        private void updateLog(MainWindow cmd, String log)
        {
            UpdateLog set = delegate ()
            {
                cmd.TextBox1.AppendText("\n" + log);
            };
            cmd.TextBox1.Dispatcher.BeginInvoke(set);
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string adb = "adb shell";
        int i = 0;
        bool adb_status = false;
        string[] GpioArrayStr;
        string GPIO_Select_String;
        char[] GPIO_Select_Arr = null;
        string SelectedGpio;
        string SysPath = "/sys/devices/platform/pinctrl@1000b000/mt_gpio";
        private DispatcherTimer Time = null;
        int Gpio_Support_R0R1 = 0;
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        private Thread led_status;
        string Log_Path = Environment.CurrentDirectory + "\\log";
        private BackgroundWorker backgroundWorker;
        public String cmd_isRun = "start";
        CmdUtils cmd = new CmdUtils();

        public MainWindow()
        {
            InitializeComponent();

            for (i = 0; i < 179; i++)
            {
                comboBox1.Items.Add(i);
            }
            MODE.Items.Add(0);
            MODE.Items.Add(1);
            MODE.Items.Add(2);
            MODE.Items.Add(3);
            MODE.Items.Add(4);
            MODE.Items.Add(5);
            DIR.Items.Add("IN");
            DIR.Items.Add("OUT");
            DOUT.Items.Add("0-Low");
            DOUT.Items.Add("1-High");
            DIN.Items.Add("0-Low");
            DIN.Items.Add("1-High");
            PULLEN.Items.Add("0-Low");
            PULLEN.Items.Add("1-High");
            PULLSEL.Items.Add("0-Low");
            PULLSEL.Items.Add("1-High");
            IES.Items.Add(0);
            IES.Items.Add(1);
            SMT.Items.Add(0);
            SMT.Items.Add(1);
            DRIVE.Items.Add(0);
            DRIVE.Items.Add(1);
            DRIVE.Items.Add(2);
            DRIVE.Items.Add(3);
            DRIVE.Items.Add(4);
            DRIVE.Items.Add(5);
            DRIVE.Items.Add(6);
            DRIVE.Items.Add(7);
            R1R0_Value.Items.Add("00");
            R1R0_Value.Items.Add("01");
            R1R0_Value.Items.Add("10");
            R1R0_Value.Items.Add("11");
            // comboBox1.SelectedIndex = 0;
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            Time = dispatcherTimer;
            Time.Interval = TimeSpan.FromMilliseconds(500);
            Time.Tick += Time_TickAsync;
            //Time.Start();

            led_status = new Thread(new ThreadStart(led_status_threadfunc));
            led_status.IsBackground = true;//设置为后台线程，当主线程结束后，后台线程自动退出，否则不会退出程序不能结束
            led_status.Start();

            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息                                       
            p.StartInfo.RedirectStandardError = false;// p.StartInfo.CreateNoWindow = false;//不显示程序窗口
            p.StartInfo.CreateNoWindow = true;          //设置不显示窗口  
            ProgressBar.Visibility = Visibility.Collapsed;
            //Catch_Log = new Thread(new ThreadStart(Catch_Logs_threadfunc));
            //Catch_Log.IsBackground = true;//设置为后台线程，当主线程结束后，后台线程自动退出，否则不会退出程序不能结束
        }

        private async Task Catch_Logs_func()
        {
            await cmd.sendCmd(this);
        }

        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)//毫秒
            {
                DispatcherHelper.DoEvents();
            }
        }

        public static class DispatcherHelper
        {
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents()
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
                try { Dispatcher.PushFrame(frame); }
                catch (InvalidOperationException) { }
            }
            private static object ExitFrames(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }

        private async Task<string> AsyncTask_Status_refresh()
        {
            p.StartInfo.Arguments = "/c adb devices";
            try
            {
                return await Task.Run(() =>
                {
                    p.Start();
                    string outtr = p.StandardOutput.ReadToEnd();
                    p.Close();
                    outtr = outtr.Replace("List of devices attached", "");
                    outtr = outtr.Replace("device", "");
                    outtr = outtr.Replace("\r", "");
                    outtr = outtr.Replace("\n", "");
                    return outtr;

                });
            }
            catch
            {
                return "err";
            }
        }

        async void led_status_threadfunc()
        {
            while (true)
            {
                Action action_offline = () =>
                {
                    led.Fill = new SolidColorBrush(Colors.Red);
                    led.Stroke = new SolidColorBrush(Colors.Red);
                };
                Action action_online = () =>
                {
                    led.Fill = new SolidColorBrush(Colors.Green);
                    led.Stroke = new SolidColorBrush(Colors.Green);
                };
                string outtr = await AsyncTask_Status_refresh();
                if (outtr.Length == 0)
                {
                    await led.Dispatcher.BeginInvoke(action_offline);
                    adb_status = false;
                }
                else
                {
                    if (!adb_status) //第一次找到设备执行adb root
                    {
                        p.StartInfo.Arguments = "/c adb root";
                        p.Start();
                        p.Close();

                        //p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + "echo 1 > /sys/class/misc/scp/scp_mobile_log" + "\"";

                        //p.Start();
                        //p.Close();
                        /* 第一次找寻gpio节点路径，暂不需要
                        p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + "find /sys/devices/platform -name mt_gpio" + "\"";
                        p.Start();
                        SysPath = p.StandardOutput.ReadToEnd();
                        SysPath = SysPath.Replace("\n", "");
                        SysPath = SysPath.Replace("\r", "");
                        p.Close();
                        */
                    }
                    await led.Dispatcher.BeginInvoke(action_online);
                    adb_status = true;
                }
            }
        }


        private void Time_TickAsync(object sender, EventArgs e)
        {
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox1.SelectionStart = TextBox1.Text.Length; //Set the current caret position at the end
            TextBox1.Focus();
            //TextBox1.ScrollToCaret(); //Now scroll it automatically
        }


        private void Button_Click_Get_Gpio(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    if (comboBox1.SelectedIndex != -1)
                    {

                        SelectedGpio = comboBox1.SelectedItem.ToString();
                        TextBox1.Text = "正在gpio" + SelectedGpio + "数据...";

                        p.StartInfo.Arguments = "/c adb shell cat  " + SysPath;
                        p.Start();

                        string outtr = p.StandardOutput.ReadToEnd();
                        GpioArrayStr = outtr.Split(new string[] { "\r\n", ":" }, StringSplitOptions.RemoveEmptyEntries); //将字符串以"\r\n", ":"分隔成字符串
                                                                                                                         //MessageBox.Show(outtr);

                        GpioArrayStr[Convert.ToInt32(SelectedGpio)] = GpioArrayStr[6 + Convert.ToInt32(SelectedGpio) * 2];
                        // TextBox1.DataContext = null;
                        p.Close();

                        GPIO_Select_String = GpioArrayStr[Convert.ToInt32(SelectedGpio)];
                        string GPIO_Select_Str_Print = GPIO_Select_String;//供打印
                        GPIO_Select_Str_Print = Regex.Replace(GPIO_Select_Str_Print, @"(?<=.{0}).{1}", " $0"); //每个字符串中间加入空格
                        TextBox1.Text = "获取GPIO" + SelectedGpio + " : " + GPIO_Select_Str_Print; //打印
                        GPIO_Select_String = GPIO_Select_String.Replace(" ", ""); //去掉空格
                        GPIO_Select_String = GPIO_Select_String.Replace("(", "");//去掉括号
                        GPIO_Select_String = GPIO_Select_String.Replace(")", "");
                        GPIO_Select_Arr = GPIO_Select_String.ToCharArray();
                        GPIO.Content = SelectedGpio;
                        MODE.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[0]) - '0'; //string转整型
                        DIR.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[1]) - '0'; //string转整型
                        DOUT.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[2]) - '0'; //string转整型
                        DIN.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[3]) - '0'; //string转整型
                        DRIVE.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[4]) - '0'; //string转整型
                        SMT.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[5]) - '0'; //string转整型
                        IES.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[6]) - '0'; //string转整型
                        PULLEN.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[7]) - '0'; //string转整型
                        PULLSEL.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[8]) - '0'; //string转整型

                        VALUE.Content = GPIO_Select_String;
                        MODE.Items.Refresh();
                        DIR.Items.Refresh();
                        DIN.Items.Refresh();
                        PULLEN.Items.Refresh();
                        PULLSEL.Items.Refresh();
                        IES.Items.Refresh();
                        SMT.Items.Refresh();
                        DRIVE.Items.Refresh();
                        if (Gpio_Support_R0R1 == 1)
                        {
                            if (GPIO_Select_Arr[9] == '0' && GPIO_Select_Arr[10] == '0')
                            {
                                R1R0_Value.SelectedIndex = 0;
                            }
                            else if (GPIO_Select_Arr[9] == '0' && GPIO_Select_Arr[10] == '1')
                            {
                                R1R0_Value.SelectedIndex = 1;
                            }
                            else if (GPIO_Select_Arr[9] == '1' && GPIO_Select_Arr[10] == '0')
                            {
                                R1R0_Value.SelectedIndex = 2;
                            }
                            else if (GPIO_Select_Arr[9] == '1' && GPIO_Select_Arr[10] == '1')
                            {
                                R1R0_Value.SelectedIndex = 3;
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    if (comboBox1.SelectedIndex != -1)
                    {
                        SelectedGpio = comboBox1.SelectedItem.ToString();
                        Gpio_Support_R0R1 = 0; //每次选择GPIO时默认不支持R0R1
                        p.StartInfo.Arguments = "/c adb shell cat  " + SysPath;

                        p.Start();

                        string outtr = p.StandardOutput.ReadToEnd();
                        GpioArrayStr = outtr.Split(new string[] { "\r\n", ":" }, StringSplitOptions.RemoveEmptyEntries);
                        // MessageBox.Show(outtr);

                        GpioArrayStr[Convert.ToInt32(SelectedGpio)] = GpioArrayStr[6 + Convert.ToInt32(SelectedGpio) * 2];
                        // TextBox1.DataContext = null;
                        // TextBox1.Text =  TextBox1.Text + "\r\n"+ GpioArrayStr[i];

                        //GpioArrayStr[5] = "010010110";
                        p.Close();

                        GPIO_Select_String = GpioArrayStr[Convert.ToInt32(SelectedGpio)];
                        GPIO_Select_String = GPIO_Select_String.Replace(" ", ""); //去掉空格
                        GPIO_Select_String = GPIO_Select_String.Remove(4, 1);//由于Android11打印第五位多个0，如： 028: 3001000111   需要去掉这个才能正常
                                                                             // MessageBox.Show(GPIO_Select_String);
                        R1R0.Visibility = Visibility.Collapsed; //先隐藏R0R1
                        R1R0_Value.Visibility = Visibility.Collapsed;
                        string GPIO_Select_Str_Print = GPIO_Select_String;
                        if (GPIO_Select_String.Length > 9)
                        {
                            Gpio_Support_R0R1 = 1;
                            R1R0.Visibility = Visibility.Visible; //将R0R1设置为可见
                            R1R0_Value.Visibility = Visibility.Visible;
                            GPIO_Select_String = GPIO_Select_String.Replace("(", "");
                            GPIO_Select_String = GPIO_Select_String.Replace(")", "");
                        }
                        GPIO_Select_Arr = GPIO_Select_String.ToCharArray();
                        GPIO.Content = SelectedGpio;
                        MODE.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[0]) - '0'; //string转整型
                        DIR.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[1]) - '0'; //string转整型
                        DOUT.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[2]) - '0'; //string转整型
                        DIN.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[3]) - '0'; //string转整型
                        DRIVE.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[4]) - '0'; //string转整型
                        SMT.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[5]) - '0'; //string转整型
                        IES.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[6]) - '0'; //string转整型
                        PULLEN.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[7]) - '0'; //string转整型
                        PULLSEL.SelectedIndex = Convert.ToInt32(GPIO_Select_Arr[8]) - '0'; //string转整型
                        if (Gpio_Support_R0R1 == 1)
                        {
                            if (GPIO_Select_Arr[9] == '0' && GPIO_Select_Arr[10] == '0')
                            {
                                R1R0_Value.SelectedIndex = 0;
                            }
                            else if (GPIO_Select_Arr[9] == '0' && GPIO_Select_Arr[10] == '1')
                            {
                                R1R0_Value.SelectedIndex = 1;
                            }
                            else if (GPIO_Select_Arr[9] == '1' && GPIO_Select_Arr[10] == '0')
                            {
                                R1R0_Value.SelectedIndex = 2;
                            }
                            else
                            {
                                R1R0_Value.SelectedIndex = 3;
                            }
                        }
                        VALUE.Content = GPIO_Select_String;
                        MODE.Items.Refresh();
                        DIR.Items.Refresh();
                        DIN.Items.Refresh();
                        PULLEN.Items.Refresh();
                        PULLSEL.Items.Refresh();
                        IES.Items.Refresh();
                        SMT.Items.Refresh();
                        DRIVE.Items.Refresh();

                        GPIO_Select_Str_Print = Regex.Replace(GPIO_Select_Str_Print, @"(?<=.{0}).{1}", " $0"); //每个字符串中间加入空格,供打印使用
                        TextBox1.Text = "获取GPIO" + SelectedGpio + " : " + GPIO_Select_Str_Print;
                    }
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private async Task AsyncTask_Get_All_Gpio()
        {
            string Text = Environment.CurrentDirectory;
            try
            {
                await Task.Run(() =>
                {
                    p.StartInfo.Arguments = "/c adb shell cat  " + SysPath;

                    p.Start();
                    string outtr = p.StandardOutput.ReadToEnd();
                    p.Close();
                    Action action_text = () =>
                    {
                        for (i = 0; i < 178; i++)
                        {
                            TextBox1.DataContext = null;
                            //TextBox1.Text = TextBox1.Text + "\r\n" + GpioArrayStr[i];
                            TextBox1.Text = outtr;
                        }
                    };

                    TextBox1.Dispatcher.BeginInvoke(action_text);
                });
            }
            catch
            {

            }

        }

        private async void Button_Click_Get_All_Gpio(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    TextBox1.Text = "正在获取全部GPIO配置数据...";
                    await AsyncTask_Get_All_Gpio();
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Set_Gpio_Value(object sender, RoutedEventArgs e)
        {
            try
            {
                p.Start();//启动程序
                //p.StandardInput.WriteLine("adb root");//目录装到C盘

                char[] GPIO_Select_Arr1 = new char[GPIO_Select_Arr.Length];
                GPIO_Select_Arr.CopyTo(GPIO_Select_Arr1, 0);

                GPIO_Select_Arr1[4] = GPIO_Select_Arr[7]; //PULLEN 由于mtk_gpio_store_pin函数中序号和show的不一样，需要交换
                GPIO_Select_Arr1[5] = GPIO_Select_Arr[8];//PULLSEL
                GPIO_Select_Arr1[7] = GPIO_Select_Arr[5];// SMT
                GPIO_Select_Arr1[8] = GPIO_Select_Arr[4];// DRIVE
                string GPIO_Select_Str = String.Concat(GPIO_Select_Arr1);

                string GPIO_Select_Str_Print = String.Concat(GPIO_Select_Arr);//供打印
                GPIO_Select_Str_Print = Regex.Replace(GPIO_Select_Str_Print, @"(?<=.{0}).{1}", " $0"); //每个字符串中间加入空格
                TextBox1.Text = "设置GPIO" + SelectedGpio + " : " + GPIO_Select_Str_Print; //打印

                GPIO_Select_Str = GPIO_Select_Str.Replace("(", "");
                GPIO_Select_Str = GPIO_Select_Str.Replace(")", "");
                //GPIO_Select_Str = Regex.Replace(GPIO_Select_Str.Replace("（", "(").Replace("）", ")"), @"\([^\(]*\)", "");//去除括号及括号内的内容

                //GPIO_Select_Str = Regex.Replace(GPIO_Select_Str, @"(?<=.{1}).{1}", " $0"); //每个字符串中间加入空格
                string cmd = "echo set " + SelectedGpio + " " + GPIO_Select_Str + " > " + SysPath;
                string cmd1 = adb + " " + "\"" + cmd + "\"";
                p.StandardInput.WriteLine(cmd1);
                p.StandardInput.WriteLine("exit");//结束标志
                TextBox1.Text = TextBox1.Text + cmd1;
                // GPIO_Select_Str = GPIO_Select_Str.Replace(" ", "");
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }

        }

        private void MODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (MODE.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0": GPIO_Select_Arr[0] = '0'; break;

                    case "1": GPIO_Select_Arr[0] = '1'; break;

                    case "2": GPIO_Select_Arr[0] = '2'; break;

                    case "3": GPIO_Select_Arr[0] = '3'; break;

                    case "4": GPIO_Select_Arr[0] = '4'; break;

                    case "5": GPIO_Select_Arr[0] = '5'; break;
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void DIR_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (DIR.SelectedItem.ToString()) //获取选择的内容
                {

                    case "IN": GPIO_Select_Arr[1] = '0'; break;

                    case "OUT": GPIO_Select_Arr[1] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void DOUT_SelectionChanged3(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (DOUT.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0-Low": GPIO_Select_Arr[2] = '0'; break;

                    case "1-High": GPIO_Select_Arr[2] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void DIN_SelectionChanged4(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (DIN.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0-Low": GPIO_Select_Arr[3] = '0'; break;

                    case "1-High": GPIO_Select_Arr[3] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void DRIVE_SelectionChanged5(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (DRIVE.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0": GPIO_Select_Arr[4] = '0'; break;

                    case "1": GPIO_Select_Arr[4] = '1'; break;

                    case "2": GPIO_Select_Arr[4] = '2'; break;

                    case "3": GPIO_Select_Arr[4] = '3'; break;

                    case "4": GPIO_Select_Arr[4] = '4'; break;

                    case "5": GPIO_Select_Arr[4] = '5'; break;

                    case "6": GPIO_Select_Arr[4] = '6'; break;

                    case "7": GPIO_Select_Arr[4] = '7'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void SMT_SelectionChanged6(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (SMT.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0": GPIO_Select_Arr[5] = '0'; break;

                    case "1": GPIO_Select_Arr[5] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void IES_SelectionChanged7(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (IES.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0": GPIO_Select_Arr[6] = '0'; break;

                    case "1": GPIO_Select_Arr[6] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void PULLEN_SelectionChanged8(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (PULLEN.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0-Low": GPIO_Select_Arr[7] = '0'; break;

                    case "1-High": GPIO_Select_Arr[7] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void PULLSEL_SelectionChanged9(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (PULLSEL.SelectedItem.ToString()) //获取选择的内容
                {

                    case "0-Low": GPIO_Select_Arr[8] = '0'; break;

                    case "1-High": GPIO_Select_Arr[8] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }
        private void R0R1_SelectionChanged10(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (R1R0_Value.SelectedItem.ToString()) //获取选择的内容
                {

                    case "00": GPIO_Select_Arr[9] = '0'; GPIO_Select_Arr[10] = '0'; break;

                    case "01": GPIO_Select_Arr[9] = '0'; GPIO_Select_Arr[10] = '1'; break;

                    case "10": GPIO_Select_Arr[9] = '1'; GPIO_Select_Arr[10] = '0'; break;

                    case "11": GPIO_Select_Arr[9] = '1'; GPIO_Select_Arr[10] = '1'; break;

                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }
        private async void Button_Click_Scp_Log(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    if (Button_log.Content.ToString() == "scp获取")
                    {
                        Button_log.Content = "停止scp";
                        cmd.shell = "adb shell" + " " + "\"" + "while true; do cat /dev/scp;done" + "\"";
                        cmd_isRun = "start";
                        TextBox1.Text = " ";

                        p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + "echo 1 > /sys/class/misc/scp/scp_mobile_log" + "\"";

                        p.Start();
                        p.Close();
                        await Catch_Logs_func();
                    }
                    else
                    {
                        Button_log.Content = "scp获取";
                        cmd_isRun = "stop";
                    }
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Clear_Log(object sender, RoutedEventArgs e)
        {
            TextBox1.Text = "";
        }

        private async Task AsyncTask_Get_Version()
        {
            string Text = Environment.CurrentDirectory;
            try
            {
                string Adb_Output = null;
                string Adb_Output1 = null;
                string sim1 = null;
                string sim2 = null;
                Action action_version = () =>
                {
                    TextBox1.Text = Adb_Output + "IMEI1  :" + sim1 + "\r\n" + "IMEI2  :" + sim2;
                };

                await Task.Run(() =>
                {
                    p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + "getprop | grep -E 'ro.build.description|ro.build.user|"
                    + "ro.vendor.mediatek.version.release|ro.vendor.mediatek.platform|ro.product.build.date|ro.vendor.mediatek.version.release|"
                    + "ro.vendor.md_apps.load_verno|ro.vendor.md_apps.load_date|ro.serialno|pax.ctrl.fingerprint.id|"
                    + "pax.ctrl.camera.sub|pax.ctrl.camera.main|pax.ctrl.camera.main2|pax.ctrl.tp.version|persist.vendor.connsys.wifi_fw_ver|persist.vendor.connsys.bt_fw_ver'"
                    + "\"";

                    p.Start();
                    Adb_Output = p.StandardOutput.ReadToEnd();
                    p.Close();
                    Adb_Output = Adb_Output.Replace("[ro.vendor.mediatek.platform]", "平台：    ");
                    Adb_Output = Adb_Output.Replace("[pax.ctrl.fingerprint.id]", "Finger    ");
                    Adb_Output = Adb_Output.Replace("[pax.ctrl.camera.sub]", "前摄型号    ");
                    Adb_Output = Adb_Output.Replace("[pax.ctrl.camera.main]", "后摄型号    ");
                    Adb_Output = Adb_Output.Replace("[pax.ctrl.camera.main2]", "后幅摄型号    ");
                    Adb_Output = Adb_Output.Replace("[pax.ctrl.tp.version]", "TP/LCD    ");
                    Adb_Output = Adb_Output.Replace("[persist.vendor.connsys.wifi_fw_ver]", "WiFi    ");
                    Adb_Output = Adb_Output.Replace("[persist.vendor.connsys.bt_fw_ver]", "BT    ");
                    Adb_Output = Adb_Output.Replace("[ro.product.build.date]", "Build Time    ");
                    Adb_Output = Adb_Output.Replace("[ro.build.description]", "Build Info    ");
                    Adb_Output = Adb_Output.Replace("[ro.build.user]", "Build Author    ");
                    Adb_Output = Adb_Output.Replace("[ro.product.build.date.utc]", "Build Time Utc    ");
                    Adb_Output = Adb_Output.Replace("[ro.vendor.md_apps.load_verno]", "Modem Ver    ");
                    Adb_Output = Adb_Output.Replace("[ro.vendor.md_apps.load_date]", "Modem Build Time    ");
                    Adb_Output = Adb_Output.Replace("[ro.serialno]", "SN    ");
                    Adb_Output = Adb_Output.Replace("[ro.vendor.mediatek.version.release]", "SW Ver    ");
                    Adb_Output = Adb_Output.Replace("[", "");
                    Adb_Output = Adb_Output.Replace("]", "");

                    //TextBox1.Text = Adb_Output;

                    const string SIM1 = @"service call iphonesubinfo 3 i32 2 | grep -o '[0-9a-f]\{8\} ' | tail -n+3 | while read a; do echo -n \\u${a:4:4}\\u${a:0:4}; done";
                    const string SIM2 = @"service call iphonesubinfo 3 i32 1 | grep -o '[0-9a-f]\{8\} ' | tail -n+3 | while read a; do echo -n \\u${a:4:4}\\u${a:0:4}; done";

                    p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + SIM1 + "&&" + SIM2 + "\"";
                    p.Start();

                    Adb_Output1 = p.StandardOutput.ReadToEnd();
                    if (Adb_Output1.Length >= 5)
                    {
                        sim1 = Adb_Output1.Substring(0, 15);
                        sim2 = Adb_Output1.Remove(0, 15);
                    }
                    TextBox1.Dispatcher.BeginInvoke(action_version);
                    p.Close();
                });
            }
            catch
            {

            }
        }

        private async void Button_Click_Get_Version(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    TextBox1.Text = "正在获取机型数据...";
                    await AsyncTask_Get_Version();
                }
            }
            catch
            {
                //  MessageBox.Show("某些版本信息不存在");
            }
        }

        private void Button_Click_help(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("[MODE] 当前pin处于的mode" + "\r\n" + "[DIR] 0：input pin, 1：output pin" + "\r\n" + "[DOUT] 输出值"+"\r\n"+ "[DIN] 输入值"+"\r\n" + "[DRIVE] 驱动能力, 一般可取值0～7"+ "\r\n" + "[SMT] 使能施密特触发器" + "\r\n" + "[IES] 输入使能，1：input信号有效 0：input信号无效" + "\r\n" + "[PULL_EN] 只对input pin有效，使能上 / 下拉" + "\r\n" + "[PULL_SEL] 只对input pin有效，1：上拉 0：下拉" + "\r\n" + "([R1][R0]) 当前GPIO pin的（上下拉）并联电阻的使能状态" + "\r\n" + "1 0表示enable R1，disable R0"+ "\r\n" + "0 1表示disable R1，enable R0" + "\r\n" + "1 1表示enable R1， enable R0 ");
            Process.Start("https://github.com/wuguangnan110/MTK_GPIO_TOOLS");
        }

        /*
        private void Button_Click_Get_PMIC_Info(object sender, RoutedEventArgs e)
        {
            try
            {
                p.StartInfo.Arguments = "/c adb shell cat /sys/kernel/debug/regulator/regulator_summary";

                p.Start();

                string Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = p.StartInfo.Arguments + "\r\n";
                TextBox1.Text = TextBox1.Text + "MT6357LDO列表如下：" + "\r\n";
                TextBox1.Text = TextBox1.Text + Adb_Output;
                p.Close();
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }
*/
        private async Task AsyncTask_Get_Mem_Info()
        {
            string Adb_Output = null;
            try
            {
                Action action_mem = () =>
                {
                    TextBox1.Text = Adb_Output;
                };
                await Task.Run(() =>
            {
                p.StartInfo.Arguments = "/c adb shell cat /proc/meminfo";
                p.Start();
                Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Dispatcher.BeginInvoke(action_mem);
                p.Close();
            });
            }
            catch
            {

            }

        }

        private async void Button_Click_Get_Mem_Info(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    TextBox1.Text = "正在获取内存数据...";
                    await AsyncTask_Get_Mem_Info();
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }


        private async Task AsyncTask_Get_Battery_Info()
        {
            string Text = Environment.CurrentDirectory;
            try
            {
                string Adb_Output = null;
                Action action_battery = () =>
                {
                    //TextBox1.Text = TextBox1.Text + "  current：";
                    TextBox1.Text = Adb_Output;
                };
                Action action_battery_reg = () =>
                {
                    TextBox1.Text = TextBox1.Text + "\r\n" + "\r\n" + "MT6371寄存器如下：" + "\r\n";
                    TextBox1.Text = TextBox1.Text + Adb_Output;
                };
                await Task.Run(() =>
                {
                    p.StartInfo.Arguments = "/c adb shell dumpsys battery";

                    p.Start();

                    Adb_Output = p.StandardOutput.ReadToEnd();
                    p.Close();
                    TextBox1.Dispatcher.BeginInvoke(action_battery);

                    p.StartInfo.Arguments = "/c adb shell cat /sys/class/power_supply/battery/current_now";
                    p.Start();
                    Adb_Output = p.StandardOutput.ReadToEnd();
                    int value = Convert.ToInt32(Adb_Output);
                    value = value / 1000;
                    Adb_Output = Convert.ToString(value);
                    //TextBox1.Dispatcher.BeginInvoke(action_battery);
                    p.Close();

                    p.StartInfo.Arguments = "/c adb shell cat /sys/kernel/debug/rt-regmap/mt6370_pmu/regs";
                    p.Start();
                    Adb_Output = p.StandardOutput.ReadToEnd();
                    TextBox1.Dispatcher.BeginInvoke(action_battery_reg);

                    p.Close();


                });
            }
            catch
            {

            }
        }

        private async void Button_Click_Get_Battery_Info(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adb_status)
                {
                    TextBox1.Text = "正在获取电池数据...";
                    await AsyncTask_Get_Battery_Info();
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private async Task<string> AsyncTask_ScreenCap()
        {
            string Text = Environment.CurrentDirectory;
            try
            {
                return await Task.Run(() =>
                {
                    string Path = Environment.CurrentDirectory + "\\Images";
                    if (!System.IO.Directory.Exists(Path))
                    {
                        System.IO.Directory.CreateDirectory(Path);//创建该文件夹
                    }
                    p.StartInfo.Arguments = "/c adb shell screencap -p /sdcard/screen.png";
                    p.Start();
                    p.Close();
                    Delay(1200); //延时，防止framebuffer未生成图片

                    p.StartInfo.Arguments = "/c adb pull /sdcard/screen.png ./Images";
                    p.Start();
                    p.Close();

                    Delay(1200); //延时
                    if (File.Exists(Environment.CurrentDirectory + "\\Images\\screen.png"))
                    {
                        File.Move(Environment.CurrentDirectory + "\\Images\\screen.png", Environment.CurrentDirectory + "\\Images\\screen" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");//将后缀加入时间戳
                    }

                    return Text;
                });
            }
            catch
            {
                return "截屏失败";
            }

        }

        private async void Button_Click_ScreenCap(object sender, RoutedEventArgs e)
        {
            if (adb_status)
            {
                try
                {
                    TextBox1.Text = "正在从手机中截屏...";
                    await AsyncTask_ScreenCap();
                    TextBox1.Text += "\r\n" + "截屏成功！保存路径：" + Environment.CurrentDirectory + "\\Images\\screen" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
                }
                catch
                {
                    MessageBox.Show("请连接手机adb");
                }
            }
        }

        private async Task<string> AsyncTask_Get_MTK_Log()
        {
            string Text = Environment.CurrentDirectory + "\\log";
            try
            {
                return await Task.Run(() =>
                {
                    p.StartInfo.Arguments = "/c adb pull /sdcard/debuglogger ./log";

                    p.Start();
                    string Adb_Output = p.StandardOutput.ReadToEnd();
                    p.Close();
                    if (Directory.Exists(Log_Path + "\\debuglogger"))
                    {
                        Directory.Move(Log_Path + "\\debuglogger", Log_Path + "\\debuglogger" + DateTime.Now.ToString("yyyyMMddHHmm"));//将后缀加入时间戳
                    }

                    return Text;
                });
            }
            catch
            {
                return "获取打印失败";
            }

        }
        private async void Button_Click_Get_MTK_Log(object sender, RoutedEventArgs e)
        {
            if (adb_status)
            {
                try
                {
                    TextBox1.Text = "正在从手机中拷贝...";
                    TextBox1.Text = "拷贝完成，路径：" + "\r\n" + await AsyncTask_Get_MTK_Log() + "\\debuglogger" + DateTime.Now.ToString("yyyyMMddHHmm");

                }
                catch
                {
                    MessageBox.Show("请连接手机adb");
                }
            }
        }

        public void Clear_Files(string path)
        {
            if (Directory.Exists(path))
            {
                //获取该路径下的文件路径
                string[] filePathList = Directory.GetFiles(path);
                foreach (string filePath in filePathList)
                {
                    File.Delete(filePath);
                }
            }
        }

        public void Clear_Directory(string path)
        {
            if (Directory.Exists(path))
            {
                //获取该路径下的文件夹路径
                string[] directorsList = Directory.GetDirectories(path);
                foreach (string directory in directorsList)
                {
                    Directory.Delete(directory, true);//删除该文件夹及该文件夹下包含的文件
                }
            }
        }

        private void Button_Click_Clear_MTK_Log(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Log_Path))
                {
                    Clear_Directory(Log_Path);
                    TextBox1.Text = "MTK log文件删除成功！" + "\r\n" + Environment.CurrentDirectory + "\\log\\debuglogger";
                }
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }


        /// <summary>
        /// 获得指定路径下所有文件名
        /// </summary>
        /// <param name="sw">文件写入流</param>
        /// <param name="path">文件写入流</param>
        /// <param name="word">文件名过滤</param>
        public void getFileFilter(string path, string word, List<FileInfo> lst)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (FileInfo f in root.GetFiles(word + "*"))
            {
                lst.Add(f);
            }
        }

        /// <summary>
        /// 获得指定路径下所有子目录名
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="word">文件名过滤</param>
        public List<FileInfo> getDirectoryFilter(string path, string word, List<FileInfo> lst)
        {
            if (!Directory.Exists(path))
            {
                return lst;
            }
            getFileFilter(path, word, lst);
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                getDirectoryFilter(d.FullName, word, lst);
            }
            return lst;
        }

        private async Task<bool> AsyncTask_Kernel_To_UTC()
        {
            string Text = Environment.CurrentDirectory + "\\log";
            ProcessStartInfo ProcessInfo;
            Process Process;
            int ExitCode;

            try
            {
                return await Task.Run(() =>
                {

                    ProcessInfo = new ProcessStartInfo();
                    ProcessInfo.FileName = "python2.exe";

                    ProcessInfo.CreateNoWindow = true;
                    ProcessInfo.UseShellExecute = false;
                    ProcessInfo.RedirectStandardOutput = true;
                    List<FileInfo> lst = new List<FileInfo>();
                    if (!Directory.Exists(Log_Path))
                    {
                        return false;
                    }

                    List<FileInfo> lstFiles = getDirectoryFilter(Log_Path, "kernel_log", lst);
                    foreach (FileInfo shpFile in lst)
                    {
                        Action action_text = () =>
                        {
                            TextBox1.AppendText("\r\n" + shpFile.FullName);
                        };

                        TextBox1.Dispatcher.BeginInvoke(action_text);
                        ProcessInfo.Arguments = Environment.CurrentDirectory + "\\tools\\kernel_to_utc.py " + shpFile.FullName;

                        Process = Process.Start(ProcessInfo);
                        Process.WaitForExit();
                        ExitCode = Process.ExitCode;
                        Process.Close();
                    }


                    return true;
                });
            }
            catch
            {
                return false;
            }
        }

        private async void Button_Click_Kernel_To_UTC(object sender, RoutedEventArgs e)
        {
            bool ret;
            TextBox1.Text = "开始转换log目录下kernel log为utc文件...";
            ret = await AsyncTask_Kernel_To_UTC();
            if (ret)
            {
                TextBox1.AppendText("\r\n" + "转换完成!!!");
            }
            else
            {
                TextBox1.AppendText("\r\n" + "文件不存在，转换失败!!!");
            }
        }


        public string Execute(string dosCommand, int milliseconds)
        {
            string output = "";     //输出字符串
            if (dosCommand != null && dosCommand != "")
            {
                Process process = new Process();     //创建进程对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";      //设定需要执行的命令
                startInfo.Arguments = "/C " + dosCommand;   //设定参数，其中的“/C”表示执行完命令后马上退出
                startInfo.UseShellExecute = false;     //不使用系统外壳程序启动
                startInfo.RedirectStandardInput = false;   //不重定向输入
                startInfo.RedirectStandardOutput = true;   //重定向输出
                startInfo.CreateNoWindow = true;     //不创建窗口
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())       //开始进程
                    {
                        if (milliseconds == 0)
                            process.WaitForExit();     //这里无限等待进程结束
                        else
                            process.WaitForExit(milliseconds);  //当超过这个毫秒后，不管执行结果如何，都不再执行这个DOS命令
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        private void Button_Click_Unlock_Android(object sender, RoutedEventArgs e)
        {
            if (adb_status)
            {
                TextBox1.Text = "开始对机器进行解锁操作...";
                ProgressBar.Visibility = Visibility.Visible;
                this.RunBackgroundWorkerStyle();
            }
            else
            {
                TextBox1.Text = "请接入设备，并确保为userdebug或者eng版本...";
            }

        }

        private void RunBackgroundWorkerStyle()
        {
            // Make sure we specify that we support progress reporting and cancellation.
            this.ProgressBar.Value = 0;
            this.ProgressBar.Maximum = 90;
            this.backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            // Handler for progress changed events. Update the ProgressBar,
            // process the data and log an event.
            this.backgroundWorker.ProgressChanged += (s, pe) =>
            {
                //修改进度条的显示。
                this.ProgressBar.Value = pe.ProgressPercentage;
                TextBox1.AppendText("\r\n" + "正在执行命令：" + pe.UserState.ToString());
            };

            this.backgroundWorker.DoWork += (s, pe) =>
            {
                //string resultStr = "";
                //RunCMDCommand(out resultStr,"adb reboot bootloader", "fastboot flashing unlock", "adb wait-for-device", "adb root", "adb disable-verity", "adb reboot", "adb wait-for-device", "adb root", "adb remount");
                //Console.WriteLine(resultStr);   
                int sum = 0;
                string result = Execute("adb reboot bootloader", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb reboot bootloader");
                result = Execute("fastboot flashing unlock", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "fastboot flashing unlock");
                TextBox1.Dispatcher.Invoke(
                    new Action(
                        delegate
                        {
                            TextBox1.AppendText("\r\n" + "请手动长按power键10秒重启！");
                            TextBox1.AppendText("\r\n" + "请手动长按power键10秒重启！");
                            TextBox1.AppendText("\r\n" + "请手动长按power键10秒重启！");
                        }
                    )
                );
                result = Execute("adb wait-for-device", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb wait-for-device 发现设备");
                result = Execute("adb root", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb root");
                result = Execute("adb disable-verity", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb disable-verity");
                result = Execute("adb reboot", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb reboot");
                result = Execute("adb wait-for-device", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb wait-for-device 发现设备");
                result = Execute("adb root", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb root");
                result = Execute("adb remount", 0);
                sum += 10;
                backgroundWorker.ReportProgress(sum, "adb remount");
            };

            // When the worker is finished, tidy up.
            this.backgroundWorker.RunWorkerCompleted += (s, pe) =>
            {
                //如果用户取消了当前操作就关闭窗口。
                if (pe.Cancelled)
                {
                    this.Close();
                }
                TextBox1.AppendText("\r\n" + "完成解锁");
                MessageBox.Show("完成");
                ProgressBar.Visibility = Visibility.Collapsed;
            };
            this.backgroundWorker.RunWorkerAsync(new Tuple<int, int, int>(100, 500, 100));
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Strings">文件内容</param>
        public static void WriteFile(string Path, string Strings)
        {
            if (!System.IO.File.Exists(Path))
            {
                //Directory.CreateDirectory(Path);
                System.IO.FileStream f = System.IO.File.Create(Path);
                f.Close();
                f.Dispose();
            }
            System.IO.StreamWriter f2 = new System.IO.StreamWriter(Path, true, System.Text.Encoding.UTF8);
            f2.WriteLine(Strings);
            f2.Close();
            f2.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string log = Log_Path + "\\" + "log" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
            if (TextBox1.Text != String.Empty)
            {
                WriteFile(log, TextBox1.Text);
                TextBox1.AppendText("\r\n" + "log保存成功！路径如下：");
                TextBox1.AppendText("\r\n" + log);
            }
        }

        private async Task AsyncTask_Get_Dumpsys()
        {
            string Adb_Output = null;
            string dumpsys_log = Log_Path + "\\" + "dumpsys_log" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
            try
            {
                Action action_mem = () =>
                {
                    //TextBox1.Text = Adb_Output;
                };
                await Task.Run(() =>
                {
                    p.StartInfo.Arguments = "/c adb shell dumpsys";
                    p.Start();
                    Adb_Output = p.StandardOutput.ReadToEnd();
                    WriteFile(dumpsys_log, Adb_Output);
                    p.Close();
                });
            }
            catch
            {

            }
        }
        private async void Button_dumpsys_Click(object sender, RoutedEventArgs e)
        {
            if (adb_status)
            {
                TextBox1.Text = "开始对机器进行dumpsys操作...";
                await AsyncTask_Get_Dumpsys();
                TextBox1.AppendText("\r\n" + "完成dumpsys操作！保存路径:");
                TextBox1.AppendText("\r\n" + Log_Path);
            }
        }
    }
}
