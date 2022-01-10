using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string adb = "adb shell";
        int i = 0;
        static int value = 0;
        string[] GpioArrayStr;
        string GPIO_Select_String;
        char[] GPIO_Select_Arr = null;
        string SelectedGpio;
        string SysPath = "/sys/devices/platform/pinctrl@1000b000/mt_gpio";
        private DispatcherTimer Time = null;
        int Gpio_Support_R0R1 = 0;

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
            Time.Tick += Time_Tick;
            Time.Start();

        }
        void Time_Tick(object sender, EventArgs e)
        {
            
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c adb devices";
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string outtr = p.StandardOutput.ReadToEnd();

            p.Close();
            outtr = outtr.Replace("List of devices attached", "");
            outtr = outtr.Replace("device", "");
            outtr = outtr.Replace("\r", "");
            outtr = outtr.Replace("\n", "");
            if (outtr.Length == 0)
            {
                led.Fill = new SolidColorBrush(Colors.Red);
                led.Stroke = new SolidColorBrush(Colors.Red);
                if (value == 0)
                {
                    MessageBox.Show("请连接adb设备");
                    value++;
                }
            }
            else
            {
                led.Fill = new SolidColorBrush(Colors.Green);
                led.Stroke = new SolidColorBrush(Colors.Green);
                Time.Stop();
                if (value == 1)
                {
                    MessageBox.Show("找到adb设备：" + outtr);
                    value = 0;
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void Button_Click_Get_Gpio(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBox1.SelectedIndex != -1)
                {
                    SelectedGpio = comboBox1.SelectedItem.ToString();
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = "/c adb shell cat  " + SysPath;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;

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
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBox1.SelectedIndex != -1)
                {
                    SelectedGpio = comboBox1.SelectedItem.ToString();
                    Gpio_Support_R0R1 = 0; //每次选择GPIO时默认不支持R0R1
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = "/c adb shell cat  " + SysPath;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;

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
                    GPIO_Select_String = GPIO_Select_String.Remove(4,1);//由于Android11打印第五位多个0，如： 028: 3001000111   需要去掉这个才能正常
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
                    if(Gpio_Support_R0R1 == 1)
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
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Get_All_Gpio(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell cat  " + SysPath;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                string outtr = p.StandardOutput.ReadToEnd();
                p.Close();
                for (i = 0; i < 180; i++)
                {
                    TextBox1.DataContext = null;
                    //TextBox1.Text = TextBox1.Text + "\r\n" + GpioArrayStr[i];
                    TextBox1.Text = outtr;
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
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息                                       
                p.StartInfo.RedirectStandardError = false;// p.StartInfo.CreateNoWindow = false;//不显示程序窗口
                p.StartInfo.CreateNoWindow = true;          //设置不显示窗口  
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
                string cmd = "echo set "  + SelectedGpio + " " + GPIO_Select_Str + " > " + SysPath;
                string cmd1 = adb  + " " + "\"" + cmd + "\"";
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
        private void Button_Click_Check_Path(object sender, RoutedEventArgs e)
        {
            try
            { 
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + "find /sys/devices/platform -name mt_gpio" + "\"";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                SysPath = p.StandardOutput.ReadToEnd();
                SysPath = SysPath.Replace("\n", "");
                SysPath = SysPath.Replace("\r", "");
                MessageBox.Show("find sys path : " + SysPath);
                TextBox2.Text = SysPath;
                p.Close();
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Clear_Log(object sender, RoutedEventArgs e)
        {
            TextBox1.Text = " ";
        }

        private void Button_Click_Get_Version(object sender, RoutedEventArgs e)
        {
            try
            { 
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";

                p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + "getprop | grep -E 'ro.build.description|ro.build.user|ro.vendor.mediatek.version.release|ro.vendor.mediatek.platform|ro.product.build.date|ro.vendor.mediatek.version.release|ro.vendor.md_apps.load_verno|ro.vendor.md_apps.load_date|vendor.gsm.serial'" + "\"";
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                
                p.Start();
                string Adb_Output = p.StandardOutput.ReadToEnd();
                p.Close();
                Adb_Output = Adb_Output.Replace("[ro.product.build.date]", "Build Time    ");
                Adb_Output = Adb_Output.Replace("[ro.build.description]", "Build Info    ");
                Adb_Output = Adb_Output.Replace("[ro.build.user]", "Build Author    ");
                Adb_Output = Adb_Output.Replace("[ro.product.build.date.utc]", "Build Time Utc    ");
                //Adb_Output = Adb_Output.Replace("[ro.product.vendor.device]", "MS Board    ");
                Adb_Output = Adb_Output.Replace("[ro.vendor.md_apps.load_verno]", "Modem Ver    ");
                Adb_Output = Adb_Output.Replace("[ro.vendor.md_apps.load_date]", "Modem Build Time    ");
                Adb_Output = Adb_Output.Replace("[ro.vendor.mediatek.platform]", "BB Chip    ");
                Adb_Output = Adb_Output.Replace("[vendor.gsm.serial]", "Bar Code    ");
                //Adb_Output = Adb_Output.Replace("[ro.build.version.release]", "Android Ver    "); 
                Adb_Output = Adb_Output.Replace("[ro.vendor.mediatek.version.release]", "SW Ver    ");
                Adb_Output = Adb_Output.Replace("[", "");
                Adb_Output = Adb_Output.Replace("]", "");
                TextBox1.Text = Adb_Output;

                const string SIM1 = @"service call iphonesubinfo 3 i32 2 | grep -o '[0-9a-f]\{8\} ' | tail -n+3 | while read a; do echo -n \\u${a:4:4}\\u${a:0:4}; done";
                const string SIM2 = @"service call iphonesubinfo 3 i32 1 | grep -o '[0-9a-f]\{8\} ' | tail -n+3 | while read a; do echo -n \\u${a:4:4}\\u${a:0:4}; done";

                p.StartInfo.Arguments = "/c adb shell" + " " + "\"" + SIM1 + "&&" + SIM2 + "\"";
                p.Start();

                

                Adb_Output = p.StandardOutput.ReadToEnd();
                string sim1 = Adb_Output.Substring(0, 15);
                string sim2 = Adb_Output.Remove(0, 15);
                TextBox1.Text = TextBox1.Text + "IMEI1  :"  + sim1 + "\r\n" + "IMEI2  :" + sim2;
                p.Close();
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

        private void Button_Click_Get_PMIC_Info(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell cat /sys/kernel/debug/regulator/regulator_summary";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                string Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = p.StartInfo.Arguments + "\r\n";
                TextBox1.Text = TextBox1.Text + "MT6357LDO列表如下：" + "\r\n";
                TextBox1.Text = TextBox1.Text + Adb_Output;
                p.Close();
                /*
                p.StartInfo.Arguments = "/c adb shell cat /sys/kernel/debug/mtk_pmic/dump_pmic_reg";
                p.Start();
                Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = TextBox1.Text + "\r\n" + "MT6357 寄存器列表如下：" + "\r\n";
                TextBox1.Text = TextBox1.Text + Adb_Output;
                p.Close();
                */
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Get_Mem_Info(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell cat /proc/meminfo";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                string Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = Adb_Output;
                p.Close();
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Get_Battery_Info(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell dumpsys battery";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                string Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = Adb_Output;
                p.Close();

                p.StartInfo.Arguments = "/c adb shell cat /sys/class/power_supply/battery/current_now";
                p.Start();
                Adb_Output = p.StandardOutput.ReadToEnd();
                int value = Convert.ToInt32(Adb_Output);
                value = value / 1000;
                Adb_Output = Convert.ToString(value);
                TextBox1.Text = TextBox1.Text + "  current：";
                TextBox1.Text = TextBox1.Text + Adb_Output;
                p.Close();

                p.StartInfo.Arguments = "/c adb shell cat /sys/kernel/debug/rt-regmap/mt6370_pmu/regs";
                p.Start();
                Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = TextBox1.Text + "\r\n" + "\r\n" + "MT6371寄存器如下：" + "\r\n";
                TextBox1.Text = TextBox1.Text + Adb_Output;
                p.Close();

            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_ScreenCap(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell screencap -p /sdcard/screen.png";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();
                string Adb_Output = p.StandardOutput.ReadToEnd();
                p.Close();
                string Path = Environment.CurrentDirectory + "\\Images";
                if (!System.IO.Directory.Exists(Path))
                {
                    System.IO.Directory.CreateDirectory(Path);//创建该文件夹
                }
                p.StartInfo.Arguments = "/c adb pull /sdcard/screen.png ./Images";
                p.Start();
                p.Close();

                TextBox1.Text = "截屏成功！保存路径：" + Environment.CurrentDirectory + "\\Images";
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Get_MTK_Log(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox1.Text = "正在从手机中拷贝...";
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb pull /sdcard/debuglogger .";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                string Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = "拷贝完成，路径："+ Environment.CurrentDirectory + "\\debuglogger";
                p.Close();
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Reboot(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c adb shell reboot";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                TextBox1.Text = "reboot successful!";
                p.Close();
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }

        private void Button_Click_Clear_MTK_Log(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";

                //string test = System.Text.RegularExpressions.Regex.Escape("service call iphonesubinfo 1 | awk -F \"'\" '{print $2}' | sed '1 d' | tr -d '.' | awk '{print}' ORS=");
                p.StartInfo.Arguments = "/c adb shell rm -rf /sdcard/debuglogger .";

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                string Adb_Output = p.StandardOutput.ReadToEnd();
                TextBox1.Text = "MTK log文件删除成功！";
                p.Close();
            }
            catch
            {
                MessageBox.Show("请连接手机adb");
            }
        }
    }
}
