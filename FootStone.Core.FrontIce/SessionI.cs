using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    public class SessionI : SessionDisp_,IServantBase
    {
         public SessionI(string name)
        {
            this.Name = name;  
            _destroy = false;       
        }

        public  override Task AddPushAsync(SessionPushPrx sessionPush, Current current = null)
        {
            SessionPushPrx = (SessionPushPrx)sessionPush.ice_fixed(current.con);
            return Task.CompletedTask;
        } 

        public override void Destroy(Current current = null)
        {
            lock (this)
            {
                if (_destroy)
                {
                    throw new Ice.ObjectNotExistException();
                }

                _destroy = true;

                Console.Out.WriteLine("The session " + Id + " is now destroyed.");
                try
                {
                    current.adapter.remove(current.id);

                    var allFacets = current.adapter.findAllFacets(current.id);
                    foreach(Ice.Object e in allFacets.Values)
                    {
                        IDisposable dis = (IDisposable)e;
                        dis.Dispose();                      
                    }
                    current.adapter.removeAllFacets(current.id);

                }
                catch (Ice.ObjectAdapterDeactivatedException)
                {
                    // This method is called on shutdown of the server, in which
                    // case this exception is expected.
                }
            }
        }

    

        public void Dispose()
        {
            
        }

  

        public string Id
        {
            get
            {
                return SessionPushPrx.ice_getIdentity().name;
            }
        }
        public SessionPushPrx SessionPushPrx { get; private set; }
        public Guid PlayerId { get; internal set; }
        public string Account { get; internal set; }

        public string Name { get; }
        private bool _destroy;

    }

       
}
