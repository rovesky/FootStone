using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IAccountGrain : IGrainWithStringKey
    {
        Task LoginRequest(LoginInfo info);

        Task RegisterRequest(RegisterInfo info);
    }
}
