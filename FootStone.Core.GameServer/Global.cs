using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace FootStone.Core.GameServer
{
    public class Global
    {

        private static Global _instance;

        /// <summary>
        /// 私有化构造函数，使得类不可通过new来创建实例
        /// </summary>
        private Global() {}

      
        public static Global Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Global();
                }
                return _instance;
            }
        }

        private  IClusterClient orleansClient;

        public IClusterClient OrleansClient
        {
            get
            {
                return orleansClient;
            }

            set
            {
                orleansClient = value;
            }
        }
    }
    
}
