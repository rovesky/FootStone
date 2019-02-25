using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GrainInterfaces
{
    public class GameState
    {
        public long id;
        public string name;
        public bool enabled;
     

        public GameState(long id)
        {
            this.id = id;
        }
    }
}
