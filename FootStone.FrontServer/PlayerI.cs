using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.FrontServer
{
    public class PlayerI : PlayerDisp_
    {
        public async override Task<PlayerInfo> getPlayerInfoAsync(string playerId, Current current = null)
        {
            try
            {
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.NewGuid());
                var playerInfo = await player.GatComponentMaster();               
                Console.WriteLine(playerInfo.Name);
                return playerInfo;
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
            }
        }
    }
}
