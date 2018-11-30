using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    public class SessionI : SessionDisp_
    {

        private Dictionary<string, PlayerObserver> _clients = new Dictionary<string, PlayerObserver>();


        public SessionI(string name)
        {
            _name = name;
            //_nextId = 0;
            //_destroy = false;
            //_objs = new List<PlayerPrx>();

            Console.Out.WriteLine("The session  is now created in server:"+name);
        }

        public override Task PingAsync(Current current = null)
        {
            throw new NotImplementedException();
        }

        public async override Task AddPushAsync(SessionPushPrx sessionPush, Current current = null)
        {
            SessionPushPrx sessionPushPrx = (SessionPushPrx)sessionPush.ice_fixed(current.con).ice_oneway();
            var PlayerPushPrx = PlayerPushPrxHelper.uncheckedCast(sessionPushPrx, "playerPush");
            var watcher = new PlayerObserver(PlayerPushPrx);
            lock (this)
            {
                _clients.Add(current.ctx["playerId"], watcher);
            }
            var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(Guid.Parse(current.ctx["playerId"]));
            await player.SubscribeForPlayerUpdates(
                await Global.Instance.OrleansClient.CreateObjectReference<IPlayerObserver>(watcher)
            );
        }

        private string _name;
        //private int _nextId; // The per-session id of the next hello object. This is used for tracing purposes.
        //private List<PlayerPrx> _objs; // List of per-session allocated hello objects.
        //private bool _destroy;
    }

       
}
