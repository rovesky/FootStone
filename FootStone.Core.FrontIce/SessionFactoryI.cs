// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************


using FootStone.GrainInterfaces;
using Ice;
using System;

namespace FootStone.Core.FrontIce
{
    public class SessionFactoryI : SessionFactoryDisp_
    {
        private string serverName;

        public SessionFactoryI(string name)
        {
            this.serverName = name;
        }
        public override SessionPrx CreateSession(string name, string password, Ice.Current current)
        {
            var session = new SessionI(name);
            var proxy = SessionPrxHelper.uncheckedCast(current.adapter.addWithUUID(session));
            current.adapter.addFacet(new AccountI(session), proxy.ice_getIdentity(), "account");
            current.adapter.addFacet(new PlayerI(session), proxy.ice_getIdentity(), "player");


            //
            // Remove endpoints to ensure that calls are collocated-only
            // This way, if we invoke on the proxy during shutdown, the invocation fails immediately
            // without attempting to establish any connection
            //
            var collocProxy = SessionPrxHelper.uncheckedCast(proxy.ice_endpoints(new Ice.Endpoint[0]));

            //
            // Never close this connection from the client and turn on heartbeats with a timeout of 30s
            //
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
            return proxy;
        }
      

        public override void Shutdown(Ice.Current current)
        {
            Console.Out.WriteLine("Shutting down...");
            current.adapter.getCommunicator().shutdown();
        }
    }

}