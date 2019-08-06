using JM.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static JM.CARRIER.TcpComm.tcpConnState;

namespace JM.CARRIER.TcpComm
{
    public class Communication
    {
        private string Ip { get; set; }
        private int Port { get; set; }
        public int ConnectStatus { get => connectStatus; set => connectStatus = value; }

        private Socket clientSocket;

        private byte[] MsgBuffer = new byte[1024];

        private int connectStatus = 0;

        public event EventHandler<tcpCommEventArgs> newTcpMessage;

        public event EventHandler<tcpCommStatusEventArgs> newTcpStatus;

        System.Timers.Timer timerConn;

        public Communication(string ip, int port)
        {
            this.Ip = ip;
            this.Port = port;
            timerConn = new System.Timers.Timer(3000);
            timerConn.Enabled = false;
            //timerConn.Elapsed += delegate {
                                
            //    clientSocket.Dispose();
            //    Thread.Sleep(500);
            //    IPAddress IP = IPAddress.Parse(this.Ip);
            //    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    this.clientSocket.BeginConnect(IP, this.Port, new AsyncCallback(ConnectCallback), this.clientSocket);
            //};
        }
      

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        public void Connect()
        {
            try
            {
                connectStatus = 0;
                IPAddress IP = IPAddress.Parse(this.Ip);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.clientSocket.BeginConnect(IP, this.Port, new AsyncCallback(ConnectCallback), this.clientSocket); 
            }
            catch(Exception ex)
            {
                LogManager.ComLogger.Info(string.Format(" Socket连接错误{0}", ex.ToString()));
                SetTcpConnStatus(tcpStatus.DisConnection);
            }

        }

       
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndConnect(ar);
                connectStatus = 1;
                ReceiveData();
                SetTcpConnStatus(tcpStatus.Connection);
                //timerConn.Enabled = false;
                //timerConn.Stop();
            }
            catch (SocketException ex)
            {
                LogManager.ComLogger.Info(string.Format(" Socket连接错误{0}", ex.ToString()));
                connectStatus = -1;
                SetTcpConnStatus(tcpStatus.DisConnection);
                //if(timerConn.Enabled == false)
                //{
                //    timerConn.Enabled = true;
                //    timerConn.Start();
                //}
            }
        }

        /// <summary>
        /// 接口服务器发送的消息
        /// </summary>
        public void ReceiveData()
        {
            clientSocket.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    int Rlength = clientSocket.EndReceive(ar);
                    if (Rlength > 0)
                    {
                        byte[] data = new byte[Rlength];
                        Array.Copy(MsgBuffer, 0, data, 0, Rlength);
                        ReceiveMess(ref MsgBuffer);
                        //在此次可以对data进行按需处理
                        clientSocket.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, 0, new AsyncCallback(ReceiveCallback), this.clientSocket);
                    }
                    else
                    {
                        //timerConn.Enabled = true;
                        //timerConn.Start();
                        LogManager.ComLogger.Info(string.Format("Socket接收长度为{0}",Rlength));
                    }
                }
            }
            catch (SocketException ex)
            {
                LogManager.ComLogger.Info(string.Format("Socket接收错误{0}", ex.ToString()));
                SetTcpConnStatus(tcpStatus.ReceiveError);
            }
        }

        public bool Send(byte[] byteData)
        {
            try
            {
                //string pp = @"([0-9].[0-9]{1,5})";

                int length = byteData.Length;
                //byte[] head = BitConverter.GetBytes(length); //设置包头
                //byte[] data = new byte[head.Length + byteData.Length];
                //Array.Copy(head, data, head.Length);
                //Array.Copy(byteData, 0, data, head.Length, byteData.Length);
                this.clientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), this.clientSocket);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                if (handler != null && handler.Connected)
                {
                    handler.EndSend(ar);
                }
            }
            catch (SocketException ex)
            {
                LogManager.ComLogger.Info(string.Format("Socket发送错误{0}", ex.ToString()));
                SetTcpConnStatus(tcpStatus.SendError);
            }
        }

        protected virtual void OnNewTcpMessage(tcpCommEventArgs e)
        {
            e.Raise(this, ref newTcpMessage);
        }

        protected virtual void OnNewTcpStatus(tcpCommStatusEventArgs e)
        {
            e.Raise(this, ref newTcpStatus);
        }


        private void ReceiveMess(ref byte[] messRec)
        {
            if (messRec != null)
            {
                string pattern = @"Net(.*?kg)";

                string strRead = Encoding.UTF8.GetString(MsgBuffer, 0, MsgBuffer.Length).Replace("\0", "");
                string[] strs = strRead.Split('\n');
                string re = "";

                foreach (var a in strs)
                {
                    re = Regex.Match(a, pattern).Value;
                    if (Regex.IsMatch(a, pattern))
                        break;
                }
                if (string.IsNullOrEmpty(re))
                {
                    return;
                }

                tcpCommEventArgs e = new tcpCommEventArgs("", re);
                OnNewTcpMessage(e);
            }

        }

        private void SetTcpConnStatus(tcpStatus state)
        {
            tcpCommStatusEventArgs e = new tcpCommStatusEventArgs((Int32)state);
            OnNewTcpStatus(e);
        }
    }
}
