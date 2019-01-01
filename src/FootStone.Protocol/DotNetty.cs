using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Protocol
{
    [Serializable]
    public class MsgHandShakeRequest
    {
        public string playerId;
    }

    [Serializable]
    public class MsgHandShakeResponse
    {
        public string channelId;
    }
}
