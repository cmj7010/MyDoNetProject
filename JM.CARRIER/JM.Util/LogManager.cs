using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.Util
{
    public class LogManager
    {
        /// <summary>
        /// 常规日志
        /// </summary>
        public static readonly Logger Logger = NLog.LogManager.GetLogger("");
        /// <summary>
        /// 通讯日志，单独写在另一个文件
        /// </summary>
        public static readonly Logger ComLogger = NLog.LogManager.GetLogger("通讯");
    }
}
