using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AutoHDR
{
    static class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static List<string> programName = new List<string>();
        public static bool working=true;
        public static int waittime = 10, holdtime = 250;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Read();
            // 创建一个新的线程来监视程序状态
            Thread monitorThread = new Thread(WatchHDR);

            // 启动监视线程
            monitorThread.Start();

            // 等待监视线程完成
            //monitorThread.Join();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AutoHDR());
        }

        static void Read()
        {
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(userDocumentsPath, "AutoHDR.txt");
            string setting = Path.Combine(userDocumentsPath, "setting.txt");
            try
            {
                programName = File.ReadAllLines(filePath).ToList();
                string[] strings = File.ReadAllLines(setting);
                holdtime = Convert.ToInt32(strings[0]);
                waittime = Convert.ToInt32(strings[1]);
            }
            catch (Exception)
            {
                File.Create(filePath);
            }
        }

        public static void Write()
        {
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);            
            string filePath = Path.Combine(userDocumentsPath, "AutoHDR.txt");
            string setting = Path.Combine(userDocumentsPath, "setting.txt");

            // 写入数据到文件
            File.WriteAllLines(filePath, programName.ToArray());
            File.WriteAllLines(setting, new string[] { holdtime.ToString(), waittime.ToString() });
        }
        public static void WatchHDR()
        {
            bool HDRstate = false;
            while (working)
            {
                bool needHDR = false;
                // 检查进程列表中是否存在要监视的程序
                Process[] processAll = Process.GetProcesses();
                foreach (Process pro in processAll)
                {
                    if (needHDR)
                        break;
                    foreach (string game in programName)
                    {
                        if (needHDR)
                            break;
                        if (pro.ProcessName == game)
                        {
                            needHDR = true;
                            if (!HDRstate)
                            {
                                //Process.Start(HDRon);
                                HDRTrun(HDRstate);
                                HDRstate = true;
                                Console.WriteLine("HDR-ON");
                                break;
                            }
                        }
                    }
                }
                if (HDRstate && !needHDR)
                {
                    //Process.Start(HDRoff);
                    HDRTrun(HDRstate);
                    HDRstate = false;
                    Console.WriteLine("HDR-OFF");
                    //break;
                }
                Thread.Sleep(500); // 等待1秒后再次检查程序状态
            }
        }

        static void HDRTrun(bool HDRstate)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo("ms-settings:display");
            Process.Start(startInfo);

            string windowTitle = "设置";
            #region
            //CultureInfo currentUiCulture = CultureInfo.CurrentUICulture;
            //Console.WriteLine("当前系统UI语言: " + currentUiCulture.DisplayName);
            //if (currentUiCulture.DisplayName == "English (United States)")
            //    windowTitle = "Settings";
            //else if (currentUiCulture.DisplayName == "中文(中华人民共和国)")
            //    windowTitle = "设置";
            #endregion
            //Console.WriteLine(startInfo.FileName);
            Thread.Sleep(holdtime);
            IntPtr hWnd = FindWindow(null, windowTitle);

            if (hWnd != IntPtr.Zero)
            {
                Console.WriteLine(hWnd.ToString());
                SetForegroundWindow(hWnd);
                Thread.Sleep(holdtime);
                if (!HDRstate)
                {
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait(" ");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait(" ");
                }
                else
                {
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait(" ");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait("{TAB}");
                    Thread.Sleep(waittime);
                    SendKeys.SendWait(" ");
                }
                CloseWindow("SystemSettings");
            }
        }

        static void CloseWindow(string windowname)
        {
            try
            {
                Process[] win = Process.GetProcessesByName(windowname);
                foreach (Process process in win)
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
            }
        }

    }
}
