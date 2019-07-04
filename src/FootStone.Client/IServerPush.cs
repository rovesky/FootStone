using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Client
{
    interface IServerPush
    {
        string GetFacet();

        void setSessionI(SessionPushI sessionI);

    }
}
