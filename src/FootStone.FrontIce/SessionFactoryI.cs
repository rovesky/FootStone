// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FootStone.FrontIce
{

    public class SessionFactoryI : ISessionFactoryDisp_
    {
        private string serverName;
        private List<Type> facets;

        private Dictionary<string, SessionI> sessions = new Dictionary<string, SessionI>();
        

        public SessionFactoryI(string name, List<Type> facets)
        {
            this.serverName = name;
            this.facets = facets;          
        }

        public override ISessionPrx CreateSession(string account, string password, Current current = null)
        {
            var logger = current.adapter.getCommunicator().getLogger();

            var sessionI = new SessionI(account);
            var proxy = ISessionPrxHelper.uncheckedCast(current.adapter.addWithUUID(sessionI));

         //   var proxy = ISessionPrxHelper.uncheckedCast(current.adapter.addWithUUID(new FSInterceptor(sessionI, logger)));
            //Ìí¼Ófacet
            foreach (var facetType in facets)
            {
                var servant = (IServantBase)Activator.CreateInstance(facetType);
                servant.setSessionI(sessionI);
                //  current.adapter.addFacet((Ice.Object)servant, proxy.ice_getIdentity(), servant.GetFacet());
                current.adapter.addFacet(new FSInterceptor((Ice.Object)servant,logger), proxy.ice_getIdentity(), servant.GetFacet());
            }

            // Never close this connection from the client and turn on heartbeats with a timeout of 30s
            current.con.setACM(30, Ice.ACMClose.CloseOff, Ice.ACMHeartbeat.HeartbeatAlways);
            current.con.setCloseCallback(_ =>
                {
                    DestroySession(account,proxy.ice_getIdentity(), current);                 
                });

            if (sessions.ContainsKey(account))
            {
                sessions.Remove(account);
            }
            sessions.Add(account, sessionI);
            logger.print($"Create session :{account},sessions count:{sessions.Count}"  );
            return proxy;
        }
     

        public override void Shutdown(Ice.Current current)
        {        
            current.adapter.getCommunicator().shutdown();

            var logger = current.adapter.getCommunicator().getLogger();
            logger.print("Ice Shutting downed!");
        }

        private void DestroySession(string account,Identity id, Current current)
        {
            var logger = current.adapter.getCommunicator().getLogger();
            try
            {
                var allFacets = current.adapter.findAllFacets(id);
                foreach (Ice.Object e in allFacets.Values)
                {
                    IDisposable dis = e as IDisposable;
                    if (dis != null)
                    {
                        try
                        {
                            dis.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                            logger.error(ex.ToString());
                        }
                    }
                }
                current.adapter.removeAllFacets(id);

                sessions.Remove(account);
                logger.print($"The session {account}:{id.name} is now destroyed from thread {Thread.CurrentThread.ManagedThreadId},current sessions count:{sessions.Count}.");
            }
            catch (Ice.ObjectAdapterDeactivatedException)
            {
                // This method is called on shutdown of the server, in which
                // case this exception is expected.
            }
            catch (Ice.LocalException e)
            {
                logger.error(e.ToString());           
            }
            catch(System.Exception e)
            {
                logger.error(e.ToString());

            }
        }
    }

}