using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.Grains
{
    public class AccountState
    {
        public string account;
        public string password;
        public string token;
       // public List<>
    }

    [StorageProvider(ProviderName = "memory1")]
    public class AccountGrain : Grain<AccountState>, IAccountGrain
    {
        public Task LoginRequest(LoginInfo info)
        {
            if(State.account == null)
            {
                throw new AccountException("account is not registered!");
            }
            if(!(State.account.Equals(info.account)
                && State.password.Equals(info.password)))
            {
                throw new AccountException("account or password is not  valid!");
            }
            State.token = Guid.NewGuid().ToString();
            return WriteStateAsync();
        }

        public Task RegisterRequest(RegisterInfo info)
        {
            if (State.account != null)
            {
                throw new AccountException("account is registered!");
            }

            State.account = info.account;
            State.password = info.password;

            return WriteStateAsync();
        }
    }
}
