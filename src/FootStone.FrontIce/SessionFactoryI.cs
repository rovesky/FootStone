// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class SessionFactoryI : ISessionFactoryDisp_
    {
        private string serverName;
        private List<Type> facets;

        public SessionFactoryI(string name, List<Type> facets)
        {
            this.serverName = name;
            this.facets = facets;          
        }

        public async override Task<ISessionPrx> CreateSessionAsync(string account, string password, Current current = null)
        {         
            var sessionI = new SessionI(account);
            var proxy = ISessionPrxHelper.uncheckedCast(current.adapter.addWithUUID(sessionI));
                       
            //Ìí¼Ófacet
            foreach (var facetType in facets)
            {
                var servant = (IServantBase)Activator.CreateInstance(facetType);
                servant.setSessionI(sessionI);
                current.adapter.addFacet((Ice.Object)servant, proxy.ice_getIdentity(), servant.GetFacet());
            }


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

            var logger = current.adapter.getCommunicator().getLogger();
            logger.print("create session :" + current.con.getInfo().connectionId);
            return proxy;
        }

     

        public override void Shutdown(Ice.Current current)
        {        
            current.adapter.getCommunicator().shutdown();

            var logger = current.adapter.getCommunicator().getLogger();
            logger.print("Ice Shutting downed!");
        }
    }

}