// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************
using FootStone.GrainInterfaces;
using Ice;
using IceGrid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class SessionFactoryI : ISessionFactoryDisp_
    {
        private string serverName;
        private List<Type> facets;
        private QueryPrx query;
        private AdminPrx admin;

        public SessionFactoryI(string name, List<Type> facets, Communicator communicator)
        {
            this.serverName = name;
            this.facets = facets;
            try
            {
                this.query = QueryPrxHelper.checkedCast(communicator.stringToProxy("FootStone/Query"));

                var registry = RegistryPrxHelper.checkedCast(communicator.stringToProxy("FootStone/Registry"));          

                var sessionPrx = registry.createAdminSession("foo", "bar");
                this.admin = sessionPrx.getAdmin();

            }
            catch (Ice.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            

        

        }
        public async override Task<ISessionPrx> CreateSessionAsync(string account, string password, Current current = null)

        {
            var sessionId = Util.stringToIdentity(account);
            //var sessionPrx = ISessionPrxHelper.uncheckedCast(await query.findObjectByIdAsync(sessionId));
            //if (sessionPrx != null)
            //{
            //    await sessionPrx.DestroyAsync();
            //}

            var sessionI = new SessionI(account);
            var proxy = ISessionPrxHelper.uncheckedCast(current.adapter.addWithUUID(sessionI));

            //try
            //{
            //    await admin.addObjectAsync(proxy);
            //}
            //catch (ObjectExistsException)
            //{
            //    await admin.updateObjectAsync(proxy);
            //}


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