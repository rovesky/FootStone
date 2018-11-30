using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{


    /// <summary>
    /// Observer class that implements the observer interface. Need to pass a grain reference to an instance of this class to subscribe for updates.
    /// </summary>
    class PlayerObserver : IPlayerObserver
    {
        readonly PlayerPushPrx playerPush;
        public PlayerObserver(PlayerPushPrx playerPush)
        {
            this.playerPush = playerPush;
        }
        public void HpChanged(int hp)
        {
            playerPush.begin_hpChanged(hp);
        }
        
    }

    public class PlayerI : PlayerDisp_
    {
        private string serverName;

        private Dictionary<string, PlayerObserver> _clients = new Dictionary<string, PlayerObserver>();
        public PlayerI(string serverName)
        {
            this.serverName = serverName;
        }

        public async override Task addPushAsync(string playerId, PlayerPushPrx playerPush, Current current = null)
        {

            Console.Out.WriteLine("adding client '" + Ice.Util.identityToString(playerPush.ice_getIdentity()) + "'");
            PlayerPushPrx push = (PlayerPushPrx)playerPush.ice_fixed(current.con).ice_oneway();
            var watcher = new PlayerObserver(push);
            lock (this)
            {
                _clients.Add(playerId, watcher);
            }
            var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(playerId));
            await player.SubscribeForPlayerUpdates(
                await Global.Instance.OrleansClient.CreateObjectReference<IPlayerObserver>(watcher)
            );

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
