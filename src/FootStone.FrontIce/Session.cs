using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;

namespace FootStone.FrontIce
{
    public class Session : ISessionDisp_,IDisposable
    {
        public Session(string account)
        {
            this.Account = account;  
        }

        public  override void AddPush(ISessionPushPrx sessionPush, Current current = null)
        {
            SessionPushPrx = (ISessionPushPrx)sessionPush.ice_fixed(current.con).ice_oneway();
        }

        public override void Destroy(Current current = null)
        {
            SessionPushPrx.ice_getConnection().close(ConnectionClose.Forcefully);         
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

        public void Dispose()
        {
            if(SessionPushPrx!= null)
                SessionPushPrx.begin_SessionDestroyed();
        }

        public T UncheckedCastPush<T>(Func<ObjectPrx, string, T> uncheckedCast) where T : ObjectPrx
        {
            return uncheckedCast(SessionPushPrx, typeof(T).Name);
        }


        public string Account { set; get; }
        public Guid   PlayerId { set; get; }

        public ISessionPushPrx SessionPushPrx { get; private set; }       

        private Dictionary<String, object> attributes = new Dictionary<string, object>();

    }
       
}
