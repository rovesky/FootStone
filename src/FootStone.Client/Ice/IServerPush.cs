using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Client
{
    public interface IServerPush
    {
        string GetFacet();

        void setSessionPushI(SessionPushI sessionPushI);
        void setAccount(string account);
    }
}
