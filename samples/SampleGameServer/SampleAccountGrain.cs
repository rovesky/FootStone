using FootStone.Core;
using FootStone.GrainInterfaces;
using SampleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public class SampleAccountGrain : AccountGrain, ISampleAccountGrain
    {
        public Task Login(string sessionId, string account, string pwd, LoginData data)
        {
            throw new NotImplementedException();
        }

        public Task Register(string account, RegisterInfo data)
        {
            throw new NotImplementedException();
        }

        public Task Test(string t)
        {
            throw new NotImplementedException();
        }
    }
}
