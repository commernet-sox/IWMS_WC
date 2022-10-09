using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.APILog
{
    public class NLogUtil
    {
        public static void WriteDebug(string message)
        {
            NLogImpl log = new NLogImpl();
            log.WriteDebug(message);
        }

        public static void WriteError(string message)
        {
            NLogImpl log = new NLogImpl();
            log.WriteError(message);
        }

        public static void WriteFatal(string message)
        {
            NLogImpl log = new NLogImpl();
            log.WriteFatal(message);
        }

        public static void WriteInfo(string message)
        {
            NLogImpl log = new NLogImpl();
            log.WriteInfo(message);
        }

        public static void WriteTrace(string message)
        {
            NLogImpl log = new NLogImpl();
            log.WriteTrace(message);
        }
    }

    internal class NLogImpl 
    {
        private Logger _logger = null;
        internal NLogImpl()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void WriteDebug(string message)
        {
            _logger.Debug(message);
        }

        public void WriteError(string message)
        {
            _logger.Error(message);
        }

        public void WriteFatal(string message)
        {
            _logger.Fatal(message);
        }

        public void WriteInfo(string message)
        {
            _logger.Info(message);
        }

        public void WriteTrace(string message)
        {
            _logger.Trace(message);
        }
    }
}
