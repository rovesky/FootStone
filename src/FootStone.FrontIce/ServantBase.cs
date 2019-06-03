using System;

namespace FootStone.FrontIce
{
    public abstract class ServantBase : IServantBase
    {

        private SessionI sessionI;

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public abstract string GetFacet();

        public void setSessionI(SessionI sessionI)
        {
            this.sessionI = sessionI;
        }

        public string Account
        {
            get
            {
                return sessionI.Get<string>("account");
            }

            set
            {
                 sessionI.Bind<string>("account",value);
            }
        }
    }
}
