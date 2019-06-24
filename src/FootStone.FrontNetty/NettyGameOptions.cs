using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.FrontNetty
{
    public class NettyGameOptions
    {
   
        public int Port { get; set; }
        public IRecvData Recv { get; set; }
    }
}
