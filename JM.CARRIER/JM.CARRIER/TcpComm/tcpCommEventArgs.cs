using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.CARRIER.TcpComm
{
    public class tcpCommEventArgs : EventArgs
    {
        private readonly string _SendMessage, _ReceiveMessage;

        public tcpCommEventArgs(string sendMess ,string receiveMess )
        {
            _SendMessage = sendMess;
            _ReceiveMessage = receiveMess;
        }

        public string SendMessage => _SendMessage;

        public string ReceiveMessage => _ReceiveMessage;
    }
}
