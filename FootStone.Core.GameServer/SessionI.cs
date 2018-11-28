using FootStone.Core.FrontServer;
using FootStone.FrontServer;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GameServer
{
    public class SessionI : SessionDisp_
    {
        public SessionI(string name)
        {
            _name = name;
            _nextId = 0;
            _destroy = false;
            _objs = new List<PlayerPrx>();

            Console.Out.WriteLine("The session " + _name + " is now created.");
        }

        public override PlayerPrx createPlayer(Current current = null)
        {
            lock (this)
            {
                if (_destroy)
                {
                    throw new Ice.ObjectNotExistException();
                }

                var hello = PlayerPrxHelper.uncheckedCast(current.adapter.addWithUUID(new PlayerI()));
                _objs.Add(hello);
                return hello;
            }
        }

        public override void destroy(Current current = null)
        {
            lock (this)
            {
                if (_destroy)
                {
                    throw new Ice.ObjectNotExistException();
                }

                _destroy = true;

                Console.Out.WriteLine("The session " + _name + " is now destroyed.");
                try
                {
                    current.adapter.remove(current.id);
                    foreach (PlayerPrx p in _objs)
                    {
                        current.adapter.remove(p.ice_getIdentity());
                    }
                }
                catch (Ice.ObjectAdapterDeactivatedException)
                {
                    // This method is called on shutdown of the server, in which
                    // case this exception is expected.
                }
            }

            _objs.Clear();
        }

        public override string getName(Current current = null)
        {
            return _name;
        }


        private string _name;
        private int _nextId; // The per-session id of the next hello object. This is used for tracing purposes.
        private List<PlayerPrx> _objs; // List of per-session allocated hello objects.
        private bool _destroy;
    }

       
}
