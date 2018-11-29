using Ice;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GameServer
{
    public class FootStoneLocator : ServantLocator
    {
        public void deactivate(string category)
        {
            throw new NotImplementedException();
        }

        public void finished(Current curr, Ice.Object servant, object cookie)
        {
            throw new NotImplementedException();
        }

        public Ice.Object locate(Current curr, out object cookie)
        {
           
            throw new NotImplementedException();
        }
    }
}
