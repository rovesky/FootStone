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

        public GameState()
        {

        }
        public GameState(long id, string name, bool enabled) : this(id)
        {
            this.name = name;
            this.enabled = enabled;
        }

        public GameState(long id)
        {
            this.id = id;
        }
    }
}
