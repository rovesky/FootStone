using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.ProtocolNetty
{
    public enum MessageType
    {
      //  PlayerHandshake = 0x01,
        BindGameServer = 0x02,

        Ping = 0x04,
        Pong = 0x05,
      
        Data  = 0x10
    }
}
