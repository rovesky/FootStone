using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.GrainInterfaces
{
    public interface IAccount
    {
        Task Login(string sessionId, string account,string pwd,LoginData data);

        Task Register(string account,RegisterInfo data);
    }
}
