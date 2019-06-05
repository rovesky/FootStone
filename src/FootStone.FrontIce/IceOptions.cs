using System;
using System.Collections.Generic;

namespace FootStone.FrontIce
{
    public class IceOptions
    {
        public string ConfigFile { get; set; }

        public List<Type> FacetTypes = new List<Type>();

        public Ice.Logger Logger { get; set; }
    }
}