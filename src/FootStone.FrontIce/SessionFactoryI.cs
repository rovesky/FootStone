// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************


using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;

namespace FootStone.Core.FrontIce
{
    public class SessionFactoryI : ISessionFactoryDisp_
    {
        private string serverName;
        private IEnumerable<IServantBase> servants;

        public SessionFactoryI(string name, IEnumerable<IServantBase> servants)
        {
            this.serverName = name;
            this.servants = servants;
        }
        public override ISessionPrx CreateSession(string name, string password, Ice.Current current)
        {
            var sessionI = new SessionI(name);
            var proxy = ISessionPrxHelper.uncheckedCast(current.adapter.addWithUUID(sessionI));

            foreach(var servant in servants)
            {
                servant.setSessionI(sessionI);
                current.adapter.addFacet((Ice.Object)servant, proxy.ice_getIdentity(), servant.GetFacet());
            }

            var facets = current.adapter.findAllFacets(proxy.ice_getIdentity());

            //Console.WriteLine("facets:"+facets);

           // Remove endpoints to ensure that calls are collocated-only
           // This way, if we invoke on the proxy during shutdown, the invocation fails immediately
           // without attempting to establish any connection
           var collocProxy = ISessionPrxHelper.uncheckedCast(proxy.ice_endpoints(new Ice.Endpoint[0]));

            // Never close this connection from the client and turn on heartbeats with a timeout of 30s
            current.con.setACM(30, Ice.ACMClose.CloseOff, Ice.ACMHeartbeat.HeartbeatAlways);
            current.con.setCloseCallback(_ =>
                {
                    try
                    {
                        collocProxy.Destroy();
                        Console.Out.WriteLine("Cleaned up dead client.");
                    }
                    catch (Ice.LocalException)
                    {
                        // The client already destroyed this session, or the server is shutting down
                    }
                });
            Console.Out.WriteLine("create session :" + current.con.getInfo().connectionId);
            return proxy;
        }


        public override void Shutdown(Ice.Current current)
        {
            Console.Out.WriteLine("Shutting down...");
            current.adapter.getCommunicator().shutdown();
        }
    }

}