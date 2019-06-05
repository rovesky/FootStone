using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
       

    public class WorldI :WorldDisp_, IServantBase
    {
        private SessionI session;
       
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();      

        public string GetFacet()
        {
            return "world";
        }

        public void setSessionI(SessionI sessionI)
        {
            this.session = sessionI;
        }


        public void Dispose()
        {

        }


        public async override Task<List<ServerInfo>> GetServerListRequestAsync(Current current = null)
        {
            var worldGrain = Global.OrleansClient.GetGrain<IWorldGrain>("1");
            return await worldGrain.GetServerList();
        }    


        public async override Task<List<PlayerShortInfo>> GetPlayerListRequestAsync(int serverId, Current current = null)
        {
            var worldGrain = Global.OrleansClient.GetGrain<IWorldGrain>("1");     
            return await worldGrain.GetPlayerInfoShortList(session.Account,serverId);
        }     
    }
}
