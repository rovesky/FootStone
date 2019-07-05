using System;
using System.Collections.Generic;
using System.Text;
using Ice;
using NLog;

namespace FootStone.Client
{
    public class NLoggerIce : Ice.Logger
    {
        private NLog.Logger logger;

        public string Prefix { get; set; }


        public NLoggerIce(NLog.Logger logger)
        {
            this.logger = logger;
        }

        public void print(string message)
        {
            logger.Info(message);
        }

        public void trace(string category, string message)
        {
            logger.Trace(message);
        }

        public void warning(string message)
        {
            logger.Warn(message);
        }

        public void error(string message)
        {
            logger.Error(message);
        }

        public string getPrefix()
        {
            return Prefix;

        }
        public Ice.Logger cloneWithPrefix(string prefix)
        {

            return null;
        }
    }
}
