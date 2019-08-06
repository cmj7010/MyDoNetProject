using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace JM.CARRIER.Tools
{
    public class AutoDeleteMessageBox
    {
        #region 调用自动关闭窗口API
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        #endregion
        public const int WM_CLOSE = 0x10;
        public AutoDeleteMessageBox()
        {
            StartKiller();
        }

        #region 自动关闭模态窗口
        public void StartKiller()
        {
            Timer timer = new Timer();
            timer.Interval = 900; // 1秒 
            timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            KillMessageBox();
            // 停止Timer  
            ((Timer)sender).Stop();
        }

        private void KillMessageBox()
        {
            // 依MessageBox的标题,找出MessageBox的视窗  
            IntPtr ptr = FindWindow(null, "MessageBox");
            if (ptr != IntPtr.Zero)
            {
                // 找到则关闭MessageBox视窗  
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }
        #endregion
    }
}
