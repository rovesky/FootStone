using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FootStone.FrontNetty
{
    public  class FrontNettyUtility
    {
        public static string  Endpoint2GameServerId(string host,int port)
        {
            return host + ":" + port;
        }

        public static EndPoint GameServerId2Endpoint(string gameServerId)
        {
            var splits = gameServerId.Split(':');
            return new IPEndPoint(IPAddress.Parse(splits[0]), int.Parse(splits[1]));
        }

    }
}
