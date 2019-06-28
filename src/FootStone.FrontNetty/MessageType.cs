using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.FrontNetty
{
    public enum MessageType
    {
        PlayerHandshake = 1,
        PlayerBindGame = 2,
        SiloHandshake = 3,
        Data  = 10
    }
}
