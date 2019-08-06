using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.CARRIER.TcpComm
{
    public class tcpCommStatusEventArgs : EventArgs
    {
        private readonly Int32 _tcpstatus;

        public tcpCommStatusEventArgs(Int32 tcpStatus)
        {
            this._tcpstatus = tcpStatus;
        }

        public int Tcpstatus => _tcpstatus;
    }
}
