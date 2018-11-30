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

        private Dictionary<string, PlayerObserver> _clients = new Dictionary<string, PlayerObserver>();
        public PlayerI()
        {
            
        }

        public async override Task AddPushAsync(PlayerPushPrx playerPush, Current current = null)
        {
          

            Console.Out.WriteLine("adding client '" + Ice.Util.identityToString(playerPush.ice_getIdentity()) + "'");
            PlayerPushPrx push = (PlayerPushPrx)playerPush.ice_fixed(current.con).ice_oneway();
            var watcher = new PlayerObserver(push);
            lock (this)
            {
                _clients.Add(current.ctx["playerId"], watcher);
            }
            var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(current.ctx["playerId"]));
            await player.SubscribeForPlayerUpdates(
                await Global.Instance.OrleansClient.CreateObjectReference<IPlayerObserver>(watcher)
            );

        }

        public async override Task<PlayerInfo> GetPlayerInfoAsync(Current current = null)
        {
            try
            {
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(current.ctx["playerId"]));
                var playerInfo = await player.GetPlayerInfo();               
                Console.Error.WriteLine("----------------" +playerInfo.name+"---------------------");
                return playerInfo;
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
            }
        }

        public async override Task SetPlayerNameAsync(string name, Current current = null)
        {
            try
            {

                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(current.ctx["playerId"]));
             
                await player.SetPlayerName(name);
              }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
