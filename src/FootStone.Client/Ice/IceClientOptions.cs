using Ice;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Client
{
    public class IceClientOptions
    {
        public Properties Properties;

        public List<Ice.Object> PushObjects;

        public bool EnableDispatcher { get;  set; }
    }
}
