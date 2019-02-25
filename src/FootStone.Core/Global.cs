﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace FootStone.Core
{
    public static class Global
    {
        static public IClusterClient OrleansClient
        {
            get;
            set;
        }
        public static string[] MainArgs { get; set; }
        public static int ZoneMsgCount { get; internal set; }
    }
    
}
