using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using JM.CARRIER.Tools;
using JM.CARRIER.TcpComm;
using static JM.CARRIER.TcpComm.tcpConnState;
using System.Timers;
using System.Text.RegularExpressions;
using JM.Util;
using System.Windows.Input;
using System.Data;

namespace JM.CARRIER
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Communication TcpComm = null;

        List<ModalWeightInfoTable> WeightList = new List<ModalWeightInfoTable>();
        List<ModalWeightInfoTable> UnSendList = new List<ModalWeightInfoTable>();
        List<ModalSpurtCodeInfo> SpurtList = new List<ModalSpurtCodeInfo>();

        WeightInfoTable dal = new WeightInfoTable();
        DataRowView SelectRow = null;
        string CacheRece = "";
        int number = 0;
        int LastCount = 0;
        int SpurtTimeCount = 20;
        tcpUtility tcputility = null;
        System.Timers.Timer timerSpurt = null; //为喷码写一个定时器
        private bool timerBool = true;
        private static readonly object timerLock = new object();
        public MainWindow()
        {
            InitializeComponent();
            number = dal.QueryData();
            timerSpurt = new System.Timers.Timer();
            timerSpurt.Interval = 1000;     //时间间隔为一秒钟
            timerSpurt.Enabled = false;
            timerSpurt.Elapsed += ElapseSpurtSend;
            timerBool = true;

            string ip = System.Configuration.ConfigurationManager.AppSettings["ServerIp"];
            int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);
            IPAddress address = IPAddress.Parse(ip);
            TcpComm = new Communication(ip, port);
            TcpComm.newTcpMessage += Receive_Weight;
            TcpComm.newTcpStatus += server_ClientConnected;

            string spurtip = System.Configuration.ConfigurationManager.AppSettings["SpurtIp"];
            int spurtport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SpurtPort"]);
            tcputility = new tcpUtility(spurtip, spurtport);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textInterval.Text = timerSpurt.Interval.ToString();
            TcpComm.Connect();
        }

        
        private void ElapseSpurtSend(object Sender, ElapsedEventArgs e)
        {
            if(timerBool)
            {
                lock (timerLock)
                {
                    if(timerBool)
                    {
                        timerBool = false;
                        int CurrentCount = 0; //喷码枪的初始值是1 

                        string recStr = tcputility.SendAndReceive(String.Format("GET_COUNTER_INFO 1\n"));
                        if (recStr.Contains("ACK-GET_COUNTER_INFO"))
                        {
                            CurrentCount = Convert.ToInt32(recStr.Split(',')[2]);
                        }

                        if (CurrentCount > LastCount) //说明喷码了，或者到了时间间隔跳过了（时间大概20多秒），
                        {
                            List<ModalWeightInfoTable> UnSendList = new List<ModalWeightInfoTable>();
                            dal.QueryData(tab: 0, ref UnSendList);

                            if (UnSendList == null || UnSendList.Count == 0)
                            {
                                return;
                            }

                            ModalWeightInfoTable moWeiht = UnSendList[0];

                            string str = Regex.Match(moWeiht.BodyWeight, @"([0-9].[0-9]{1,})").Value.Replace(".", "");
                            recStr = tcputility.SendAndReceive(String.Format("D{0}{1}\n", str.PadRight(4, '0'), new Random().Next(0, 9)));

                            if (recStr.Contains("ACK-Auto Data Received")) //如果发送成功，记录下来
                            {
                                LastCount = CurrentCount;

                                this.Dispatcher.Invoke(() =>
                                {
                                    dal.UpData(moWeiht.Number, 1);

                                    List<ModalWeightInfoTable> templist = new List<ModalWeightInfoTable>();
                                    dal.QueryData(ref templist);
                                    weightDtaGrid.ItemsSource = null;
                                    weightDtaGrid.ItemsSource = templist;
                                    WeightList.Clear();
                                    WeightList = templist;

                                    SpurtList.Add(new ModalSpurtCodeInfo
                                    {
                                        Counter = CurrentCount + 1,
                                        SpurtCode = str
                                    });
                                    SpurtDtaGrid.ItemsSource = null;
                                    SpurtDtaGrid.ItemsSource = SpurtList;
                                });

                            }
                            else
                            {
                                AutoDeleteMessageBox Auto = new AutoDeleteMessageBox(); // 自动关闭窗口
                                System.Windows.Forms.MessageBox.Show("发送喷码值失败！", "MessageBox");
                                return;
                            }
                        }
                        timerBool = true;
                    }                   
                }
            }           
        }
       

        private void Receive_Weight(Object sender, tcpCommEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ModalWeightInfoTable weightinfo = new ModalWeightInfoTable();
                weightDtaGrid.ItemsSource = null;
                CacheRece = e.ReceiveMessage;

                weightinfo.Number = ++number;
                weightinfo.BodyWeight = CacheRece;
                weightinfo.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                weightinfo.Tab = 0;
                //WeightList.Add(weightinfo);
                //weightDtaGrid.ItemsSource = null;
                //weightDtaGrid.ItemsSource = WeightList;
                dal.AddData(weightinfo);
                List<ModalWeightInfoTable> templist = new List<ModalWeightInfoTable>();
                dal.QueryData(ref templist);
                weightDtaGrid.ItemsSource = null;
                weightDtaGrid.ItemsSource = templist;
                WeightList.Clear();
                WeightList = templist;
            });
        }


        private void server_ClientConnected(object sender, tcpCommStatusEventArgs e)
        {
            if (e.Tcpstatus == (Int32)tcpStatus.Connection)
            {
                JM.Util.LogManager.ComLogger.Debug("客户端状态 {0} 已连接.", e.Tcpstatus);
                this.Dispatcher.Invoke(() =>
                {
                    txtNetStatus.Text = "已连接";
                });
            }
            else if (e.Tcpstatus == (Int32)tcpStatus.DisConnection)
            {
                JM.Util.LogManager.ComLogger.Debug("客户端状态 {0} 断开.", e.Tcpstatus.ToString());
                this.Dispatcher.Invoke(() =>
                {
                    txtNetStatus.Text = "已断开";
                });
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e) //查询按键对应代码
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                //if (datePickerStart.SelectedDate == null || datePickerEnd.SelectedDate == null)
                //{
                //    AutoDeleteMessageBox Auto = new AutoDeleteMessageBox(); // 自动关闭窗口
                //    System.Windows.Forms.MessageBox.Show("输入信息不完全！", "MessageBox");
                //    return;
                //}
                List<ModalWeightInfoTable> templist = new List<ModalWeightInfoTable>();
                dal.QueryData(ref templist);
                weightDtaGrid.ItemsSource = null;
                weightDtaGrid.ItemsSource = templist;
                WeightList.Clear();
                WeightList = templist;
            }
            else
            {
                if (datePickerStart.SelectedDate == null ^ datePickerEnd.SelectedDate == null)
                {
                    AutoDeleteMessageBox Auto = new AutoDeleteMessageBox(); // 自动关闭窗口
                    System.Windows.Forms.MessageBox.Show("输入信息不完全！", "MessageBox");
                    return;
                }
            }
            List<ModalWeightInfoTable> MoList = new List<ModalWeightInfoTable>();
            if (string.IsNullOrEmpty(textBox.Text)&& (datePickerStart.SelectedDate != null || datePickerEnd.SelectedDate != null)) // 按日期查询
            {          
                string time1 = datePickerStart.SelectedDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                string time2 = datePickerEnd.SelectedDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
                dal.QueryData(time1, time2, ref MoList);
                WeightList.Clear();
                WeightList.AddRange(MoList);
            }           
            if (!string.IsNullOrEmpty(textBox.Text))  //按条码查询
            {
                if (datePickerStart.SelectedDate == null && datePickerEnd.SelectedDate == null)
                {                 
                    dal.QueryData(Int32.Parse(textBox.Text), ref MoList);
                    WeightList.Clear();
                    WeightList.AddRange(MoList);
                    weightDtaGrid.ItemsSource = null;
                    weightDtaGrid.ItemsSource = WeightList;
                }
                else
                {  if ((datePickerStart.SelectedDate != null && datePickerEnd.SelectedDate != null))
                        System.Windows.Forms.MessageBox.Show("条码查询和日期查询无法同时进行");
                }
            }
           
        }

   
        private void btnExport_Click(object sender, RoutedEventArgs e)  //导出
        {

            BllExcel.Instance.DetectExport(WeightList);
            ////选取文件
            //Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            //sfd.Filter = "Txt文本文档(*.txt)|*.txt";
            //sfd.FileName = "";
            //sfd.AddExtension = true;//自动添加扩展名
            //string strFileName;
            //if (sfd.ShowDialog() == true)
            //{
            //    strFileName = sfd.FileName;
            //    FileStream fs = new FileStream(strFileName, FileMode.CreateNew);
            //    StreamWriter sw = new StreamWriter(fs);
            //    //开始写入
            //    sw.Write("Number\tBodyWeight\tTime\tVehicleCode\r\n");
            //    string txt = "";
            //    for (int i = 0; i < CodeScanList.Count; i++)
            //    {
            //        txt += CodeScanList[i].ToString();
            //    }
            //    sw.Write(txt);
            //    //清空缓冲区
            //    sw.Flush();
            //    //关闭流
            //    sw.Close();
            //    fs.Close();
            //}
            //else
            //{
            //    return;
            //}
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("确认要清除所有吗？","警告！",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WeightList.Clear();
                if( dal.DeleteData() >0)
                {
                    MessageBox.Show("删除成功！");
                }
            }
           
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            WeightList.Clear();
            WeightList.Clear();
            weightDtaGrid.ItemsSource = null;            
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (weightDtaGrid != null && weightDtaGrid.SelectedItems.Count >0)
            {
                var rowview = weightDtaGrid.SelectedItem as System.Data.DataRowView;
                if(dal.DeleteData(new ModalWeightInfoTable().Row2Model(rowview.Row)) >=1)
                {
                    MessageBox.Show("删除成功！","MessageBox");
                    List<ModalWeightInfoTable> templist = new List<ModalWeightInfoTable>();
                    dal.QueryData(ref templist);
                    weightDtaGrid.ItemsSource = null;
                    weightDtaGrid.ItemsSource = templist;
                    WeightList.Clear();
                    WeightList = templist;
                }
            }
        }       

        private void BtnStopSpurt_Click(object sender, RoutedEventArgs e)
        {
            if(timerSpurt.Enabled == true)
            {
                timerSpurt.Enabled = false;
                timerSpurt.Stop();
                btnStopSpurt.IsEnabled = false;
                btnStartSpurt.IsEnabled = true;
                textInterval.IsEnabled = true;                
            }
        }

        private void BtnStartSpurt_Click(object sender, RoutedEventArgs e)
        {
            if (timerSpurt.Enabled == false)
            {
                timerSpurt.Enabled = true;
                timerSpurt.Start();
                btnStartSpurt.IsEnabled = false;
                btnStopSpurt.IsEnabled = true;
                textInterval.IsEnabled = false;
                LastCount = 0;
            }
        }
              
        private void TextInterval_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                try
                {
                    if (!string.IsNullOrEmpty(textInterval.Text.Trim()))
                    {
                        timerSpurt.Interval = Int32.Parse(textInterval.Text.Trim());
                        MessageBox.Show($"当前时间间隔为：{timerSpurt.Interval}");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Error : {ex.Message}","您输入的内容有误！");
                }
            }
        }

        private void TextCount_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                try
                {
                    if (!string.IsNullOrEmpty(textCount.Text.Trim()))
                    {
                        Int64 sendCount = Convert.ToInt64(textCount.Text.Trim());
                        string recStr = tcputility.SendAndReceive(String.Format("SET_COUNTER_INFO 1 {0} 1\n", sendCount));
                        if (recStr.Contains("ACK-SET_COUNTER_INFO Successful"))
                        {
                            MessageBox.Show("设置喷码计数成功！");
                        }
                        else
                        {
                            MessageBox.Show("设置喷码计数失败！");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error : {ex.Message}", "您输入的内容有误！");
                }
            }
        }
    }
}   
