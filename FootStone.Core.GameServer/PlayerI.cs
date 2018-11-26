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

                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(playerId));
                var playerInfo = await player.GetPlayerInfoAsync();               
               // Console.WriteLine(playerInfo.Name);
                return playerInfo;
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
            }
        }

        public async override Task setPlayerNameAsync(string playerId, string name, Current current = null)
        {
            try
            {

                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(playerId));
                // var playerInfo = await player.se
                // Console.WriteLine(playerInfo.Name);
                await player.setPlayerName(name);
              }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
