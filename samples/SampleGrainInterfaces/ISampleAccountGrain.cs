using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleGrainInterfaces
{
    public interface ISampleAccountGrain : IAccount, IGrainWithStringKey
    {
        Task Test(string t);
    }
}
