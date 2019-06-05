using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class SessionI : ISessionDisp_
    {
         public SessionI(string account)
        {
            this.Account = account;  
            _destroy = false;       
        }

        public  override Task AddPushAsync(ISessionPushPrx sessionPush, Current current = null)
        {
            SessionPushPrx = (ISessionPushPrx)sessionPush.ice_fixed(current.con);
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

                var logger = current.adapter.getCommunicator().getLogger();
                try
                {
                    current.adapter.remove(current.id);

                    var allFacets = current.adapter.findAllFacets(current.id);
                    foreach(Ice.Object e in allFacets.Values)
                    {
                        IServantBase dis = (IServantBase)e;
                        try
                        {
                            dis.Destroy();
                        }
                        catch(System.Exception ex)
                        {
                            logger.error(ex.ToString());
                        }
                    }
                    current.adapter.removeAllFacets(current.id);
               
                    logger.print("The session " + Id + " is now destroyed.");
                }
                catch (Ice.ObjectAdapterDeactivatedException)
                {
                    // This method is called on shutdown of the server, in which
                    // case this exception is expected.
                }
            }
        }


        public string Id
        {
            get
            {
                if (SessionPushPrx == null)
                    return "";
                return SessionPushPrx.ice_getIdentity().name;
            }
        }

        public T Get<T>(string key)
        {
            if (!attributes.ContainsKey(key))
            {
                return default(T);
            }
            return (T)attributes[key];
        }

        public void Bind<T>(string key,T value)
        {
             attributes.Add(key, value);
        }

        public void Unbind(string key)
        {
            attributes.Remove(key);
        }

        public string Account { set; get; }
        public Guid   PlayerId { set; get; }

        public ISessionPushPrx SessionPushPrx { get; private set; }
             
     
        private bool _destroy;
        private Dictionary<String, object> attributes = new Dictionary<string, object>();

    }

       
}
