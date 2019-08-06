using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JM.CARRIER.TcpComm
{
    public static class EventArgsExtensions
    {
        public static void Raise<TEventArgs>(this TEventArgs e, 
            Object sender, ref EventHandler<TEventArgs> eventDelegate)
        {
            EventHandler<TEventArgs> temp = Volatile.Read(ref eventDelegate);

            if(temp!= null)
            {
                temp(sender, e);
            }
        }
    }
}
