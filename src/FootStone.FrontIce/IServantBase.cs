using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public interface IServantBase : IDisposable
    {
       // void Destroy();

        string GetFacet();

        void setSessionI(SessionI sessionI);
      
    }
}
