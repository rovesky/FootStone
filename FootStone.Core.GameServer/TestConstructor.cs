using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GameServer
{
    public class BaseTestConstructor
    {
        private string a;

        protected BaseTestConstructor(string a)
        {
            this.a = a;
        }
    }


    public class DeriveTestConstructor : BaseTestConstructor
    {
   

        public  DeriveTestConstructor(string a) :base(a)
        {
            
        }
    }
}
