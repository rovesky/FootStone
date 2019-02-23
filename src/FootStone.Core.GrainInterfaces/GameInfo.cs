using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GrainInterfaces
{
    public class GameInfo
    {
        public long id;
        public string name;
        public bool enabled;

        public GameInfo(long id)
        {
            this.id = id;
        }
    }
}
