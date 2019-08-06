using JM.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JM.CARRIER.TcpComm
{
    public class tcpUtility
    {
        private byte[] recvData = new byte[1024];
        private string _ip ;
        private int _port;

        public tcpUtility(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }


        public string SendAndReceive(string sendData)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string str = "";
            try
            {
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(_ip), _port);
                clientSocket.Blocking = true;
                clientSocket.Connect(epServer);
                
                byte[] SendData = Encoding.Default.GetBytes(sendData);
                int ret = clientSocket.Send(SendData);
                clientSocket.ReceiveTimeout = 2000;
                while (ret > 0)
                {
                    ret = clientSocket.Receive(recvData, recvData.Length, 0);
                    if (ret > 0)
                    {
                        str += Encoding.ASCII.GetString(recvData, 0, ret);
                    }
                }
                
                return str;
            }
            catch (Exception ex)
            {
                clientSocket.Close();
                LogManager.ComLogger.Info(string.Format("tcp Utility 有错误:{0}", ex.Message));
                return str;
            }
        }
    }
}
