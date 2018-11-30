using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    public class PlayerI : PlayerDisp_
    {
        private string serverName;
        public PlayerI(string serverName)
        {
            this.serverName = serverName;
        }
        public async override Task<PlayerInfo> getPlayerInfoAsync(string playerId, Current current = null)
        {
            try
            {
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(playerId));
                var playerInfo = await player.GetPlayerInfoAsync();               
                Console.Error.WriteLine("----------------" + serverName+"."+playerInfo.name+"---------------------");
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
