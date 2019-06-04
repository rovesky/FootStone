using System;
using System.Collections.Generic;
using System.Text;
using Ice;

namespace FootStone.FrontIce
{
    public class MyLoggerI : Ice.Logger
    {
        public void print(string message)
        {
            Console.WriteLine("MyLoggerI");
        }
        public void trace(string category, string message)
        {
            Console.WriteLine("MyLoggerI");
        }
        public void warning(string message) {
            Console.WriteLine("MyLoggerI");
        }
        public void error(string message) {
            Console.WriteLine("MyLoggerI");
        }

        public string getPrefix() {
            return "";

        }
        public Logger cloneWithPrefix(string prefix) {

            return null;
        }

       
    }


    public class MyLoggerPluginFactoryI : Ice.PluginFactory
    {
        public Ice.Plugin create(Ice.Communicator communicator, string name, string[] args)
        {
            Ice.Logger logger = new MyLoggerI();
            return new Ice.LoggerPlugin(communicator, logger);
        }
    }
}
