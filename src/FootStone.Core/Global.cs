using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;

namespace FootStone.Core
{
    public static class Global
    {
        static public IClusterClient OrleansClient
        {
            get
            {
                return FSHost == null ? null : FSHost.Services.GetRequiredService<IClusterClient>();
            }

        }

        static public IFSHost  FSHost { get; set; }

     //   public static string[] MainArgs { get; set; }
       // public static int ZoneMsgCount { get; internal set; }
    }
    
}
