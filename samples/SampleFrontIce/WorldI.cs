using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using NLog;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleFrontIce
{       

    public class WorldI :IWorldDisp_, IServantBase
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        
        private Session session;
        private IClusterClient orleansClient;

        public WorldI(Session session, IClusterClient orleansClient)
        {
            this.session = session;
            this.orleansClient = orleansClient;
        }
             
        public string GetFacet()
        {
            return nameof(IWorldPrx);
        }


        public void Dispose()
        {

        }


        public async override Task<List<ServerInfo>> GetServerListRequestAsync(Current current = null)
        {
            var worldGrain = orleansClient.GetGrain<IWorldGrain>("1");
            return await worldGrain.GetServerList();
        }    


        public async override Task<List<PlayerShortInfo>> GetPlayerListRequestAsync(int serverId, Current current = null)
        {
            var worldGrain = orleansClient.GetGrain<IWorldGrain>("1");     
            return await worldGrain.GetPlayerInfoShortList(session.Account,serverId);
        }     
    }
}
