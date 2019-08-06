using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.CARRIER.TcpComm
{
    public static class tcpConnState
    {
        internal enum tcpStatus : Int32
        {
            DisConnection  ,
            Connection ,
            SendError ,
            ReceiveError
        }
    }
}
